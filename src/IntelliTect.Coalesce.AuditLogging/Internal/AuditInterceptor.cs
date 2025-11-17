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
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.AuditLogging.Internal;

internal sealed class AuditInterceptor<TAuditLog> : SaveChangesInterceptor
    where TAuditLog : class, IAuditLog
{
    private readonly AuditOptions _options;

    private CoalesceAudit? _audit;

    private IAuditLogDbContext<TAuditLog> GetContext(DbContextEventData data)
        => (IAuditLogDbContext<TAuditLog>)(data.Context ?? throw new InvalidOperationException("DbContext unavailable."));

    #region SavingChanges

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        _audit = null;
        if (GetContext(eventData).SuppressAudit) return result;

        _audit = new CoalesceAudit(_options.AuditConfiguration ?? new());
        _audit.PreSaveChanges(eventData.Context!);

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

        _audit = new CoalesceAudit(_options.AuditConfiguration ?? new());
        _audit.PreSaveChanges(eventData.Context!);

        if (_options.PropertyDescriptions.HasFlag(PropertyDescriptionMode.FkListText))
        {
#pragma warning disable CS4014 // Executes synchronously when async: false
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

#pragma warning disable CS4014 // Executes synchronously when async: false
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

    /// <summary>
    /// Generates a stored procedure name based on the SQL hash to handle version conflicts.
    /// </summary>
    private static string GetStoredProcedureName(string sql)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(sql));
        var shortHash = Convert.ToHexString(hash[..4]); // Use first 8 characters
        return $"AuditMerge_{shortHash}";
    }

    /// <summary>
    /// Generates the DDL to create the stored procedure.
    /// </summary>
    private static string GetStoredProcedureDdl(string procedureName, string sql)
    {
        return $"""
            IF OBJECT_ID('{procedureName}', 'P') IS NOT NULL
                DROP PROCEDURE [{procedureName}];

            EXEC('CREATE PROCEDURE [{procedureName}]
                @MergePayload NVARCHAR(MAX),
                @MergeWindowSeconds INT
            AS
            BEGIN
                {sql.Replace("'", "''")}
            END');
            """;
    }

    /// <summary>
    /// Ensures the stored procedure exists and returns its name.
    /// </summary>
    private async ValueTask<string> EnsureStoredProcedureExists(DbContext db, string sql, bool async)
    {
        var cacheKey = (db.Model, db.Database.GetConnectionString());

        var procedureName = GetStoredProcedureName(sql);

        // Check if we've already verified this procedure exists for this model
        if (AuditOptions.StoredProcedureCache.TryGetValue<string>(cacheKey, out var cachedName) && cachedName == procedureName)
        {
            return procedureName;
        }

        // Check if the procedure exists and has the correct content
        var needsCreation = await CheckProcedureNeedsCreation(db, procedureName, sql, async);

        if (needsCreation)
        {
            // Create or update the stored procedure
            var ddl = GetStoredProcedureDdl(procedureName, sql);

            if (async)
            {
                await db.Database.ExecuteSqlRawAsync(ddl);
            }
            else
            {
                db.Database.ExecuteSqlRaw(ddl);
            }
        }

        // Cache that we've verified this procedure exists with the correct content
        AuditOptions.StoredProcedureCache.Set(cacheKey, procedureName, new MemoryCacheEntryOptions
        {
            Size = procedureName.Length + cacheKey.Item2?.Length,
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1) // Re-verify after an hour
        });

        return procedureName;
    }

    /// <summary>
    /// Checks if a stored procedure needs to be created or updated.
    /// </summary>
    private async ValueTask<bool> CheckProcedureNeedsCreation(DbContext db, string procedureName, string expectedSql, bool async)
    {
        // Use FormattableString to ensure proper parameterization
        FormattableString sql = $"SELECT OBJECT_DEFINITION(OBJECT_ID({procedureName}, 'P')) AS [Value]";
        var query = db.Database.SqlQuery<string?>(sql);

        string? result;
        if (async)
        {
            result = await query.FirstOrDefaultAsync();
        }
        else
        {
            result = query.FirstOrDefault();
        }

        if (result == null)
        {
            // Procedure doesn't exist
            return true;
        }

        // Procedure exists, check if content matches
        var existingDefinition = result!;

        // Extract the SQL from the stored procedure definition to compare with expected SQL
        // The existing definition includes the CREATE PROCEDURE wrapper, so we need to extract just the body
        var bodyStart = existingDefinition.IndexOf("BEGIN", StringComparison.OrdinalIgnoreCase);
        var bodyEnd = existingDefinition.LastIndexOf("END", StringComparison.OrdinalIgnoreCase);

        if (bodyStart >= 0 && bodyEnd > bodyStart)
        {
            var existingBody = existingDefinition.Substring(bodyStart + 5, bodyEnd - bodyStart - 5).Trim();
            var expectedBody = expectedSql.Trim();

            if (existingBody != expectedBody)
            {
                // Content doesn't match, needs update
                return true;
            }
        }
        else
        {
            // Couldn't parse the procedure body, safer to recreate
            return true;
        }

        return false; // Procedure exists and content matches
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

        // This must be the exact same date for all items for the merge logic in the SQL to work properly.
        // UtcNow is used (instead of Now) because Postgres doesn't support non-zero offset values.
        var date = DateTimeOffset.UtcNow;

        var auditLogs = audit.Entries
            .Where(e => e.Entity is not IAuditLog && e.Entity is not AuditLogProperty)
            .Select(e =>
            {
                var entityClrType = e.Entity.GetType();
                var keyProperties = db.Model
                    .FindEntityType(entityClrType)?
                    .FindPrimaryKey()?
                    .Properties;

                var auditLog = Activator.CreateInstance<TAuditLog>();

                auditLog.Date = date;
                auditLog.State = e.State;
                auditLog.Type = e.Entry.Metadata.ClrType.Name;
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
                        var propMeta = property.PropertyEntry.Metadata;
                        var propType = propMeta.ClrType;
                        var propPureType = Nullable.GetUnderlyingType(propType) ?? propType;

                        return new AuditLogProperty
                        {
                            PropertyName = property.PropertyName,
                            OldValue = property.OldValueFormatted,
                            OldValueDescription = property.OldValueDescription,
                            NewValue = property.NewValueFormatted,
                            NewValueDescription = property.NewValueDescription,
                            CanMerge = _options.MergeMode switch
                            {
                                MergeMode.None => false,
                                MergeMode.All => true,

                                MergeMode.NonDiscreteOnly =>
                                    !propMeta.IsForeignKey() &&
                                    !propPureType.IsEnum &&
                                    propPureType != typeof(bool),

                                MergeMode.StringsOnly =>
                                    propType == typeof(string) &&
                                    !propMeta.IsForeignKey(),

                                _ => false
                            }
                        };
                    })
                    .Where(property =>
                        property.OldValue != property.NewValue ||
                        property.OldValueDescription != property.NewValueDescription)
                    .ToList();

                operationContext?.Populate(auditLog, e.Entry);

                return auditLog;
            })
            // The change is only useful if there are any properties.
            // This is checked after `operationContext.Populate` so that that user
            // has a chance to apply customizations before we filter out an entry.
            .Where(e => e.Properties?.Any() == true)
            .ToList();

        if (auditLogs.Count == 0)
        {
            return;
        }

        List<TAuditLog> mergeLogs = new();
        List<TAuditLog> noMergeLogs = new();

        if (
            _options.MergeWindow > TimeSpan.Zero &&
            db.Database.ProviderName == "Microsoft.EntityFrameworkCore.SqlServer"
        )
        {
            foreach (var log in auditLogs)
            {
                if (log.State == AuditEntryState.EntityModified && log.Properties!.All(p => p.CanMerge))
                {
                    mergeLogs.Add(log);
                }
                else
                {
                    noMergeLogs.Add(log);
                }
            }
        }
        else
        {
            noMergeLogs = auditLogs;
        }

        if (mergeLogs.Count > 0)
        {
            var conn = db.Database.GetDbConnection();
            var cmd = conn.CreateCommand();

            var jsonParam = cmd.CreateParameter();
            jsonParam.ParameterName = "MergePayload";
            jsonParam.Value = JsonSerializer.Serialize(mergeLogs, _mergeJsonOptions);

            var mergeWindowParam = cmd.CreateParameter();
            mergeWindowParam.ParameterName = "MergeWindowSeconds";
            mergeWindowParam.Value = _options.MergeWindow.TotalSeconds;

            if (_options.UseStoredProcedures)
            {
                // Use stored procedure mode
                var sql = GetSqlServerSql(db.Model);
                var procedureName = await EnsureStoredProcedureExists(db, sql, async);

                cmd.CommandText = $"EXEC [{procedureName}] @MergePayload, @MergeWindowSeconds";
            }
            else
            {
                // Use raw SQL mode (existing behavior)
                cmd.CommandText = GetSqlServerSql(db.Model);
            }

            if (async)
            {
                await db.Database.ExecuteSqlRawAsync(cmd.CommandText, jsonParam, mergeWindowParam);
            }
            else
            {
                db.Database.ExecuteSqlRaw(cmd.CommandText, jsonParam, mergeWindowParam);
            }
        }

        if (noMergeLogs.Count > 0)
        {
            // Naïve insertion for providers where we don't know how to generate 
            // SQL that will merge existing entries, or when the merge feature is disabled.

            db.AddRange(noMergeLogs);

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
