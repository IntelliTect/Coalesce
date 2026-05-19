#if NET10_0_OR_GREATER
using IntelliTect.Coalesce.MultiTenancy;

namespace Microsoft.EntityFrameworkCore;

/// <summary>
/// Extension methods for querying tenanted entities.
/// </summary>
public static class TenantQueryExtensions
{
    /// <summary>
    /// Perform a query without any tenant filtering.
    /// </summary>
    public static IQueryable<T> IgnoreTenantFilter<T>(this IQueryable<T> query)
        where T : class
        => query.IgnoreQueryFilters([MultiTenancyConvention.QueryFilterName]);
}
#endif
