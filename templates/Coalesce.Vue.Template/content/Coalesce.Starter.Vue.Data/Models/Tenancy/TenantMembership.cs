namespace Coalesce.Starter.Vue.Data.Models;

[Index(nameof(UserId), nameof(TenantId), IsUnique = true)]
[InternalUse]
public class TenantMembership : TenantedBase
{
    public string TenantMembershipId { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string UserId { get; set; } = default!;
    public User? User { get; set; }
}
