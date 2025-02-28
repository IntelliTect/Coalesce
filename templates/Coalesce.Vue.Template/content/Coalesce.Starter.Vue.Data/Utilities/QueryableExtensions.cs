namespace Coalesce.Starter.Vue.Data.Utilities;

public static class QueryableExtensions
{
#if Tenancy
    /// <summary>
    /// Perform an untracked query without any tenant filtering.
    /// </summary>
    public static IQueryable<T> IgnoreTenancy<T>(this IQueryable<T> query)
        where T : class
        => query.IgnoreQueryFilters().AsNoTrackingWithIdentityResolution();
#endif
}