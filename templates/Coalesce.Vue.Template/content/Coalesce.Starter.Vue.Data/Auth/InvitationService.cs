using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Coalesce.Starter.Vue.Data.Auth;

public class InvitationService(
    IDataProtectionProvider dataProtector,
    IUrlHelper urlHelper
)
{
    private IDataProtector GetProtector() => dataProtector.CreateProtector("invitations");

    public string CreateInvitationLink(UserInvitation invitation)
    {
        var inviteJson = JsonSerializer.Serialize(invitation);
        var inviteCode = GetProtector().Protect(inviteJson);

        return urlHelper.PageLink("/invitation", values: new { code = inviteCode })!;
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
