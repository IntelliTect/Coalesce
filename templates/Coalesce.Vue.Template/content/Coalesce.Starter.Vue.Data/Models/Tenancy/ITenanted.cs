namespace Coalesce.Starter.Vue.Data.Models;

/// <summary>
/// The model belongs to a tenant and should be filtered to the tenant of the current user/HTTP request.
/// </summary>
public interface ITenanted
{
    string TenantId { get; set; }
    Tenant? Tenant { get; set; }
}