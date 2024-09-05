using Coalesce.Starter.Vue.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Coalesce.Starter.Vue.Data.Auth;

public class ClaimsPrincipalFactory(
    AppDbContext db,
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    IOptions<IdentityOptions> options
) : UserClaimsPrincipalFactory<User, Role>(userManager, roleManager, options)
{
    public override async Task<ClaimsPrincipal> CreateAsync(User user)
    {
        var identity = await GenerateClaimsAsync(user);

        // Attach additional custom claims
        identity.AddClaim(new Claim(AppClaimTypes.FullName, user.FullName ?? ""));

        // Store all the permissions in a dedicated identity
        // whose RoleClaimType is Permission so that they can still be treated like roles
        // (and so they work with IsInRole and coalesce attribute-based security).
        var permissions = (await db.Entry(user)
            .Collection(u => u.UserRoles!)
            .Query()
            .Select(r => r.Role!)
            .ToListAsync())
            .SelectMany(role => role.Permissions!)
            .Select(p => new Claim(AppClaimTypes.Permission, p.ToString()));

        var permissionIdentity = new ClaimsIdentity(
            permissions, 
            "Permissions",
            Options.ClaimsIdentity.UserNameClaimType,
            AppClaimTypes.Permission);

        return new ClaimsPrincipal([identity, permissionIdentity]);
    }
}
