namespace Coalesce.Starter.Vue.Data.Models;

[Index(nameof(UserId), nameof(TenantId), IsUnique = true)]
[InternalUse]
public class TenantMembership : TenantedBase
{
    public int TenantMembershipId { get; set; }

    [Required]
    public string UserId { get; set; } = default!;
    public User? User { get; set; }
}
