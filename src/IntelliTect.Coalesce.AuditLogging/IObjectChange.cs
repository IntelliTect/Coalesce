using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System;
using IntelliTect.Coalesce.DataAnnotations;
using static IntelliTect.Coalesce.DataAnnotations.SecurityPermissionLevels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using IntelliTect.Coalesce.AuditLogging;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Threading;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.AspNetCore.Http;
using IntelliTect.Coalesce.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Z.EntityFramework.Plus;
using Microsoft.CodeAnalysis.Differencing;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Configuration.Provider;
using System.Data.Common;
using System.Text.Json;

namespace IntelliTect.Coalesce.AuditLogging;

public interface IObjectChange<TUserKey>
{
    long ObjectChangeId { get; set; }

    string EntityTypeName { get; set; }

    string? EntityKeyValue { get; set; }

    TUserKey UserId { get; set; }

    DateTimeOffset Date { get; set; }

    string State { get; set; }

    /// <summary>
    /// Populate the values of extra custom fields on this change entry. TODO: remove this?
    /// </summary>
    Task PopulateAsync(DbContext db) => Task.CompletedTask;
}

public interface IObjectChange<TUserKey, TObjectChangeProperty> : IObjectChange<TUserKey>
{
    ICollection<TObjectChangeProperty>? Properties { get; set; }
}

[Edit(DenyAll)]
[Delete(DenyAll)]
[Create(DenyAll)]
public abstract class ObjectChange<TUserKey, TObjectChangeProperty> : IObjectChange<TUserKey, TObjectChangeProperty>
{
    [DefaultOrderBy(OrderByDirection = DefaultOrderByAttribute.OrderByDirections.Descending)]
    public long ObjectChangeId { get; set; }

    [MaxLength(100), Column(TypeName = "varchar(100)")]
    [ListText, Search]
    public string EntityTypeName { get; set; }

    public string EntityKeyValue { get; set; }

    public TUserKey? UserId { get; set; }

    public DateTimeOffset Date { get; set; }

    public ICollection<TObjectChangeProperty> Properties { get; set; }

    /// <summary>
    /// Stringified value of <see cref="AuditEntryState"/>
    /// </summary>
    [MaxLength(30), Column(TypeName = "varchar(30)")]
    public string State { get; set; }

    /// <summary>
    /// Populate the values of extra custom fields on this change entry.
    /// </summary>
    public async Task PopulateAsync(DbContext db)
    {

    }
}

public abstract class ObjectChange<TUserKey> : ObjectChange<TUserKey, ObjectChangeProperty>
{
    public string RequestUrl { get; set; }
    public string Referrer { get; set; }
    public string ClientIpAddress { get; set; }
}


public interface IObjectChangeProperty
{
    long ObjectChangePropertyId { get; set; }

    long ObjectChangeId { get; set; }

    string PropertyName { get; set; }

    string? OldValue { get; set; }

    string? NewValue { get; set; }
}

[Edit(DenyAll)]
[Delete(DenyAll)]
[Create(DenyAll)]
public class ObjectChangeProperty : IObjectChangeProperty
{
    public long ObjectChangePropertyId { get; set; }

    public long ObjectChangeId { get; set; }

    [ListText, Search, MaxLength(100), Column(TypeName = "varchar(100)")]
#if NET7_0_OR_GREATER 
required 
#endif
    public string PropertyName { get; set; } = null!;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }
}

public interface IAuditLogContext<TUserKey, TObjectChange, TObjectChangeProperty>
    where TObjectChange : class, IObjectChange<TUserKey, TObjectChangeProperty>
    where TObjectChangeProperty : class, IObjectChangeProperty
{
    DbSet<TObjectChange> ObjectChanges { get; }
    DbSet<TObjectChangeProperty> ObjectChangeProperties { get; }
    bool SuppressAudit { get; }

    public void AddCoalesceAuditLogs(ModelBuilder builder)
    {
        builder.Entity<TObjectChange>(e =>
        {
            e.HasIndex(c => new { c.EntityTypeName, c.EntityKeyValue });
            e.HasIndex(c => c.State);

            // An index on EntityTypeName is needed by itself because 
            // the index that includes EntityKeyValue is no good when not looking for a specific key
            // (looking at that index requires an index scan rather than a seek).
            e.HasIndex(c => c.EntityTypeName);

            e.HasMany(c => c.Properties)
                .WithOne()
                .HasPrincipalKey(c => c.ObjectChangeId)
                .HasForeignKey(c => c.ObjectChangeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

public interface IAuditLogContext<TUserKey, TObjectChange> 
    : IAuditLogContext<TUserKey, TObjectChange, ObjectChangeProperty>
    where TObjectChange : class, IObjectChange<TUserKey, ObjectChangeProperty>
    { }

public static class ModelBuilderExtensions
{
    public static void UseCoalesceAuditLogs<TUserKey, TObjectChange, TObjectChangeProperty>(
        this ModelBuilder builder,
        IAuditLogContext<TUserKey, TObjectChange, TObjectChangeProperty> _dbContextTypeHint)
        where TObjectChange : class, IObjectChange<TUserKey, TObjectChangeProperty>
        where TObjectChangeProperty : class, IObjectChangeProperty
        => ConfigureCoalesceAuditLogs<TUserKey, TObjectChange, TObjectChangeProperty>(builder);

    public static void ConfigureCoalesceAuditLogs<TUserKey, TObjectChange, TObjectChangeProperty>(this ModelBuilder builder)
        where TObjectChange : class, IObjectChange<TUserKey, TObjectChangeProperty>
        where TObjectChangeProperty : class, IObjectChangeProperty
    {
        builder.Entity<TObjectChange>(e =>
        {
            e.HasIndex(c => new { c.EntityTypeName, c.EntityKeyValue });
            e.HasIndex(c => c.State);

            // An index on EntityTypeName is needed by itself because 
            // the index that includes EntityKeyValue is no good when not looking for a specific key
            // (looking at that index requires an index scan rather than a seek).
            e.HasIndex(c => c.EntityTypeName);

            e.HasMany(c => c.Properties)
                .WithOne()
                .HasPrincipalKey(c => c.ObjectChangeId)
                .HasForeignKey(c => c.ObjectChangeId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

public static class DbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder<TContext> AddCoalesceAuditLogging<TContext, TUserKey, TObjectChange, TObjectChangeProperty>(
        this DbContextOptionsBuilder<TContext> builder)
        where TContext : DbContext, IAuditLogContext<TUserKey, TObjectChange, TObjectChangeProperty>
        where TObjectChange : class, IObjectChange<TUserKey, TObjectChangeProperty>
        where TObjectChangeProperty : class, IObjectChangeProperty
    {
        builder.AddInterceptors(new AuditingInterceptor<TUserKey, TObjectChange, TObjectChangeProperty>());
        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(new AuditExtension<TUserKey, TObjectChange>());

        return builder;
    }
}

public class AuditExtension<TUserKey, TObjectChange> : IDbContextOptionsExtension
    where TObjectChange : class, IObjectChange<TUserKey>
{
    private DbContextOptionsExtensionInfo? _info;
    public DbContextOptionsExtensionInfo Info => _info ??= new ExtensionInfo(this);

    public void ApplyServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.TryAddScoped<IAuditOperationContext<TUserKey, TObjectChange>, AuditOperationContext<TUserKey, TObjectChange>>();
    }

    public void Validate(IDbContextOptions options)
    {
    }

    private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
    {
        public ExtensionInfo(IDbContextOptionsExtension extension) : base(extension) { }

        public override bool IsDatabaseProvider => false;

        public override string LogFragment => "";

        public override int GetServiceProviderHashCode() => 0;

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) { }

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => other is ExtensionInfo;
    }
}

public interface IAuditOperationContext<TUserKey, TObjectChange>
    where TObjectChange : class, IObjectChange<TUserKey>
{
    void Populate(TObjectChange auditEntry, object entity);
}

public class AuditOperationContext<TUserKey, TObjectChange> : IAuditOperationContext<TUserKey, TObjectChange>
    where TObjectChange : class, IObjectChange<TUserKey>
{
    private readonly IHttpContextAccessor httpContextAccessor;

    public AuditOperationContext(IHttpContextAccessor httpContextAccessor)
    {
        this.httpContextAccessor = httpContextAccessor;
    }

    public void Populate(TObjectChange auditEntry, object entity)
    {
        string? userId = httpContextAccessor.HttpContext?.User.GetUserId();
        if (userId != null)
        {
            auditEntry.UserId = (TUserKey)Convert.ChangeType(userId, typeof(TUserKey));
        }
    }
}

public sealed class AuditingInterceptor<TUserKey, TObjectChange, TObjectChangeProperty> : SaveChangesInterceptor
    where TObjectChange : class, IObjectChange<TUserKey, TObjectChangeProperty>, new()
    where TObjectChangeProperty : class, IObjectChangeProperty
{
    private Audit? _audit;

    private IAuditLogContext<TUserKey, TObjectChange, TObjectChangeProperty> GetContext(DbContextEventData data) 
        => (IAuditLogContext<TUserKey, TObjectChange, TObjectChangeProperty>)(data.Context ?? throw new InvalidOperationException("DbContext unavailable."));

    #region SavingChanges
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        _audit = null;
        if (GetContext(eventData).SuppressAudit) return result;

        _audit = new Audit();
        _audit.PreSaveChanges(eventData.Context);

        return result;
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
        var entityType = model.FindEntityType(typeof(TObjectChange));
        var propEntityType = model.FindEntityType(typeof(TObjectChangeProperty));

        var tableName = entityType.GetSchemaQualifiedTableName();
        var propTableName = propEntityType.GetSchemaQualifiedTableName();

        var basePropNames = typeof(IObjectChange<TUserKey>).GetProperties().Select(p => p.Name).ToArray();
        var props = entityType.GetDeclaredProperties();
        var customProps = props.ExceptBy(basePropNames, p => p.PropertyInfo?.Name);
        var propsByName = props.ToLookup(p => p.Name);

        var objectChangeStoreId = StoreObjectIdentifier.Create(entityType, StoreObjectType.Table)!.Value;
        string GetColName(string propName) => propsByName[propName].Single().GetColumnName(objectChangeStoreId)!;

        string entityTypeNameCol = GetColName(nameof(IObjectChange<TUserKey>.EntityTypeName));
        string entityKeyValueCol = GetColName(nameof(IObjectChange<TUserKey>.EntityKeyValue));
        string dateCol = GetColName(nameof(IObjectChange<TUserKey>.Date));
        string createdByCol = GetColName(nameof(IObjectChange<TUserKey>.UserId));
        string stateCol = GetColName(nameof(IObjectChange<TUserKey>.State));

        return $"""
            SET NOCOUNT ON;
            SET XACT_ABORT ON;

            -- Setup iteration over each distinct [ObjectChange] represented by our incoming flat data.
            DECLARE object_cursor CURSOR FOR 
            SELECT EntityTypeName, EntityKeyValue, CreatedById, [Date], [State], RequestUrl, Referrer, ClientIpAddress, properties
            FROM OPENJSON(@json) with (   
            	EntityTypeName [varchar](100) '$.EntityTypeName',
            	EntityKeyValue [nvarchar](450) '$.EntityKeyValue',
            	CreatedById [nvarchar](450) '$.CreatedById',
            	Date [datetimeoffset](7) '$.Date',
            	State [varchar](30) '$.State',
                {string.Join(",\n    ", customProps.Select(p => $"{p.Name} {p.GetRelationalTypeMapping().StoreType} '$.{p.Name}'"))},
            	Properties [nvarchar](max) '$.Properties' as JSON
            );

            DECLARE @EntityTypeName [varchar](100);
            DECLARE @EntityKeyValue [nvarchar](450);
            DECLARE @CreatedById [nvarchar](450);
            DECLARE @Date [datetimeoffset](7);
            DECLARE @State [varchar](30);
            {string.Join(";\n", customProps.Select(p => $"DECLARE @{p.Name} {p.GetRelationalTypeMapping().StoreType};"))};
            DECLARE @Properties [nvarchar](max);

            OPEN object_cursor

            WHILE (1=1)
            BEGIN  
                -- Looping over each entity in the set of changes passed in.
            	FETCH NEXT FROM object_cursor INTO 
            		@EntityTypeName, 
            		@EntityKeyValue,
            		@CreatedById,
            		@Date,
            		@State,
                    {string.Join(",\n        ", customProps.Select(p => $"@{p.Name}"))},
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
                    @mostRecentObjectChangeId = [ObjectChangeId],
                    @shouldUpdate = (CASE WHEN 
                        -- Don't update if the type and/or key matched on NULL=NULL
                        @EntityTypeName IS NOT NULL
                        AND @EntityKeyValue IS NOT NULL
                        -- These fields must match exactly:
                        AND {createdByCol} = @CreatedById
                        AND {stateCol} = @State
                        {string.Join("\n           ", customProps.Select(p => $"AND {p.GetColumnName()} = @{p.Name}"))}
                        -- Date of the most recent record must be within X amount of time, 
                        AND DATEDIFF(second, {dateCol}, @Date) < @MergeWindowSeconds
                        -- and the incoming record must have a date equal or after the existing record
                        AND @Date >= {dateCol}
                        -- The existing ObjectChange record has the same set of properties as the incoming record
                        AND (
                        		SELECT TOP 1 STRING_AGG([PropertyName], ',') WITHIN GROUP (ORDER BY [PropertyName] ASC)
                        		FROM {propTableName} 
                        		WHERE {propTableName}.[ObjectChangeId] = {tableName}.[ObjectChangeId]
                        		GROUP BY [ObjectChangeId]
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
                WHERE {entityTypeNameCol} = @EntityTypeName AND {entityKeyValueCol} = @EntityKeyValue
                ORDER BY [ObjectChangeId] DESC;

                IF @mostRecentObjectChangeId > 0 AND @shouldUpdate = 1
                    BEGIN
                        -- We found an existing record for the given EntityType and KeyValue and it is a candidate for being updated.
                        --PRINT('AUDIT UPDATE ' + @EntityTypeName + ':' + @EntityKeyValue);

                        UPDATE {tableName}
                        SET {dateCol} = @Date
                        WHERE [ObjectChangeId] = @mostRecentObjectChangeId;


                        MERGE {propTableName}
                        USING (
                        	SELECT PropertyName, NewValue 
                        	FROM OPENJSON(@Properties) with (
            					PropertyName [nvarchar](max) '$.PropertyName',
            					OldValue [nvarchar](max) '$.OldValue',
            					NewValue [nvarchar](max) '$.NewValue'
            				)
                        ) AS Source
                        ON [ObjectChangeId] = @mostRecentObjectChangeId AND {propTableName}.[PropertyName] = Source.PropertyName
                        WHEN MATCHED THEN
                        	-- DO NOT UPDATE OldValue! It stays the same so we represent the transition from the original OldValue to the new NewValue.
                        	UPDATE SET NewValue = Source.NewValue
                        ;
                    END;
                ELSE
                    BEGIN
                        -- The existing record didn't exist, or was not a candidate for being updated. Insert a new record.
                        -- PRINT('AUDIT INSERT ' + @EntityTypeName + ':' + @EntityKeyValue);

                        INSERT INTO {tableName}
                        ({entityTypeNameCol}, {entityKeyValueCol}, {createdByCol}, {dateCol}, {stateCol}, {string.Join(", ", customProps.Select(p => p.GetColumnName()))})
                        VALUES (@EntityTypeName, @EntityKeyValue, @CreatedById, @Date, @State, {string.Join(", ", customProps.Select(p => $"@{p.Name}"))});

                        SET @mostRecentObjectChangeId = SCOPE_IDENTITY();

                        INSERT INTO {propTableName}
                        (ObjectChangeId, PropertyName, OldValue, NewValue)
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
    });

    private Task SaveAudit(DbContext db, Audit audit, bool async)
    {
        audit.PostSaveChanges();

        var operation = db.GetService<IAuditOperationContext<TUserKey, TObjectChange>>();
        var date = DateTimeOffset.Now; // This must be the exact same date for all items for the merge logic in the SQL to work properly.

        var objectChanges = audit.Entries
            // The change is only useful if there's more than one property, since one of the properties is always the primary key.
            // This happens when the only changed properties on the object are marked as excluded properties.
            .Where(e => e.Properties.Count > 1 && e is not TObjectChange && e is not TObjectChangeProperty)
            .Select(e =>
            {
                var keyProperty = db.Model
                    .FindEntityType(e.Entity.GetType())?
                    .FindPrimaryKey()?
                    .Properties.SingleOrDefault()?
                    .PropertyInfo;

                var objectChange = Activator.CreateInstance<TObjectChange>();

                objectChange.Date = date;
                objectChange.State = e.StateName;
                objectChange.EntityTypeName = e.EntityTypeName;
                objectChange.EntityKeyValue = keyProperty?.GetValue(e.Entity)?.ToString();
                objectChange.Properties = e.Properties
                    .Where(property => property.OldValueFormatted != property.NewValueFormatted)
                    .Select(property =>
                    {
                        var prop = Activator.CreateInstance<TObjectChangeProperty>();
                        prop.PropertyName = property.PropertyName;
                        prop.OldValue = property.OldValueFormatted;
                        prop.NewValue = property.NewValueFormatted;
                        return prop;
                    }).ToList();

                operation.Populate(objectChange, e.Entity);

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
            jsonParam.Value = JsonSerializer.Serialize(objectChanges); // todo: super wrong, need to exclude user nav properties that may be tacked on.


            // MergeWindowSeconds, in seconds, that a record can be for it to be updated (rather than inserting a new one).
            // This is chosen to be just under a minute so that changes happening on a one-minute interval to the same record
            // (there's nothing like this that I'm aware of currently in the application, but anyway...)
            // will not continuously roll forward on the same ObjectChange record for all time.

            var mergeWindowParam = cmd.CreateParameter();
            mergeWindowParam.ParameterName = "MergeWindowSeconds";
            mergeWindowParam.Value = 50; // todo: configurable?

            if (async)
            {
                return cmd.ExecuteNonQueryAsync();
            }
            else
            {
                cmd.ExecuteNonQuery();
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