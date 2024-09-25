namespace Coalesce.Starter.Vue.Data.Models;

[Index(nameof(UserId), nameof(TenantId), IsUnique = true)]
[InternalUse]
public class TenantMembership : TenantedBase
{
    [Key]
    [ForeignKey(nameof(User))]
    public required string UserId { get; set; }

    public User? User { get; set; }

    // NB: The other half of this many-to-many join table is the Tenant, which comes from TenantedBase.
}
