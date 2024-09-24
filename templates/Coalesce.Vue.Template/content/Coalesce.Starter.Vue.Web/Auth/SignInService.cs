using Coalesce.Starter.Vue.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Coalesce.Starter.Vue.Data.Auth;

public class SignInService(
    AppDbContext db,
    SignInManager<User> signInManager,
    ILogger<SignInService> logger
)
{
#if GoogleAuth
    public async Task OnGoogleTicketReceived(TicketReceivedContext ctx)
    {
        var (user, remoteLoginInfo) = await GetOrCreateUser(ctx);
        if (user is null)
        {
            await Results.Text("Forbidden", statusCode: StatusCodes.Status403Forbidden).ExecuteAsync(ctx.HttpContext);
            ctx.HandleResponse();
            return;
        }

#if TenantCreateExternal
        // Note: domain will be null for personal gmail accounts.
        string? gSuiteDomain = ctx.Principal!.FindFirstValue("hd");

        if (string.IsNullOrWhiteSpace(gSuiteDomain))
        {
            await Forbid(ctx, "Personal Google accounts are not permitted.");
            return;
        }

        var tenant = await GetAndAssignUserExternalTenant(user, remoteLoginInfo, gSuiteDomain);
#endif

#if UserPictures
        // Populate or update user photo from Google
        await UpdateUserPhoto(user, ctx.Options.Backchannel,
            () => new HttpRequestMessage(HttpMethod.Get, ctx.Principal!.FindFirstValue("pictureUrl")));
#endif

        // OPTIONAL: Populate other fields on `user` specific to Google, if any.
        await signInManager.UserManager.UpdateAsync(user);

        await SignInExternalUser(ctx, remoteLoginInfo);
    }
#endif

#if MicrosoftAuth
    public async Task OnMicrosoftTicketReceived(TicketReceivedContext ctx)
    {
        var (user, remoteLoginInfo) = await GetOrCreateUser(ctx);
        if (user is null)
        {
            await Forbid(ctx);
            return;
        }

#if TenantCreateExternal
        var accessJtw = new JwtSecurityTokenHandler()
            .ReadJwtToken(ctx.Properties!.GetTokenValue(OpenIdConnectParameterNames.AccessToken));
        string? entraTenantId = accessJtw.Claims.FirstOrDefault(c => c.Type == "tid")?.Value;

        if (string.IsNullOrWhiteSpace(entraTenantId))
        {
            await Forbid(ctx, "Personal accounts are not permitted.");
            return;
        }

        var tenant = await GetAndAssignUserExternalTenant(user, remoteLoginInfo, entraTenantId);
#endif

#if UserPictures
        // Populate or update user photo from Microsoft Graph
        await UpdateUserPhoto(user, ctx.Options.Backchannel, () =>
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me/photos/96x96/$value");
            request.Headers.Authorization = new("Bearer", ctx.Properties!.GetTokenValue(OpenIdConnectParameterNames.AccessToken));
            return request;
        });
#endif

        // OPTIONAL: Populate additional fields on `user` specific to Microsoft, if any.
        await signInManager.UserManager.UpdateAsync(user);

        await SignInExternalUser(ctx, remoteLoginInfo);
    }

    private static async Task Forbid(TicketReceivedContext ctx, string message = "Forbidden")
    {
        await Results.Text(message, statusCode: StatusCodes.Status403Forbidden).ExecuteAsync(ctx.HttpContext);
        ctx.HandleResponse();
    }
#endif

#if (GoogleAuth || MicrosoftAuth)

    private async Task<(User? user, UserLoginInfo remoteLoginInfo)> GetOrCreateUser(
        TicketReceivedContext ctx
    )
    {
        var remoteProvider = ctx.Scheme.Name;
        var remoteUser = ctx.Principal!;
        var remoteUserId = remoteUser.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var remoteUserEmail = remoteUser.FindFirstValue(ClaimTypes.Email);

        var remoteLoginInfo = new UserLoginInfo(remoteProvider, remoteUserId, ctx.Scheme.DisplayName);

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
            // Don't match existing users by email if the email isn't confirmed.
            if (user?.EmailConfirmed == false) user = null;
        }

        if (user is null)
        {
            if (!await CanUserSignUpAsync(ctx, db, remoteUser))
            {
                return (null, remoteLoginInfo);
            }

            user = new User { UserName = remoteUserEmail };

#if Tenancy
            // If this user is the first user, make them the global admin
            if (!db.Users.Any())
            {
                user.IsGlobalAdmin = true;
            }
#else
            // If this user is the first user, give them all roles so there is an initial admin.
            if (!db.Users.Any())
            {
                user.UserRoles = db.Roles.Select(r => new UserRole { Role = r, User = user }).ToList();
            }
#endif

            await signInManager.UserManager.CreateAsync(user);
        }

        if (!foundByLogin)
        {
            await signInManager.UserManager.AddLoginAsync(user, remoteLoginInfo);
        }

        user.FullName = remoteUser.FindFirstValue(ClaimTypes.Name) ?? user.FullName;
        user.Email = remoteUserEmail ?? user.Email;
        user.EmailConfirmed = true;
        // OPTIONAL: Update any other properties on the user as desired.

        return (user, remoteLoginInfo);
    }


#if TenantCreateExternal
    private async Task<Tenant> GetAndAssignUserExternalTenant(
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
            db.Tenants.Add(tenant = new() { ExternalId = externalId, Name = user.Email?.Split('@').Last() ?? externalId });
            await db.SaveChangesAsync();

            db.TenantId = tenant.TenantId;
            new DatabaseSeeder(db).SeedNewTenant(tenant, user.Id);
        }
        db.TenantId = tenant.TenantId;

        var membership = await db.TenantMemberships.SingleOrDefaultAsync(tm => tm.TenantId == tenant.TenantId && tm.UserId == user.Id);
        if (membership is null)
        {
            membership = new() { TenantId = tenant.TenantId, UserId = user.Id };
            db.Add(membership);
            
            logger.LogInformation($"Automatically granting membership for user {user.Id} into tenant {tenant.TenantId} based on external tenant {externalId}");

            await db.SaveChangesAsync();
        }

        return tenant;
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

        var request = requestFactory();

        if (request.RequestUri is null) return;

        try
        {
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
                photo.SetTracking(user.Id);
            }
            user.PhotoMD5 = MD5.HashData(content);
        }
        catch { /* Failure is non-critical */ }
    }
#endif

    private Task<bool> CanUserSignUpAsync(TicketReceivedContext ctx, AppDbContext db, ClaimsPrincipal remoteUser)
    {
        // OPTIONAL: Examine the properties of `remoteUser` and determine if they're permitted to sign up.
        return Task.FromResult(true);
    }

    private async Task SignInExternalUser(TicketReceivedContext ctx, UserLoginInfo remoteLoginInfo)
    {
        // ExternalLoginSignInAsync checks that the user isn't locked out.
        var result = await signInManager.ExternalLoginSignInAsync(
            remoteLoginInfo.LoginProvider,
            remoteLoginInfo.ProviderKey,
            isPersistent: true,
            bypassTwoFactor: true);

        if (!result.Succeeded)
        {
            await Results.Text("Forbidden", statusCode: StatusCodes.Status403Forbidden).ExecuteAsync(ctx.HttpContext);
            ctx.HandleResponse();
            return;
        }

        ctx.HttpContext.Response.Redirect(ctx.ReturnUri ?? "/");
        ctx.HandleResponse();
    }

#endif
        }
