using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace IntelliTect.Coalesce.AuditLogging.Internal;

internal sealed class AuditInterceptor<TAuditLog> : SaveChangesInterceptor
    where TAuditLog : class, IAuditLog
{
    private readonly AuditOptions _options;

    private CoalesceAudit? _audit;

    private IAuditLogDbContext<TAuditLog> GetContext(DbContextEventData data)
        => (IAuditLogDbContext<TAuditLog>)(data.Context ?? throw new InvalidOperationException("DbContext unavailable."));

    #region SavingChanges

    private static readonly FieldInfo m_auditConfiguration = typeof(Audit)
        .GetField("_configuration", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
        ?? throw new NotSupportedException("Unknown version of Z.EntityFramework.Plus - field Audit._configuration not found.");

    private void AttachConfig(Audit audit)
    {
        var configLazy = new Lazy<AuditConfiguration>(
            () => _options.AuditConfiguration ?? AuditManager.DefaultConfiguration.Clone());

        if (_options.AuditConfiguration is not null)
        {
            // Force the lazy to evaluate so that Audit.CurrentOrDefaultConfiguration 
            // will return our instance instead of AuditManager.DefaultConfiguration.
            var _ = configLazy.Value;
        }

        m_auditConfiguration.SetValue(audit, configLazy);
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        _audit = null;
        if (GetContext(eventData).SuppressAudit) return result;
        
        _audit = new CoalesceAudit();
        AttachConfig(_audit);
        _audit.PreSaveChanges(eventData.Context);

        if (_options.PropertyDescriptions.HasFlag(PropertyDescriptionMode.FkListText))
        {
            await _audit.PopulateOldDescriptions(eventData.Context!, true);
        }

        return result;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        _audit = null;
        if (GetContext(eventData).SuppressAudit) return result;

        _audit = new CoalesceAudit();
        AttachConfig(_audit);
        _audit.PreSaveChanges(eventData.Context);

        if (_options.PropertyDescriptions.HasFlag(PropertyDescriptionMode.FkListText))
        {
#pragma warning disable CS4014 // Executes synchonously when async: false
            _audit.PopulateOldDescriptions(eventData.Context!, async: false);
#pragma warning restore CS4014
        }

        return result;
    }

    #endregion

    #region SavedChanges
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (_audit is null) return result;

#pragma warning disable CS4014 // Executes synchonously when async: false
        SaveAudit(eventData.Context!, _audit, async: false);
#pragma warning restore CS4014

        return result;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (_audit is null) return result;

        await SaveAudit(eventData.Context!, _audit, async: true);

        return result;
    }
    #endregion

    /// <summary>
    /// Cache for generated SQL. Size is limited in case there is an application whose Model
    ///  is not a singleton, e.g. dynamic model configuration for schema-based tenancy.
    /// </summary>
    private static readonly MemoryCache _sqlCache = new MemoryCache(new MemoryCacheOptions() { SizeLimit = 4_000_000 });

    public AuditInterceptor(AuditOptions options)
    {
        _options = options;
    }

    private string GetSqlServerSql(IModel model)
    {
        return _sqlCache.GetOrCreate(model, entry =>
        {
            string sql = BuildSqlServerSql(model);
            entry.Size = sql.Length;
            return sql;
        })!;

        static string BuildSqlServerSql(IModel model)
        {
            var entityType = model.FindEntityType(typeof(TAuditLog))!;
            var propEntityType = model.FindEntityType(typeof(AuditLogProperty))!;

            var tableName = entityType.GetSchemaQualifiedTableName();
            var propTableName = propEntityType.GetSchemaQualifiedTableName();

            var basePropNames = typeof(IAuditLog).GetProperties().Select(p => p.Name).ToArray();
            var props = entityType.GetDeclaredProperties();
            var propsByName = props.ToLookup(p => p.Name);

            var customProps = props.ExceptBy(basePropNames, p => p.PropertyInfo?.Name);
            var mergeMatchProps = customProps.Concat(props.Where(p => p.Name is nameof(IAuditLog.State) or nameof(IAuditLog.Description)));
            var cursorProps = mergeMatchProps.Concat(props.Where(p => p.Name is nameof(IAuditLog.Date) or nameof(IAuditLog.Type) or nameof(IAuditLog.KeyValue)));

            string GetColName(string propName, IEntityType table) => GetPropColName(table
                .GetDeclaredProperties()
                .Single(p => p.Name == propName), table);

            string GetPropColName(IProperty prop, IEntityType table) => prop
                .GetColumnName(StoreObjectIdentifier.Create(table, StoreObjectType.Table)!.Value)!;

            string pkCol = GetColName(nameof(IAuditLog.Id), entityType);
            string entityTypeNameCol = GetColName(nameof(IAuditLog.Type), entityType);
            string entityKeyValueCol = GetColName(nameof(IAuditLog.KeyValue), entityType);
            string dateCol = GetColName(nameof(IAuditLog.Date), entityType);
            string stateCol = GetColName(nameof(IAuditLog.State), entityType);

            string propFkCol = GetColName(nameof(AuditLogProperty.ParentId), propEntityType);
            string propPkCol = GetColName(nameof(AuditLogProperty.Id), propEntityType);
            string propNameCol = GetColName(nameof(AuditLogProperty.PropertyName), propEntityType);
            string propOldCol = GetColName(nameof(AuditLogProperty.OldValue), propEntityType);
            string propOldDescCol = GetColName(nameof(AuditLogProperty.OldValueDescription), propEntityType);
            string propNewCol = GetColName(nameof(AuditLogProperty.NewValue), propEntityType);
            string propNewDescCol = GetColName(nameof(AuditLogProperty.NewValueDescription), propEntityType);

            return $"""
            SET NOCOUNT ON;
            SET XACT_ABORT ON;

            -- Setup iteration over each distinct [AuditLog] represented by our incoming flat data.
            DECLARE object_cursor CURSOR FOR 
            SELECT {string.Join(", ", cursorProps.Select(p => p.Name))}, Properties
            FROM OPENJSON(@MergePayload) with (   
                {string.Join(",\n    ", cursorProps.Select(p => $"{p.Name} {p.GetRelationalTypeMapping().StoreType} '$.{p.Name}'"))},
            	Properties [nvarchar](max) '$.Properties' as JSON
            );

            {string.Join(";\n    ", cursorProps.Select(p => $"DECLARE @{p.Name} {p.GetRelationalTypeMapping().StoreType}"))};
            DECLARE @Properties [nvarchar](max);

            OPEN object_cursor

            WHILE (1=1)
            BEGIN  
                -- Looping over each entity in the set of changes passed in.
            	FETCH NEXT FROM object_cursor INTO 
                    {string.Join(",\n        ", cursorProps.Select(p => $"@{p.Name}"))},
            		@Properties;

            	IF @@FETCH_STATUS <> 0  
            	BEGIN
            		BREAK;
            	END

                -- Find the most recent change record for the given entity.
                DECLARE @mostRecentAuditLogId [bigint];
                DECLARE @shouldUpdate [bit];
                SET @mostRecentAuditLogId = 0;
                SET @shouldUpdate = 0;

                SELECT TOP 1 
                    @mostRecentAuditLogId = {pkCol},
                    @shouldUpdate = (CASE WHEN 
                        -- Don't update if the type and/or key matched on NULL=NULL
                        @Type IS NOT NULL
                        AND @KeyValue IS NOT NULL
                        -- These fields must match exactly:
                        {string.Join("\n           ", mergeMatchProps.Select(p => $"AND ({GetPropColName(p, entityType)} = @{p.Name} OR ISNULL({GetPropColName(p, entityType)}, @{p.Name}) IS NULL)"))}
                        -- Date of the most recent record must be within X amount of time, 
                        AND DATEDIFF(second, {dateCol}, @Date) < @MergeWindowSeconds
                        -- and the incoming record must have a date equal or after the existing record
                        AND @Date >= {dateCol}
                        -- The existing AuditLog record has the same set of properties as the incoming record
                        AND (
                        		SELECT TOP 1 ( SELECT {propNameCol} + ','
                        		FROM {propTableName} 
                        		WHERE {propTableName}.{propFkCol} = {tableName}.{pkCol}
                                ORDER BY {propNameCol} ASC
                                FOR XML PATH('') ) as PropNames
                        	) = (
                        		SELECT TOP 1 ( SELECT [PropertyName] + ','
                        		FROM OPENJSON(@Properties) WITH (
            						PropertyName [nvarchar](max) '$.PropertyName'
            					)
                                ORDER BY PropertyName ASC
                                FOR XML PATH('') ) as PropNames
                        	)
                        THEN 1 
                        ELSE 0 
                    END)
                FROM {tableName}
                WHERE {entityTypeNameCol} = @Type AND {entityKeyValueCol} = @KeyValue
                ORDER BY {pkCol} DESC;

                IF @mostRecentAuditLogId > 0 AND @shouldUpdate = 1
                    BEGIN
                        -- We found an existing record for the given Type and KeyValue and it is a candidate for being updated.

                        UPDATE {tableName}
                        SET {dateCol} = @Date
                        WHERE {pkCol} = @mostRecentAuditLogId;


                        MERGE {propTableName}
                        USING (
                        	SELECT PropertyName, NewValue, NewValueDescription
                        	FROM OPENJSON(@Properties) with (
            					PropertyName [nvarchar](max) '$.PropertyName',
            					OldValue [nvarchar](max) '$.OldValue',
            				    OldValueDescription [nvarchar](max) '$.OldValueDescription',
            					NewValue [nvarchar](max) '$.NewValue',
            				    NewValueDescription [nvarchar](max) '$.NewValueDescription'
            				)
                        ) AS Source
                        ON {propFkCol} = @mostRecentAuditLogId AND {propTableName}.{propNameCol} = Source.PropertyName
                        WHEN MATCHED THEN
                        	-- DO NOT UPDATE OldValue! It stays the same so we represent the transition from the original OldValue to the new NewValue.
                        	UPDATE SET {propNewCol} = Source.NewValue, {propNewDescCol} = Source.NewValueDescription
                        ;
                    END;
                ELSE
                    BEGIN
                        -- The existing record didn't exist, or was not a candidate for being updated. Insert a new record.

                        INSERT INTO {tableName}
                        ({string.Join(", ", cursorProps.Select(p => GetPropColName(p, entityType)))})
                        VALUES ({string.Join(", ", cursorProps.Select(p => $"@{p.Name}"))});

                        SET @mostRecentAuditLogId = SCOPE_IDENTITY();

                        INSERT INTO {propTableName}
                        ({propFkCol}, {propNameCol}, {propOldCol}, {propOldDescCol}, {propNewCol}, {propNewDescCol})
                        SELECT @mostRecentAuditLogId, PropertyName, OldValue, OldValueDescription, NewValue, NewValueDescription
                        FROM OPENJSON (@Properties) WITH (
            				PropertyName [nvarchar](max) '$.PropertyName',
            				OldValue [nvarchar](max) '$.OldValue',
            				OldValueDescription [nvarchar](max) '$.OldValueDescription',
            				NewValue [nvarchar](max) '$.NewValue',
            				NewValueDescription [nvarchar](max) '$.NewValueDescription'
            			);

                    END;
            END;

            CLOSE object_cursor;
            DEALLOCATE object_cursor;
            """;
        }
    }

    private async ValueTask SaveAudit(DbContext db, CoalesceAudit audit, bool async)
    {
        audit.PostSaveChanges();

        if (_options.PropertyDescriptions.HasFlag(PropertyDescriptionMode.FkListText))
        {
            await audit.PopulateNewDescriptions(db, async);
        }

        var serviceProvider = new EntityFrameworkServiceProvider(db);
        var operationContext = _options.OperationContextType is null 
            ? null 
            : (IAuditOperationContext<TAuditLog>)ActivatorUtilities.GetServiceOrCreateInstance(serviceProvider, _options.OperationContextType);

        var date = DateTimeOffset.Now; // This must be the exact same date for all items for the merge logic in the SQL to work properly.

        var auditLogs = audit.Entries
            // The change is only useful if there's more than one property, since one of the properties is always the primary key.
            // This happens when the only changed properties on the object are marked as excluded properties.
            .Where(e => e.Properties.Count > 1 && e.Entity is not IAuditLog && e.Entity is not AuditLogProperty)
            .Select(e =>
            {
                var entityClrType = e.Entity.GetType();
                var keyProperties = db.Model
                    .FindEntityType(entityClrType)?
                    .FindPrimaryKey()?
                    .Properties;

                var auditLog = Activator.CreateInstance<TAuditLog>();

                auditLog.Date = date;
                auditLog.State = (IntelliTect.Coalesce.AuditLogging.AuditEntryState)e.State;
                auditLog.Type = e.EntityTypeName;
                auditLog.KeyValue = keyProperties is null 
                    ? null 
                    : string.Join(";", keyProperties.Select(p => e.Entry.CurrentValues[p]));

                if (
                    _options.Descriptions.HasFlag(DescriptionMode.ListText) &&
                    ReflectionRepository.Global.GetOrAddType(entityClrType).ClassViewModel is { } cvm &&
                    // If the list text for the target is the PK,
                    // the description won't be useful as it'll just duplicate the value prop.
                    cvm.ListTextProperty is { IsPrimaryKey: false, PropertyInfo: var listTextProp }
                )
                {
                    try
                    {
                        auditLog.Description = listTextProp.GetValue(e.Entity)?.ToString();
                    }
                    catch { }
                }

                auditLog.Properties = e.Properties
                    .Select(property =>
                    {
                        return new AuditLogProperty
                        {
                            PropertyName = property.PropertyName,
                            OldValue = property.OldValueFormatted,
                            OldValueDescription = audit.OldValueDescriptions?.GetValueOrDefault((e, property.PropertyName)),
                            NewValue = property.NewValueFormatted,
                            NewValueDescription = audit.NewValueDescriptions?.GetValueOrDefault((e, property.PropertyName))
                        };
                    })
                    .Where(property => 
                        property.OldValue != property.NewValue || 
                        property.OldValueDescription != property.NewValueDescription)
                    .ToList();

                operationContext?.Populate(auditLog, e.Entry);

                return auditLog;
            })
            .ToList();

        if (auditLogs.Count == 0)
        {
            return;
        }

        if (_options.MergeWindow > TimeSpan.Zero && db.Database.ProviderName == "Microsoft.EntityFrameworkCore.SqlServer")
        {
            var conn = db.Database.GetDbConnection();
            var cmd = conn.CreateCommand();
            cmd.CommandText = GetSqlServerSql(db.Model);

            var jsonParam = cmd.CreateParameter();
            jsonParam.ParameterName = "MergePayload";
            jsonParam.Value = JsonSerializer.Serialize(auditLogs, _mergeJsonOptions);


            // MergeWindowSeconds, in seconds, that a record can be for it to be updated (rather than inserting a new one).
            // This is chosen to be just under a minute so that changes happening on a one-minute interval to the same record
            // (there's nothing like this that I'm aware of currently in the application, but anyway...)
            // will not continuously roll forward on the same AuditLog record for all time.

            var mergeWindowParam = cmd.CreateParameter();
            mergeWindowParam.ParameterName = "MergeWindowSeconds";
            mergeWindowParam.Value = _options.MergeWindow.TotalSeconds;

            if (async)
            {
                await db.Database.ExecuteSqlRawAsync(cmd.CommandText, jsonParam, mergeWindowParam);
            }
            else
            {
                db.Database.ExecuteSqlRaw(cmd.CommandText, jsonParam, mergeWindowParam);
            }
        }
        else
        {
            // Naïve insertion for providers where we don't know how to generate 
            // SQL that will merge existing entries, or when the merge feature is disabled.

            db.AddRange(auditLogs);

            if (async)
            {
                await db.SaveChangesAsync();
            }
            else
            {
                db.SaveChanges();
            }
        }
    }

    private static readonly JsonSerializerOptions _mergeJsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        // 1: outer array
        // 2: AuditLog instance
        // 3: Properties array
        // 4: AuditLogProperties object
        // 5: Individual fields on AuditLogProperties (not sure why this counts as a depth layer)
        MaxDepth = 5,
        
    };
}
