namespace Coalesce.Starter.Vue.Data.Auth;

public class UserInfo
{
    public string? Id { get; set; }

    public string? UserName { get; set; }

#if Identity
    public string? FullName { get; set; }

    public required ICollection<string> Roles { get; set; }
    public required ICollection<string> Permissions { get; set; }
#endif
}