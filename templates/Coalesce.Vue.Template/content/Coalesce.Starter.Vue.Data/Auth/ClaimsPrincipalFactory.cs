using Coalesce.Starter.Vue.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security;

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

#if Tenancy
        if (user.IsGlobalAdmin)
        {
            identity.AddClaim(new Claim(identity.RoleClaimType, AppClaimTypes.GlobalAdminRole));
        }

        // TODO: Validate that the user is still a member of the tenant.
        // TODO: Allow pulling a new tenantID from the httpcontext Items
        // for tenant switching
        identity.AddClaim(new Claim(AppClaimTypes.TenantId, db.TenantIdOrThrow));
#endif

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

        ClaimsPrincipal result = new([identity, permissionIdentity]);

#if Tenancy
        if (!user.IsGlobalAdmin && result.IsInRole(AppClaimTypes.GlobalAdminRole))
        {
            // Safety/sanity check that the user hasn't been able to elevate to global admin
            // by some unexpected claim or permission fulfilling the global admin role check:
            throw new SecurityException($"User ${user.Id} unexpectly appears to be a global admin.");
        }
#endif
        return result;
    }
}
