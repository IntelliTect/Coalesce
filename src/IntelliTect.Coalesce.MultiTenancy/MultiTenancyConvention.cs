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

namespace IntelliTect.Coalesce.MultiTenancy;

/// <summary>
/// <para>
/// An EF Core model-finalizing convention that configures multi-tenancy for all entities
/// implementing <typeparamref name="TTenanted"/>. It runs after all entity types are registered,
/// ensuring correct handling of TPH hierarchies regardless of entity discovery order.
/// </para>
/// <para>
/// Register via <see cref="MultiTenancyDbContextOptionsBuilderExtensions.UseCoalesceMultiTenancy{TTenanted, TKey}"/>
/// or directly in <c>ConfigureConventions</c>:
/// <code>configurationBuilder.Conventions.Add(_ => new MultiTenancyConvention&lt;ITenanted&gt;(this, () => TenantIdOrThrow));</code>
/// </para>
/// </summary>
public class MultiTenancyConvention<TTenanted>(
    string tenantIdPropertyName,
    string? providerName,
    LambdaExpression tenantIdExpression
) : IModelFinalizingConvention
    where TTenanted : class
{
    /// <summary>
    /// The name used for global query filters added by this convention.
    /// Use this with <c>IgnoreQueryFilters</c> to selectively bypass tenancy filtering.
    /// </summary>
    public const string QueryFilterName = "TenancyFilter";

    internal string TenantIdPropName => tenantIdPropertyName;

    /// <summary>
    /// Applies the multi-tenancy configuration directly to a <see cref="ModelBuilder"/>.
    /// This is used by the <c>ConfigureMultiTenancy</c> extension method for backward compatibility.
    /// </summary>
    public void Apply(ModelBuilder builder)
    {
        var tenantIdGetter = BuildTenantIdGetter(tenantIdExpression);
        var tenantIdQueryExpr = tenantIdExpression.Body;

        var tenantedTypes = builder.Model.GetEntityTypes()
            .Where(e => typeof(TTenanted).IsAssignableFrom(e.ClrType))
            .ToList();

        // Process root types first to ensure PKs are expanded before derived types.
        foreach (var entityType in tenantedTypes.Where(e => e.BaseType is null))
        {
            SetTenancyFilter(entityType, tenantIdQueryExpr);
            ExpandPrimaryKey(entityType, tenantIdGetter);
        }

        // Then handle derived types — their PKs are already composite from the base;
        // just rewire any FKs that reference them directly.
        foreach (var entityType in tenantedTypes.Where(e => e.BaseType is not null))
        {
            RewireForeignKeys(entityType, entityType.FindPrimaryKey()!);
        }
    }

    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        var tenantIdGetter = BuildTenantIdGetter(tenantIdExpression);
        var tenantIdQueryExpr = tenantIdExpression.Body;

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

    /// <summary>
    /// Rewrites a zero-arg closure expression like <c>() => this.TenantIdOrThrow</c> into a delegate
    /// <c>Func&lt;DbContext, object?&gt;</c> so the value generator can call it on any DbContext instance.
    /// </summary>
    internal static Func<DbContext, object?> BuildTenantIdGetter(LambdaExpression tenantIdExpression)
    {
        if (tenantIdExpression.Body is MemberExpression { Expression: ConstantExpression constExpr } memberExpr
            && typeof(DbContext).IsAssignableFrom(constExpr.Type))
        {
            var param = Expression.Parameter(typeof(DbContext));
            var rewritten = Expression.Lambda<Func<DbContext, object?>>(
                Expression.Convert(
                    Expression.MakeMemberAccess(
                        Expression.Convert(param, constExpr.Type),
                        memberExpr.Member
                    ),
                    typeof(object)
                ),
                param
            );
            return rewritten.Compile();
        }

        // Fallback for non-trivial expressions: compile and invoke.
        var compiled = tenantIdExpression.Compile();
        return _ => compiled.DynamicInvoke();
    }

    private sealed class TenantIdValueGenerator(Func<DbContext, object?> getTenantId) : ValueGenerator
    {
        public override bool GeneratesTemporaryValues => false;
        public override bool GeneratesStableValues => true;
        protected override object? NextValue(EntityEntry entry) => getTenantId(entry.Context);
    }
}
