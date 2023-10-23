using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace IntelliTect.Coalesce.AuditLogging.Internal;

internal sealed class AuditingInterceptor<TAuditLog> : SaveChangesInterceptor
    where TAuditLog : class, IAuditLog
{
    private readonly AuditOptions _options;

    private Audit? _audit;

    private IAuditLogContext<TAuditLog> GetContext(DbContextEventData data)
        => (IAuditLogContext<TAuditLog>)(data.Context ?? throw new InvalidOperationException("DbContext unavailable."));

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

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        _audit = null;
        if (GetContext(eventData).SuppressAudit) return ValueTask.FromResult(result);

        _audit = new Audit();
        AttachConfig(_audit);
        _audit.PreSaveChanges(eventData.Context);

        return ValueTask.FromResult(result);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        _audit = null;
        if (GetContext(eventData).SuppressAudit) return result;

        _audit = new Audit();
        AttachConfig(_audit);
        _audit.PreSaveChanges(eventData.Context);

        return result;
    }

    #endregion

    #region SavedChanges
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        if (_audit is null) return result;

        SaveAudit(eventData.Context!, _audit, async: false);

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

    public AuditingInterceptor(AuditOptions options)
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
            var cursorProps = customProps.Concat(props.Where(p => p.Name is nameof(IAuditLog.State) or nameof(IAuditLog.Date) or nameof(IAuditLog.Type) or nameof(IAuditLog.KeyValue)));

            string GetColName(string propName, IEntityType table) => table
                .GetDeclaredProperties()
                .Single(p => p.Name == propName)
                .GetColumnName(StoreObjectIdentifier.Create(table, StoreObjectType.Table)!.Value)!;

            string GetPropColName(IProperty prop, IEntityType table) => prop
                .GetColumnName(StoreObjectIdentifier.Create(table, StoreObjectType.Table)!.Value)!;

            string pkCol = GetColName(nameof(IAuditLog.Id), entityType);
            string entityTypeNameCol = GetColName(nameof(IAuditLog.Type), entityType);
            string entityKeyValueCol = GetColName(nameof(IAuditLog.KeyValue), entityType);
            string dateCol = GetColName(nameof(IAuditLog.Date), entityType);
            string stateCol = GetColName(nameof(IAuditLog.State), entityType);

            string propFkCol = GetColName(nameof(AuditLogProperty.ParentId), propEntityType);
            string propPkCol = GetColName(nameof(AuditLogProperty.Id), propEntityType);

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
                        AND {stateCol} = @State
                        {string.Join("\n           ", customProps.Select(p => $"AND ({GetPropColName(p, entityType)} = @{p.Name} OR ISNULL({GetPropColName(p, entityType)}, @{p.Name}) IS NULL)"))}
                        -- Date of the most recent record must be within X amount of time, 
                        AND DATEDIFF(second, {dateCol}, @Date) < @MergeWindowSeconds
                        -- and the incoming record must have a date equal or after the existing record
                        AND @Date >= {dateCol}
                        -- The existing AuditLog record has the same set of properties as the incoming record
                        AND (
                        		SELECT TOP 1 STRING_AGG([PropertyName], ',') WITHIN GROUP (ORDER BY [PropertyName] ASC)
                        		FROM {propTableName} 
                        		WHERE {propTableName}.{propFkCol} = {tableName}.{pkCol}
                        		GROUP BY {propTableName}.{propFkCol}
                        	) = (
                        		SELECT TOP 1 STRING_AGG([PropertyName], ',') WITHIN GROUP (ORDER BY [PropertyName] ASC)
                        		FROM OPENJSON(@Properties) WITH (
            						PropertyName [nvarchar](max) '$.PropertyName'
            					)
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
                        	SELECT PropertyName, NewValue 
                        	FROM OPENJSON(@Properties) with (
            					PropertyName [nvarchar](max) '$.PropertyName',
            					OldValue [nvarchar](max) '$.OldValue',
            					NewValue [nvarchar](max) '$.NewValue'
            				)
                        ) AS Source
                        ON {propFkCol} = @mostRecentAuditLogId AND {propTableName}.[PropertyName] = Source.PropertyName
                        WHEN MATCHED THEN
                        	-- DO NOT UPDATE OldValue! It stays the same so we represent the transition from the original OldValue to the new NewValue.
                        	UPDATE SET NewValue = Source.NewValue
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
                        ({propFkCol}, PropertyName, OldValue, NewValue)
                        SELECT @mostRecentAuditLogId, PropertyName, OldValue, NewValue
                        FROM OPENJSON (@Properties) WITH (
            				PropertyName [nvarchar](max) '$.PropertyName',
            				OldValue [nvarchar](max) '$.OldValue',
            				NewValue [nvarchar](max) '$.NewValue'
            			);

                    END;
            END;

            CLOSE object_cursor;
            DEALLOCATE object_cursor;
            """;
        }
    }

    private Task SaveAudit(DbContext db, Audit audit, bool async)
    {
        audit.PostSaveChanges();

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
                var keyProperties = db.Model
                    .FindEntityType(e.Entity.GetType())?
                    .FindPrimaryKey()?
                    .Properties;

                var auditLog = Activator.CreateInstance<TAuditLog>();

                auditLog.Date = date;
                auditLog.State = (IntelliTect.Coalesce.AuditLogging.AuditEntryState)e.State;
                auditLog.Type = e.EntityTypeName;
                auditLog.KeyValue = keyProperties is null ? null : string.Join(";", keyProperties.Select(p => e.Entry.CurrentValues[p]));
                auditLog.Properties = e.Properties
                    .Where(property => property.OldValueFormatted != property.NewValueFormatted)
                    .Select(property =>
                    {
                        var prop = new AuditLogProperty
                        {
                            PropertyName = property.PropertyName,
                            OldValue = property.OldValueFormatted,
                            NewValue = property.NewValueFormatted
                        };
                        return prop;
                    }).ToList();

                operationContext?.Populate(auditLog, e.Entry);

                return auditLog;
            })
            .ToList();

        if (auditLogs.Count == 0)
        {
            return Task.CompletedTask;
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
                return db.Database.ExecuteSqlRawAsync(cmd.CommandText, jsonParam, mergeWindowParam);
            }
            else
            {
                db.Database.ExecuteSqlRaw(cmd.CommandText, jsonParam, mergeWindowParam);
                return Task.CompletedTask;
            }
        }
        else
        {
            // Naïve insertion for providers where we don't know how to generate 
            // SQL that will merge existing entries, or when the merge feature is disabled.

            db.AddRange(auditLogs);

            if (async)
            {
                return db.SaveChangesAsync();
            }
            else
            {
                db.SaveChanges();
                return Task.CompletedTask;
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
