using IntelliTect.Coalesce.MultiTenancy;
using IntelliTect.Coalesce.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Linq.Expressions;

namespace IntelliTect.Coalesce;

public static class MultiTenancyDbContextOptionsBuilderExtensions
{
    /// <summary>
    /// <para>
    /// Configures multi-tenancy for all entities that implement <typeparamref name="TTenanted"/>.
    /// This registers both a save interceptor (for enforcing tenant isolation on writes) and
    /// a model-finalizing convention (for query filters, composite PK expansion, and FK rewiring).
    /// </para>
    /// <para>
    /// Call this from <c>OnConfiguring</c> or a standalone options builder:
    /// <code>optionsBuilder.UseCoalesceMultiTenancy&lt;ITenanted&gt;(t =&gt; t.TenantId, nameof(TenantIdOrThrow));</code>
    /// No additional calls in <c>OnModelCreating</c> are needed.
    /// </para>
    /// </summary>
    /// <typeparam name="TTenanted">The interface or base class that marks an entity as tenant-scoped.</typeparam>
    /// <param name="builder">The db context options to configure.</param>
    /// <param name="tenantIdProperty">
    /// An expression identifying the tenant ID property on <typeparamref name="TTenanted"/>,
    /// e.g. <c>t =&gt; t.TenantId</c>.
    /// </param>
    /// <param name="contextTenantIdProperty">
    /// The name of the property on the <see cref="DbContext"/> that returns the current tenant ID value,
    /// e.g. <c>nameof(TenantIdOrThrow)</c>. This property is used in query filters and by the save interceptor.
    /// </param>
    public static DbContextOptionsBuilder UseCoalesceMultiTenancy<TTenanted>(
        this DbContextOptionsBuilder builder,
        Expression<Func<TTenanted, object>> tenantIdProperty,
        string contextTenantIdProperty
    )
        where TTenanted : class
    {
        var entityTenantIdPropertyName = tenantIdProperty.GetExpressedProperty().Name;

        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(
            new MultiTenancyExtension<TTenanted>(entityTenantIdPropertyName, contextTenantIdProperty));

        builder.AddInterceptors(new MultiTenancyInterceptor<TTenanted>(entityTenantIdPropertyName, contextTenantIdProperty));

        return builder;
    }

    /// <inheritdoc cref="UseCoalesceMultiTenancy{TTenanted}(DbContextOptionsBuilder, Expression{Func{TTenanted, object}}, string)"/>
    /// <param name="builder">The db context options to configure.</param>
    /// <param name="tenantIdProperty">
    /// An expression identifying the tenant ID property on <typeparamref name="TTenanted"/>,
    /// e.g. <c>t =&gt; t.TenantId</c>.
    /// </param>
    /// <param name="contextTenantIdProperty">
    /// An expression identifying the property on the <see cref="DbContext"/> that returns the current tenant ID value,
    /// e.g. <c>(MyDbContext db) =&gt; db.TenantIdOrThrow</c>.
    /// </param>
    public static DbContextOptionsBuilder UseCoalesceMultiTenancy<TTenanted>(
        this DbContextOptionsBuilder builder,
        Expression<Func<TTenanted, object>> tenantIdProperty,
        LambdaExpression contextTenantIdProperty
    )
        where TTenanted : class
    {
        return UseCoalesceMultiTenancy<TTenanted>(builder, tenantIdProperty, contextTenantIdProperty.GetExpressedProperty().Name);
    }
}
