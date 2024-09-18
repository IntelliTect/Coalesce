namespace Coalesce.Starter.Vue.Data.Models;

[InternalUse]
[Index(nameof(UserId), nameof(TenantId), IsUnique = true)]
public class TenantMembership : TenantedBase
{
    public int TenantMembershipId { get; set; }

    [Required]
    public string UserId { get; set; } = default!;
    public User? User { get; set; }
}
