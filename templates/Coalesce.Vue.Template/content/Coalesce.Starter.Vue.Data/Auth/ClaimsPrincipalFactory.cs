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
#if Tenancy
        var tenantId = db.TenantId;
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            // User doesn't have a selected tenant. Pick one for them.
            var membership = await db.TenantMemberships
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(tm => tm.UserId == user.Id);
            
            // Default to the "null" tenant if the user belongs to no tenants.
            // This allows the rest of the sign-in process to function,
            // but will never match a real tenant or produce real roles/permissions.
            // This allows new users to accept invitations or create their own tenant.
            tenantId = db.TenantId = membership?.TenantId ?? AppClaimValues.NullTenantId;
        }

#endif
        var identity = await GenerateClaimsAsync(user);

        // Attach additional custom claims
        identity.AddClaim(new Claim(AppClaimTypes.FullName, user.FullName ?? ""));

#if Tenancy
        if (user.IsGlobalAdmin)
        {
            identity.AddClaim(new Claim(identity.RoleClaimType, AppClaimValues.GlobalAdminRole));
        }

        // TODO: Validate that the user is still a member of the tenant.
        // TODO: Allow pulling a new tenantID from the httpcontext Items
        // for tenant switching
        identity.AddClaim(new Claim(AppClaimTypes.TenantId, tenantId));
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
        if (!user.IsGlobalAdmin && result.IsInRole(AppClaimValues.GlobalAdminRole))
        {
            // Safety/sanity check that the user hasn't been able to elevate to global admin
            // by some unexpected claim or permission fulfilling the global admin role check:
            throw new SecurityException($"User ${user.Id} unexpectly appears to be a global admin.");
        }
#endif
        return result;
    }
}
