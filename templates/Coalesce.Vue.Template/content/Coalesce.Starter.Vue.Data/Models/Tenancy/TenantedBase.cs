namespace Coalesce.Starter.Vue.Data.Models;

public abstract class TenantedBase
#if TrackingBase
    : TrackingBase, ITenanted
#else
    : ITenanted
#endif
{
    [InternalUse]
    public int TenantId { get; set; }
    [InternalUse]
    public Tenant? Tenant { get; set; }
}
