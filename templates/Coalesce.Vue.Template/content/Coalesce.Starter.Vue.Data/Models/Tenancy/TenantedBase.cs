namespace Coalesce.Starter.Vue.Data.Models;

public abstract class TenantedBase
#if TrackingBase
    : TrackingBase, ITenanted
#else
    : ITenanted
#endif
{
    [InternalUse, Required]
    public string TenantId { get; set; } = null!;
    [InternalUse]
    public Tenant? Tenant { get; set; }
}
