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

        foreach (var entityType in builder.Model.GetEntityTypes()
            .Where(e => typeof(TTenanted).IsAssignableFrom(e.ClrType))
            .ToList())
        {
            ConfigureEntityType(entityType, tenantIdQueryExpr, tenantIdGetter);
        }
    }

    public void ProcessModelFinalizing(IConventionModelBuilder modelBuilder, IConventionContext<IConventionModelBuilder> context)
    {
        var tenantIdGetter = BuildTenantIdGetter(tenantIdExpression);
        var tenantIdQueryExpr = tenantIdExpression.Body;

        foreach (var entityType in modelBuilder.Metadata.GetEntityTypes()
            .Where(e => typeof(TTenanted).IsAssignableFrom(e.ClrType))
            .ToList())
        {
            ConfigureEntityType((IMutableEntityType)entityType, tenantIdQueryExpr, tenantIdGetter);
        }
    }

    private void ConfigureEntityType(
        IMutableEntityType entityType,
        Expression tenantIdQueryExpr,
        Func<DbContext, object?> tenantIdGetter)
    {
        // Global query filters are only set on root types in a TPH/TPT hierarchy;
        // EF Core does not allow filters on derived types.
        if (entityType.BaseType is null)
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

        // Expand PKs to (TenantId, EntityKey) so that the tenant ID is the leading column,
        // and rewire any referencing FKs to include TenantId — making cross-tenant references
        // structurally impossible at the database level.
        //
        // This is transparent to Coalesce because Coalesce is only aware of the single-column
        // "logical" PK (EntityKey); the TenantId is always sourced from the logged-in user.

        var key = entityType.FindPrimaryKey();
        var tenantIdProp = entityType.FindProperty(TenantIdPropName);

        if (key is { Properties.Count: 1 } && tenantIdProp is not null && key.Properties.Single() != tenantIdProp)
        {
            // Install a value generator so entities can be .Add()ed while TenantId is still null
            // (EF may not have derived it yet from a navigation property).
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
                var newPk = entityType.BaseType != null
                    // TPH/TPT derived types inherit the PK from their base and cannot redefine it.
                    ? entityType.FindPrimaryKey()!
                    : entityType.SetPrimaryKey([tenantIdProp, pkProp])!;

                foreach (var fk in entityType.GetReferencingForeignKeys().ToList())
                {
                    fk.SetProperties(
                    [
                        fk.DeclaringEntityType.FindProperty(TenantIdPropName)
                            ?? throw new InvalidOperationException(
                                $"Foreign key from untenanted entity {fk.DeclaringEntityType} cannot reference tenanted principal {entityType}"),
                        fk.Properties.Single()
                    ], newPk);
                }

                pkProp.ValueGenerated = oldPkGenerated;
            }
        }
        else if (entityType.BaseType != null && tenantIdProp is not null && key is { Properties.Count: > 1 })
        {
            // Derived TPH types whose PK was already expanded by the base type
            // (i.e. the base was processed first, so this entity's PK is already composite).
            // Their own referencing FKs still need to be rewired to include TenantId.
            var newPk = entityType.FindPrimaryKey()!;
            foreach (var fk in entityType.GetReferencingForeignKeys().ToList())
            {
                if (fk.Properties.Count > 1) continue; // already rewired
                fk.SetProperties(
                [
                    fk.DeclaringEntityType.FindProperty(TenantIdPropName)
                        ?? throw new InvalidOperationException(
                            $"Foreign key from untenanted entity {fk.DeclaringEntityType} cannot reference tenanted principal {entityType}"),
                    fk.Properties.Single()
                ], newPk);
            }
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
        protected override object? Next(EntityEntry entry) => getTenantId(entry.Context);
    }
}
