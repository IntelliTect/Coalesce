using IntelliTect.Coalesce;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Coalesce.Starter.Vue.Data.Auth;

public class UserPasskeyInfo
{
    public required byte[] CredentialId { get; set; }
    public string? Name { get; set; }
    public DateTimeOffset CreatedOn { get; set; }
}

[Coalesce, Service]
public class PasskeyService(
    UserManager<User> userManager,
    SignInManager<User> signInManager
)
{
    [Coalesce, Execute(AllowAll)]
    public async Task<string> GetRequestOptions(string? username = null)
    {
        var user = string.IsNullOrEmpty(username) ? null : await userManager.FindByNameAsync(username);
        var optionsJson = await signInManager.MakePasskeyRequestOptionsAsync(user);
        return optionsJson;
    }

    [Coalesce]
    public async Task<string> GetCreationOptions(ClaimsPrincipal principal)
    {
        var user = await userManager.GetUserAsync(principal);
        if (user == null) throw new InvalidOperationException("User not found");

        var userId = await userManager.GetUserIdAsync(user);
        var userName = await userManager.GetUserNameAsync(user) ?? "User";

        var optionsJson = await signInManager.MakePasskeyCreationOptionsAsync(new()
        {
            Id = userId,
            Name = userName,
            DisplayName = userName
        });

        return optionsJson;
    }

    [Coalesce]
    public async Task<ICollection<UserPasskeyInfo>> GetPasskeys(ClaimsPrincipal principal)
    {
        var user = await userManager.GetUserAsync(principal);
        if (user == null) throw new InvalidOperationException("User not found");

        var passkeys = await userManager.GetPasskeysAsync(user);
        return passkeys.Select(p => new UserPasskeyInfo
        {
            CredentialId = p.CredentialId,
            Name = p.Name,
            CreatedOn = p.CreatedAt
        }).ToList();
    }

    [Coalesce]
    public async Task<ItemResult> AddPasskey(ClaimsPrincipal principal, string credentialJson, string? name = null)
    {
        var user = await userManager.GetUserAsync(principal);
        if (user == null) return "User not found";

        var attestationResult = await signInManager.PerformPasskeyAttestationAsync(credentialJson);
        if (!attestationResult.Succeeded)
        {
            return $"Could not add the passkey: {attestationResult.Failure.Message}";
        }

        var passkey = attestationResult.Passkey;
        if (!string.IsNullOrWhiteSpace(name))
        {
            passkey.Name = name;
        }

        var addPasskeyResult = await userManager.AddOrUpdatePasskeyAsync(user, passkey);
        if (!addPasskeyResult.Succeeded)
        {
            return "The passkey could not be added to your account.";
        }

        return true;
    }

    [Coalesce]
    public async Task<ItemResult> RenamePasskey(ClaimsPrincipal principal, byte[] credentialId, string name)
    {
        var user = await userManager.GetUserAsync(principal);
        if (user == null) return "User not found";

        var passkey = await userManager.GetPasskeyAsync(user, credentialId);
        if (passkey == null)
        {
            return "The specified passkey could not be found.";
        }

        passkey.Name = name;
        var result = await userManager.AddOrUpdatePasskeyAsync(user, passkey);
        if (!result.Succeeded)
        {
            return "The passkey could not be updated.";
        }

        return new(true, "Passkey updated successfully.");
    }

    [Coalesce]
    public async Task<ItemResult> DeletePasskey(ClaimsPrincipal principal, byte[] credentialId)
    {
        var user = await userManager.GetUserAsync(principal);
        if (user == null) return "User not found";

        var result = await userManager.RemovePasskeyAsync(user, credentialId);
        if (!result.Succeeded)
        {
            return "The passkey could not be deleted.";
        }

        return new(true, "Passkey deleted successfully.");
    }
}


