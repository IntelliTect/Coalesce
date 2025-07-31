using Coalesce.Starter.Vue.Data;
using Coalesce.Starter.Vue.Data.Models;
using IntelliTect.Coalesce.Models;
using Microsoft.AspNetCore.Authentication;
#if GoogleAuth
using Microsoft.AspNetCore.Authentication.Google;
#endif
#if MicrosoftAuth
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
#endif
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Coalesce.Starter.Vue.Web.Pages;

[AllowAnonymous]
public class ExternalLoginModel(
    AppDbContext db,
    SignInManager<User> signInManager,
    ILogger<ExternalLoginModel> logger
) : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public string? ErrorMessage { get; set; }

    public IActionResult OnGet() => RedirectToPage("Login");

    public IActionResult OnPost(string provider)
    {
        // Request a redirect to the external login provider.
        var redirectUrl = Url.Page("ExternalLogin", pageHandler: "Callback", values: new { ReturnUrl });
        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
        return new ChallengeResult(provider, properties);
    }

    public async Task<IActionResult> OnGetCallbackAsync(string? remoteError = null)
    {
        if (remoteError != null)
        {
            ErrorMessage = $"Error from external provider: {remoteError}";
            return Page();
        }

        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info == null) return RedirectToPage("SignIn");

        switch (info.LoginProvider)
        {
#if GoogleAuth
            case GoogleDefaults.AuthenticationScheme:
                return await OnGoogleTicketReceived(info);
#endif
#if MicrosoftAuth
            case MicrosoftAccountDefaults.AuthenticationScheme:
                return await OnMicrosoftTicketReceived(info);
#endif
#if OtherOAuth
            case "MyOtherOAuth":
                return await OnMyOtherOAuthTicketReceived(info);
#endif
            default:
                ErrorMessage = "Unknown external provider";
                return Page();
        }
    }

#if GoogleAuth
    private async Task<IActionResult> OnGoogleTicketReceived(ExternalLoginInfo info)
    {
        var result = await GetOrCreateUser(info);
        if (!result.WasSuccessful || result.Object is not User user)
        {
            return Forbid(result.Message);
        }

#if TenantCreateExternal
        // Note: domain will be null for personal gmail accounts.
        string? gSuiteDomain = info.Principal!.FindFirstValue("hd");
        if (!string.IsNullOrWhiteSpace(gSuiteDomain))
        {
            await GetAndAssignUserExternalTenant(user, info, gSuiteDomain);
        }
#endif

#if UserPictures
        // Populate or update user photo from Google
        await UpdateUserPhoto(user,
            HttpContext.RequestServices.GetRequiredService<IOptions<GoogleOptions>>().Value.Backchannel,
            () => new HttpRequestMessage(HttpMethod.Get, info.Principal!.FindFirstValue("pictureUrl")));

#endif
        // OPTIONAL: Populate additional fields on `user` specific to Google, if any.

        await signInManager.UserManager.UpdateAsync(user);

        return await SignInExternalUser(info);
    }
#endif

#if MicrosoftAuth
    private async Task<IActionResult> OnMicrosoftTicketReceived(ExternalLoginInfo info)
    {
        var result = await GetOrCreateUser(info);
        if (!result.WasSuccessful || result.Object is not User user)
        {
            return Forbid(result.Message);
        }

#if TenantCreateExternal
        try
        {
            var accessJwt = new JwtSecurityTokenHandler()
                .ReadJwtToken(info.AuthenticationProperties!.GetTokenValue(OpenIdConnectParameterNames.AccessToken));
            string? entraTenantId = accessJwt.Claims.FirstOrDefault(c => c.Type == "tid")?.Value;

            if (entraTenantId is not null)
            {
                await GetAndAssignUserExternalTenant(user, info, entraTenantId);
            }
        }
        catch (SecurityTokenMalformedException)
        {
            // Expected for personal MSFT accounts, which cannot automatically create an external tenant.
            // Personal accounts use opaque access tokens, not JWTs.
        }
#endif

#if UserPictures
        // Populate or update user photo from Microsoft Graph
        await UpdateUserPhoto(user,
            HttpContext.RequestServices.GetRequiredService<IOptions<MicrosoftAccountOptions>>().Value.Backchannel,
            () =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me/photos/96x96/$value");
                request.Headers.Authorization = new("Bearer", info.AuthenticationProperties!.GetTokenValue(OpenIdConnectParameterNames.AccessToken));
                return request;
            });

#endif
        // OPTIONAL: Populate additional fields on `user` specific to Microsoft, if any.

        await signInManager.UserManager.UpdateAsync(user);

        return await SignInExternalUser(info);
    }
#endif


#if OtherOAuth
    private async Task<IActionResult> OnMyOtherOAuthTicketReceived(ExternalLoginInfo info)
    {
        var result = await GetOrCreateUser(info);
        if (!result.WasSuccessful || result.Object is not User user)
        {
            return Forbid(result.Message);
        }

#if TenantCreateExternal
        // TODO: Determine an identifier for the external tenant that will
        // be used to create and assign membership to a tenant within this application.
        string? externalTenantIdentifier = null;
        if (!string.IsNullOrWhiteSpace(externalTenantIdentifier))
        {
            await GetAndAssignUserExternalTenant(user, info, externalTenantIdentifier);
        }
#endif

#if UserPictures
        await UpdateUserPhoto(user,
            // NOTE: If you switch to using a dedicated NuGet package for your OAuth provider, your options type might not be `OAuthOptions`.
            HttpContext.RequestServices.GetRequiredKeyedService<IOptions<OAuthOptions>>(info.ProviderKey).Value.Backchannel,
            () => throw new NotImplementedException("Implement code to construct an HttpRequestMessage that will fetch a profile picture from the OAuth provider, or delete this code if that is not possible."));

#endif
        // OPTIONAL: Populate additional fields on `user` specific to your provider, if any.

        await signInManager.UserManager.UpdateAsync(user);

        return await SignInExternalUser(info);
    }
#endif

    private async Task<ItemResult<User>> GetOrCreateUser(ExternalLoginInfo info)
    {
        var remoteProvider = info.LoginProvider;
        var remoteUser = info.Principal!;
        var remoteUserId = remoteUser.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var remoteUserEmail = remoteUser.FindFirstValue(ClaimTypes.Email);

        // Find by the external user ID
        bool foundByLogin = false;
        User? user = await signInManager.UserManager.FindByLoginAsync(remoteProvider, remoteUserId);

        // If not found, look for an existing user by email address
        if (user is not null)
        {
            foundByLogin = true;
        }
        else if (remoteUserEmail is not null)
        {
            user = await signInManager.UserManager.FindByEmailAsync(remoteUserEmail);
            if (user?.EmailConfirmed == false)
            {
                // Don't match existing users by email if the email isn't confirmed.
#if (!LocalAuth)
                // Note: this error message assumes that the only way an unverified account can exist
                // is if the application has local user accounts. Customize this message if needed.
#endif
                // https://security.stackexchange.com/questions/260562/adding-sso-to-an-existing-website-should-sso-login-link-to-matching-email-addr
                // It is crucial for security that you don't just mark the existing account as verified,
                // as it may have a password attached that is controlled/known by a different person
                // who is squatting on an email address in the system on hopes of hijacking the account.
                // The person who owns the current verified external login needs to sign into that account with its password,
                // including performing a "forgot password" request if the password isn't actually known.
                return $"An existing unverified user account with email address {remoteUserEmail} already exists. " +
                    $"You must log into this account with its username and password and verify the account's email address " +
                    $"before you can link the account to your {info.ProviderDisplayName} login.";
            }
        }

        if (user is null)
        {
            if (await CanUserSignUpAsync(info) is { WasSuccessful: false } canSignIn) return new(canSignIn);

            user = new User { UserName = remoteUserEmail, Email = remoteUserEmail, EmailConfirmed = true };

            new DatabaseSeeder(db).InitializeFirstUser(user);

            var result = await signInManager.UserManager.CreateAsync(user);
            if (!result.Succeeded) return string.Join(", ", result.Errors);
        }

        if (!foundByLogin)
        {
            var result = await signInManager.UserManager.AddLoginAsync(user, info);
            if (!result.Succeeded) return string.Join(", ", result.Errors);
        }

        user.FullName = remoteUser.FindFirstValue(ClaimTypes.Name) ?? user.FullName;
        if (!string.IsNullOrWhiteSpace(remoteUserEmail))
        {
            user.Email = remoteUserEmail;
            user.EmailConfirmed = true;
        }
        // OPTIONAL: Update any other properties on the user as desired.

        return user;
    }


#if TenantCreateExternal
    private async Task GetAndAssignUserExternalTenant(
        User user,
        UserLoginInfo userLoginInfo,
        string externalTenantId
    )
    {
        var externalId = $"{userLoginInfo.LoginProvider}:{externalTenantId}";

        var tenant = await db.Tenants.SingleOrDefaultAsync(t => t.ExternalId == externalId);
        if (tenant is null)
        {
            // Automatically create a tenant in our application based on the external tenant.
            db.Tenants.Add(tenant = new()
            {
                ExternalId = externalId,
                Name = user.Email?.Split('@').Last() ?? externalId
            });
            await db.SaveChangesAsync();

            new DatabaseSeeder(db).SeedNewTenant(tenant, user.Id);
        }
        db.TenantId = tenant.TenantId;

        var membership = await db.TenantMemberships.SingleOrDefaultAsync(tm => tm.TenantId == tenant.TenantId && tm.UserId == user.Id);
        if (membership is null)
        {
            membership = new() { TenantId = tenant.TenantId, UserId = user.Id };
            db.Add(membership);

            logger.LogInformation(
                "Automatically granting membership for user {UserId} into tenant {TenantId} based on external tenant {ExternalId}",
                user.Id, tenant.TenantId, externalId);

            await db.SaveChangesAsync();
        }
    }
#endif

#if UserPictures
    private async Task UpdateUserPhoto(User user, HttpClient client, Func<HttpRequestMessage> requestFactory)
    {
        UserPhoto? photo = user.Photo = db.UserPhotos.Where(p => p.UserId == user.Id).FirstOrDefault();
        if (photo is not null && photo.ModifiedOn >= DateTimeOffset.Now.AddDays(-7))
        {
            // User photo already populated and reasonably recent.
            return;
        }

        try
        {
            var request = requestFactory();

            if (request.RequestUri is null) return;

            var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode) return;

            byte[] content = await response.Content.ReadAsByteArrayAsync();

            if (content is not { Length: > 0 }) return;

            if (photo is null)
            {
                user.Photo = photo = new UserPhoto { UserId = user.Id, Content = content };
            }
            else
            {
                photo.Content = content;
            }
#if TrackingBase
            photo.SetTracking(user.Id);
#else
            photo.ModifiedOn = DateTimeOffset.Now;
#endif
            user.PhotoHash = MD5.HashData(content);
        }
        catch { /* Failure is non-critical */ }
    }
#endif

    private Task<ItemResult> CanUserSignUpAsync(ExternalLoginInfo remoteLoginInfo)
    {
        // OPTIONAL: Examine the properties of `remoteLoginInfo` and determine if the user is permitted to sign up.
        return Task.FromResult(new ItemResult(true));
    }

    private async Task<IActionResult> SignInExternalUser(ExternalLoginInfo remoteLoginInfo)
    {
        // ExternalLoginSignInAsync checks that the user isn't locked out.
        var result = await signInManager.ExternalLoginSignInAsync(
            remoteLoginInfo.LoginProvider,
            remoteLoginInfo.ProviderKey,
            isPersistent: true,
            bypassTwoFactor: true);

        if (!result.Succeeded)
        {
            return Forbid(result.IsLockedOut ? "Account locked." : "Unable to sign in.");
        }

        // OPTIONAL: If desired, save the user's OAuth tokens for later user:
        // (you'll need to request offline_access OAuth scope for this to be of any real use).
        // await signInManager.UpdateExternalAuthenticationTokensAsync(remoteLoginInfo);

        return LocalRedirect(ReturnUrl ?? "/");
    }

    private IActionResult Forbid(string? message = null)
    {
        message ??= "Forbidden";

        ErrorMessage = message;

        return new PageResult { StatusCode = StatusCodes.Status403Forbidden };
    }
}
