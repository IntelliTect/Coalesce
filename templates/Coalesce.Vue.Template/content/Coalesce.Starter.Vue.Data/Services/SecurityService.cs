using IntelliTect.Coalesce.Utilities;

namespace Coalesce.Starter.Vue.Data.Services;
[Coalesce, Service]
public class SecurityService()
{

    [Coalesce, Execute(HttpMethod = HttpMethod.Get)]
    public UserInfo WhoAmI(ClaimsPrincipal user)
    {
        return new UserInfo
        {
            Id = user.GetUserId(),
            UserName = user.Identity.Name,

            Roles = user.Claims
                .Where(c => c.Type == AppClaimTypes.Role)
                .Select(c => c.Value)
                .ToList(),

            Permissions = user.Claims
                .Where(c => c.Type == AppClaimTypes.Permission)
                .Select(c => c.Value)
                .ToList(),
        };
    }
}

public class UserInfo
{
    public required string Id { get; set; }

    public required string UserName { get; set; }

    public required ICollection<string> Roles { get; set; }
    public required ICollection<string> Permissions { get; set; }
}