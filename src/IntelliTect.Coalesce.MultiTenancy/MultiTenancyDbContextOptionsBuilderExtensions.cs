using IntelliTect.Coalesce.MultiTenancy;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Linq;
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
    /// Call this from <c>OnConfiguring</c>:
    /// <code>optionsBuilder.UseCoalesceMultiTenancy&lt;ITenanted, string&gt;(t =&gt; t.TenantId, () =&gt; TenantIdOrThrow);</code>
    /// No additional calls in <c>OnModelCreating</c> are needed.
    /// </para>
    /// </summary>
    /// <typeparam name="TTenanted">The interface or base class that marks an entity as tenant-scoped.</typeparam>
    /// <typeparam name="TKey">The type of the tenant ID property (e.g. <c>string</c> or <c>int</c>).</typeparam>
    /// <param name="builder">The db context options to configure.</param>
    /// <param name="tenantIdProperty">
    /// An expression identifying the tenant ID property on <typeparamref name="TTenanted"/>,
    /// e.g. <c>t =&gt; t.TenantId</c>.
    /// </param>
    /// <param name="tenantIdExpression">
    /// A zero-argument expression that returns the current tenant ID, e.g. <c>() =&gt; TenantIdOrThrow</c>.
    /// The expression body is embedded directly into query filters; EF Core will substitute the
    /// correct <see cref="DbContext"/> instance at query-execution time.
    /// It is also used by the save interceptor and value generators.
    /// </param>
    public static DbContextOptionsBuilder UseCoalesceMultiTenancy<TTenanted, TKey>(
        this DbContextOptionsBuilder builder,
        Expression<Func<TTenanted, TKey>> tenantIdProperty,
        Expression<Func<TKey>> tenantIdExpression
    )
        where TTenanted : class
    {
        var tenantIdPropertyName = GetPropertyName(tenantIdProperty);
        var getTenantId = MultiTenancyConvention<TTenanted>.BuildTenantIdGetter(tenantIdExpression);

        // Detect provider name from already-registered database provider extension.
        // This works because UseXxx() (e.g. UseSqlite, UseInMemoryDatabase) is called before UseMultiTenancy.
        var providerName = builder.Options.Extensions
            .FirstOrDefault(e => e.Info.IsDatabaseProvider)
            ?.GetType().Assembly.GetName().Name;

        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(
            new MultiTenancyExtension<TTenanted>(tenantIdPropertyName, providerName, tenantIdExpression));

        builder.AddInterceptors(new MultiTenancyInterceptor<TTenanted>(tenantIdPropertyName, getTenantId));

        return builder;
    }

    private static string GetPropertyName<TTenanted, TKey>(Expression<Func<TTenanted, TKey>> propertyExpression)
    {
        if (propertyExpression.Body is MemberExpression memberExpr)
        {
            return memberExpr.Member.Name;
        }
        throw new ArgumentException(
            "tenantIdProperty must be a simple property access expression, e.g. t => t.TenantId",
            nameof(propertyExpression));
    }
}
