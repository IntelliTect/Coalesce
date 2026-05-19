using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace IntelliTect.Coalesce.MultiTenancy;

/// <summary>
/// Constants for the multi-tenancy convention.
/// </summary>
public static class MultiTenancyConvention
{
    /// <summary>
    /// The name used for global query filters added by the multi-tenancy convention.
    /// Use this with <c>IgnoreQueryFilters</c> to selectively bypass tenancy filtering.
    /// </summary>
    public const string QueryFilterName = "TenancyFilter";
}

/// <summary>
/// <para>
/// An EF Core model-finalizing convention that configures multi-tenancy for all entities
/// implementing <typeparamref name="TTenanted"/>. It runs after all entity types are registered,
/// ensuring correct handling of TPH hierarchies regardless of entity discovery order.
/// </para>
/// </summary>
public class MultiTenancyConvention<TTenanted>(
    string entityTenantIdPropertyName,
    string? providerName,
    Type dbContextType,
    string contextTenantIdPropertyName
) : IModelFinalizingConvention
    where TTenanted : class
{
    /// <inheritdoc cref="MultiTenancyConvention.QueryFilterName"/>
    public const string QueryFilterName = MultiTenancyConvention.QueryFilterName;

    internal string TenantIdPropName => entityTenantIdPropertyName;

    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        var tenantIdValuePropInfo = dbContextType.GetProperty(contextTenantIdPropertyName)
            ?? throw new InvalidOperationException(
                $"Property '{contextTenantIdPropertyName}' not found on {dbContextType.Name}. " +
                $"Ensure the property exists and is public.");

        // Build the query filter expression fragment: db.TenantIdOrThrow
        // Uses null! constant of the DbContext type — EF recognizes the type and substitutes the real instance at query time.
        var dbContextConstant = Expression.Constant(null, dbContextType);
        var tenantIdQueryExpr = Expression.MakeMemberAccess(dbContextConstant, tenantIdValuePropInfo);

        // Build runtime getter via reflection for value generator and interceptor
        Func<DbContext, object?> tenantIdGetter = db => tenantIdValuePropInfo.GetValue(db);

        var tenantedTypes = modelBuilder.Metadata.GetEntityTypes()
            .Where(e => typeof(TTenanted).IsAssignableFrom(e.ClrType))
            .ToList();

        foreach (var entityType in tenantedTypes.Where(e => e.BaseType is null))
        {
            SetTenancyFilter((IMutableEntityType)entityType, tenantIdQueryExpr);
            ExpandPrimaryKey((IMutableEntityType)entityType, tenantIdGetter);
        }

        foreach (var entityType in tenantedTypes.Where(e => e.BaseType is not null))
        {
            RewireForeignKeys((IMutableEntityType)entityType, ((IMutableEntityType)entityType).FindPrimaryKey()!);
        }
    }

    private void SetTenancyFilter(IMutableEntityType entityType, Expression tenantIdQueryExpr)
    {
        var param = Expression.Parameter(entityType.ClrType);
        var filterBody = Expression.Lambda(
            Expression.Equal(
                Expression.MakeMemberAccess(param, entityType.ClrType.GetProperty(TenantIdPropName)!),
                tenantIdQueryExpr
            ),
            param
        );
#if NET10_0_OR_GREATER
        entityType.SetQueryFilter(QueryFilterName, filterBody);
#else
        entityType.SetQueryFilter(filterBody);
#endif
    }

    private void ExpandPrimaryKey(IMutableEntityType entityType, Func<DbContext, object?> tenantIdGetter)
    {
        var key = entityType.FindPrimaryKey();
        var tenantIdProp = entityType.FindProperty(TenantIdPropName);

        if (key is not { Properties.Count: 1 } || tenantIdProp is null || key.Properties.Single() == tenantIdProp)
            return;

        tenantIdProp.SetValueGeneratorFactory((p, t) => new TenantIdValueGenerator(tenantIdGetter));

        var pkProp = key.Properties.Single();
        var oldPkGenerated = pkProp.ValueGenerated;

        if (
            providerName == "Microsoft.EntityFrameworkCore.Sqlite" &&
            new ReflectionTypeViewModel(pkProp.ClrType).PureType.IsNumber &&
            pkProp.ValueGenerated is ValueGenerated.OnAdd &&
            pkProp.GetValueGeneratorFactory() is null
        )
        {
            // SQLite does not support composite PKs where one column is AUTOINCREMENT.
            // As a workaround, keep the single-column PK and add a composite alternate key
            // that referencing FKs are rewired to use instead.
            var tenantedAk = entityType.AddKey([tenantIdProp, pkProp])!;

            foreach (var fk in entityType.GetReferencingForeignKeys().ToList())
            {
                var dependentTenantId = fk.DeclaringEntityType.FindProperty(TenantIdPropName);
                if (dependentTenantId is null) continue;

                var newFk = fk.DeclaringEntityType.AddForeignKey([dependentTenantId, fk.Properties.Single()], tenantedAk, entityType);
                newFk.DeleteBehavior = DeleteBehavior.NoAction;
            }
        }
        else
        {
            var newPk = entityType.SetPrimaryKey([tenantIdProp, pkProp])!;
            RewireForeignKeys(entityType, newPk);
            pkProp.ValueGenerated = oldPkGenerated;
        }
    }

    private void RewireForeignKeys(IMutableEntityType entityType, IMutableKey principalKey)
    {
        foreach (var fk in entityType.GetReferencingForeignKeys().ToList())
        {
            if (fk.Properties.Count > 1) continue; // already rewired
            fk.SetProperties(
            [
                fk.DeclaringEntityType.FindProperty(TenantIdPropName)
                    ?? throw new InvalidOperationException(
                        $"Foreign key from untenanted entity {fk.DeclaringEntityType} cannot reference tenanted principal {entityType}"),
                fk.Properties.Single()
            ], principalKey);
        }
    }

    private sealed class TenantIdValueGenerator(Func<DbContext, object?> getTenantId) : ValueGenerator
    {
        public override bool GeneratesTemporaryValues => false;
        public override bool GeneratesStableValues => true;
        protected override object? NextValue(EntityEntry entry) => getTenantId(entry.Context);
    }
}
