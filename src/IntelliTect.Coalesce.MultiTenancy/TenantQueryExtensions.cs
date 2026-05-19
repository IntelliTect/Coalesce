#if NET10_0_OR_GREATER
using IntelliTect.Coalesce.MultiTenancy;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Extension methods for querying tenanted entities.
/// </summary>
public static class TenantQueryExtensions
{
    /// <summary>
    /// Perform an untracked query without any tenant filtering.
    /// Results are returned with <see cref="EntityFrameworkQueryableExtensions.AsNoTrackingWithIdentityResolution{TEntity}"/>
    /// to prevent accidental cross-tenant modifications via change tracking.
    /// </summary>
    public static IQueryable<T> IgnoreTenantFilter<T>(this IQueryable<T> query)
        where T : class
        => query.IgnoreQueryFilters([MultiTenancyConvention.QueryFilterName]).AsNoTrackingWithIdentityResolution();

    /// <summary>
    /// Perform a query without any tenant filtering, keeping change tracking enabled.
    /// Use with caution — tracked cross-tenant entities can be accidentally modified and saved.
    /// </summary>
    public static IQueryable<T> IgnoreTenantFilterTracked<T>(this IQueryable<T> query)
        where T : class
        => query.IgnoreQueryFilters([MultiTenancyConvention.QueryFilterName]);
}
#endif
