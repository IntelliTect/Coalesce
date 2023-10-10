using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Z.EntityFramework.Plus;

namespace IntelliTect.Coalesce.AuditLogging.Internal;

internal sealed class AuditingInterceptor<TObjectChange> : SaveChangesInterceptor
    where TObjectChange : class, IObjectChange
{
    private Audit? _audit;

    private IAuditLogContext<TObjectChange> GetContext(DbContextEventData data)
        => (IAuditLogContext<TObjectChange>)(data.Context ?? throw new InvalidOperationException("DbContext unavailable."));

    #region SavingChanges
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        _audit = null;
        if (GetContext(eventData).SuppressAudit) return ValueTask.FromResult(result);

        _audit = new Audit();
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

    private static readonly MemoryCache _sqlCache = new MemoryCache(new MemoryCacheOptions() { SizeLimit = 4_000_000 });

    private string BuildSqlServerSql(IModel model)
    {
        var entityType = model.FindEntityType(typeof(TObjectChange))!;
        var propEntityType = model.FindEntityType(typeof(ObjectChangeProperty))!;

        var tableName = entityType.GetSchemaQualifiedTableName();
        var propTableName = propEntityType.GetSchemaQualifiedTableName();

        var basePropNames = typeof(IObjectChange).GetProperties().Select(p => p.Name).ToArray();
        var props = entityType.GetDeclaredProperties();
        var propsByName = props.ToLookup(p => p.Name);

        var customProps = props.ExceptBy(basePropNames, p => p.PropertyInfo?.Name);
        var cursorProps = customProps.Concat(props.Where(p => p.Name is nameof(IObjectChange.State) or nameof(IObjectChange.Date) or nameof(IObjectChange.Type) or nameof(IObjectChange.KeyValue)));

        string GetColName(string propName, IEntityType table) => table
            .GetDeclaredProperties()
            .Single(p => p.Name == propName)
            .GetColumnName(StoreObjectIdentifier.Create(table, StoreObjectType.Table)!.Value)!;

        string GetPropColName(IProperty prop, IEntityType table) => prop
            .GetColumnName(StoreObjectIdentifier.Create(table, StoreObjectType.Table)!.Value)!;

        string pkCol = GetColName(nameof(IObjectChange.Id), entityType);
        string entityTypeNameCol = GetColName(nameof(IObjectChange.Type), entityType);
        string entityKeyValueCol = GetColName(nameof(IObjectChange.KeyValue), entityType);
        string dateCol = GetColName(nameof(IObjectChange.Date), entityType);
        string stateCol = GetColName(nameof(IObjectChange.State), entityType);

        string propFkCol = GetColName(nameof(ObjectChangeProperty.ParentId), propEntityType);
        string propPkCol = GetColName(nameof(ObjectChangeProperty.Id), propEntityType);

        return $"""
            SET NOCOUNT ON;
            SET XACT_ABORT ON;

            -- Setup iteration over each distinct [ObjectChange] represented by our incoming flat data.
            DECLARE object_cursor CURSOR FOR 
            SELECT {string.Join(", ", cursorProps.Select(p => p.Name))}, Properties
            FROM OPENJSON(@json) with (   
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
                DECLARE @mostRecentObjectChangeId [bigint];
                DECLARE @shouldUpdate [bit];
                SET @mostRecentObjectChangeId = 0;
                SET @shouldUpdate = 0;

                SELECT TOP 1 
                    @mostRecentObjectChangeId = {pkCol},
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
                        -- The existing ObjectChange record has the same set of properties as the incoming record
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

                IF @mostRecentObjectChangeId > 0 AND @shouldUpdate = 1
                    BEGIN
                        -- We found an existing record for the given Type and KeyValue and it is a candidate for being updated.

                        UPDATE {tableName}
                        SET {dateCol} = @Date
                        WHERE {pkCol} = @mostRecentObjectChangeId;


                        MERGE {propTableName}
                        USING (
                        	SELECT PropertyName, NewValue 
                        	FROM OPENJSON(@Properties) with (
            					PropertyName [nvarchar](max) '$.PropertyName',
            					OldValue [nvarchar](max) '$.OldValue',
            					NewValue [nvarchar](max) '$.NewValue'
            				)
                        ) AS Source
                        ON {propFkCol} = @mostRecentObjectChangeId AND {propTableName}.[PropertyName] = Source.PropertyName
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

                        SET @mostRecentObjectChangeId = SCOPE_IDENTITY();

                        INSERT INTO {propTableName}
                        ({propFkCol}, PropertyName, OldValue, NewValue)
                        SELECT @mostRecentObjectChangeId, PropertyName, OldValue, NewValue
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

    private string GetSql(IModel model) => _sqlCache.GetOrCreate(model, entry =>
    {
        string sql = BuildSqlServerSql(model);
        entry.Size = sql.Length;
        return sql;
    })!;

    private Task SaveAudit(DbContext db, Audit audit, bool async)
    {
        audit.PostSaveChanges();

        var serviceProvider = new EntityFrameworkServiceProvider(db);
        var operationContext = serviceProvider.GetService<IAuditOperationContext<TObjectChange>>();

        var date = DateTimeOffset.Now; // This must be the exact same date for all items for the merge logic in the SQL to work properly.

        var objectChanges = audit.Entries
            // The change is only useful if there's more than one property, since one of the properties is always the primary key.
            // This happens when the only changed properties on the object are marked as excluded properties.
            .Where(e => e.Properties.Count > 1 && e.Entity is not TObjectChange && e.Entity is not ObjectChangeProperty)
            .Select(e =>
            {
                var keyProperties = db.Model
                    .FindEntityType(e.Entity.GetType())?
                    .FindPrimaryKey()?
                    .Properties;

                var objectChange = Activator.CreateInstance<TObjectChange>();

                objectChange.Date = date;
                objectChange.State = (IntelliTect.Coalesce.AuditLogging.AuditEntryState)e.State;
                objectChange.Type = e.EntityTypeName;
                objectChange.KeyValue = keyProperties is null ? null : string.Join(";", keyProperties.Select(p => e.Entry.CurrentValues[p]));
                objectChange.Properties = e.Properties
                    .Where(property => property.OldValueFormatted != property.NewValueFormatted)
                    .Select(property =>
                    {
                        var prop = new ObjectChangeProperty
                        {
                            PropertyName = property.PropertyName,
                            OldValue = property.OldValueFormatted,
                            NewValue = property.NewValueFormatted
                        };
                        return prop;
                    }).ToList();

                operationContext?.Populate(objectChange, e.Entry);
                objectChange.Populate(db, serviceProvider, e.Entry);

                return objectChange;
            })
            .ToList();

        if (objectChanges.Count == 0)
        {
            return Task.CompletedTask;
        }

        if (db.Database.ProviderName == "Microsoft.EntityFrameworkCore.SqlServer")
        {
            var conn = db.Database.GetDbConnection();
            var cmd = conn.CreateCommand();
            cmd.CommandText = GetSql(db.Model);

            var jsonParam = cmd.CreateParameter();
            jsonParam.ParameterName = "json";
            jsonParam.Value = JsonSerializer.Serialize(objectChanges); // todo: super wrong, need to exclude user nav properties that may be tacked on, should exclude null


            // MergeWindowSeconds, in seconds, that a record can be for it to be updated (rather than inserting a new one).
            // This is chosen to be just under a minute so that changes happening on a one-minute interval to the same record
            // (there's nothing like this that I'm aware of currently in the application, but anyway...)
            // will not continuously roll forward on the same ObjectChange record for all time.

            var mergeWindowParam = cmd.CreateParameter();
            mergeWindowParam.ParameterName = "MergeWindowSeconds";
            mergeWindowParam.Value = 50; // todo: configurable?

            if (async)
            {
                //return cmd.ExecuteNonQueryAsync();
                return db.Database.ExecuteSqlRawAsync(cmd.CommandText, jsonParam, mergeWindowParam);
            }
            else
            {
                db.Database.ExecuteSqlRaw(cmd.CommandText, jsonParam, mergeWindowParam);
                //cmd.ExecuteNonQuery();
                return Task.CompletedTask;
            }
        }
        else
        {
            // Naïve insertion for providers where we don't know how to generate 
            // SQL that will merge existing entries.
            // TODO: Test if this works at all

            db.AddRange(objectChanges);

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
}
