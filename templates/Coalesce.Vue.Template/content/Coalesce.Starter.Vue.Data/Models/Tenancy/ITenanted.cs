namespace Coalesce.Starter.Vue.Data.Models;

/// <summary>
/// The model belongs to a tenant and should be filtered to the tenant of the current user/HTTP request.
/// </summary>
public interface ITenanted
{
    int TenantId { get; set; }
    Tenant? Tenant { get; set; }

    static virtual IQueryable<T> WhereTenantMatches<T>(IQueryable<T> query, int tenantId)
        where T : ITenanted
        => query.Where(x => x.TenantId == tenantId);
}