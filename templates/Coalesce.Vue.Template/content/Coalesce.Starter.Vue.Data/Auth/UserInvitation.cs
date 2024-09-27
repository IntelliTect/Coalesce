namespace Coalesce.Starter.Vue.Data.Auth;

public class UserInvitation
{
    public required string TenantId { get; set; }

    public required string Email { get; set; }

    public required DateTimeOffset Issued { get; set; }

    public required string[] Roles { get; set; }
}
