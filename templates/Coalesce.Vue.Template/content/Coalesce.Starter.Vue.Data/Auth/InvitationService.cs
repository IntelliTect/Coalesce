using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Coalesce.Starter.Vue.Data.Auth;

public class InvitationService(
    AppDbContext db,
    IDataProtectionProvider dataProtector,
    IUrlHelper urlHelper
)
{
    private IDataProtector GetProtector() => dataProtector.CreateProtector("invitations");

    public async Task<ItemResult> CreateAndSendInvitation(
        string email,
        Role[] roles
    )
    {
        var tenantId = db.TenantIdOrThrow;

        if (roles.Any(r => r.TenantId != tenantId)) return "Role/tenant mismatch";

        var invitation = new UserInvitation
        {
            Email = email,
            Issued = DateTimeOffset.Now,
            Roles = roles.Select(r => r.Id).ToArray(),
            TenantId = db.TenantIdOrThrow
        };

        var user = await db.Users
            .Where(u => u.Email == email && u.EmailConfirmed)
            .FirstOrDefaultAsync();

        if (user is not null)
        {
            // Immediately accept the invitation if the user's email address already exists
            return await AcceptInvitation(invitation, user);
        }

        var link = CreateInvitationLink(invitation);

        // TODO: Implement email sending and send the invitation link directly to `email`.
        // Returning it directly in the result message is a temporary measure.

        return new(true, message: $"Give the following invitation link to {email}:\n\n{link}");
    }

    public async Task<ItemResult> AcceptInvitation(
        UserInvitation invitation,
        User acceptingUser
    )
    {
        var tenant = await db.Tenants.FindAsync(invitation.TenantId);
        if (tenant is null) return "Tenant not found";

        // Note: `acceptingUser` will be untracked after ForceSetTenant.
        db.ForceSetTenant(invitation.TenantId);

        if (await db.TenantMemberships.AnyAsync(m => m.User == acceptingUser))
        {
            return $"{acceptingUser.UserName ?? acceptingUser.Email} is already a member of {tenant.Name}.";
        }

        db.TenantMemberships.Add(new() { UserId = acceptingUser.Id });
        db.UserRoles.AddRange(invitation.Roles.Select(rid => new UserRole { RoleId = rid, UserId = acceptingUser.Id }));
        await db.SaveChangesAsync();

        return new(true, $"{acceptingUser.UserName ?? acceptingUser.Email} has been added as a member of {tenant.Name}.");
    }

    public string CreateInvitationLink(UserInvitation invitation)
    {
        var inviteJson = JsonSerializer.Serialize(invitation);
        var inviteCode = GetProtector().Protect(inviteJson);

        return urlHelper.PageLink("/Invitation", values: new { code = inviteCode })!;
    }

    public ItemResult<UserInvitation> DecodeInvitation(string code)
    {
        try
        {
            var unprotected = GetProtector().Unprotect(code);
            var invite = JsonSerializer.Deserialize<UserInvitation>(unprotected);

            if (invite is null || invite.Issued.AddHours(24) < DateTimeOffset.Now)
            {
                return "The invitation is no longer valid.";
            }

            return invite;
        }
        catch
        {
            return "The invitation is no longer valid.";
        }
    }
}
