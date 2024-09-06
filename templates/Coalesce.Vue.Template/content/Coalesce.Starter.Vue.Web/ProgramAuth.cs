using Coalesce.Starter.Vue.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Data;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Coalesce.Starter.Vue.Data.Auth;

public static class ProgramAuth
{
    public static void ConfigureAuthentication(this WebApplicationBuilder builder)
    {
        var config = builder.Configuration;

        builder.Services
            .AddIdentity<User, Role>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders()
            .AddClaimsPrincipalFactory<ClaimsPrincipalFactory>();

        builder.Services
            .AddAuthentication()
#if GoogleAuth
            .AddGoogle(options =>
            {
                options.ClientId = config["Authentication:Google:ClientId"]!;
                options.ClientSecret = config["Authentication:Google:ClientSecret"]!;
                options.ClaimActions.MapJsonKey("hd", "hd"); // Hosted domain (i.e. GSuite domain).
#if UserPictures
                options.ClaimActions.MapJsonKey("pictureUrl", "picture");
#endif

                options.Events.OnTicketReceived = OnTicketReceived(async (user, db, ctx) =>
                {
                    // OPTIONAL: Populate fields on `user` specific to Google, if any.

                    // NOTE: If needed, the GSuite domain (if any) of the user can be acquired as follows:
                    //var domain = ctx.Principal!.FindFirstValue("hd")
#if UserPictures
                    // Populate or update user photo from Google
                    await UpdateUserPhoto(user, db, ctx.Options.Backchannel, 
                        () => new HttpRequestMessage(HttpMethod.Get, ctx.Principal!.FindFirstValue("pictureUrl")));
#endif
                });
             })
#endif
#if MicrosoftAuth
            .AddMicrosoftAccount(options =>
            {
                options.ClientId = config["Authentication:Microsoft:ClientId"]!;
                options.ClientSecret = config["Authentication:Microsoft:ClientSecret"]!;
#if UserPictures
                options.SaveTokens = true;
#endif

                options.Events.OnTicketReceived = OnTicketReceived(async (user, db, ctx) =>
                {
                    // OPTIONAL: Populate additional fields on `user` specific to Microsoft, if any.

                    // NOTE: If needed, the Entra/AAD TenantID of the user can be acquired as follows:
                    //var tenantId = new JwtSecurityTokenHandler()
                    //    .ReadJwtToken(ctx.Properties!.GetTokenValue(OpenIdConnectParameterNames.AccessToken))
                    //    .Claims.First(c => c.Type == "tid").Value;

#if UserPictures
                    // Populate or update user photo from Microsoft Graph
                    await UpdateUserPhoto(user, db, ctx.Options.Backchannel, () => {
                        var request = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me/photos/96x96/$value");
                        request.Headers.Authorization = new("Bearer", ctx.Properties!.GetTokenValue(OpenIdConnectParameterNames.AccessToken));
                        return request;
                    });
#endif
                });
            })
#endif
            ;

        builder.Services.Configure<SecurityStampValidatorOptions>(o =>
        {
            // Configure how often to refresh user claims and validate
            // that the user is still allowed to sign in.
            o.ValidationInterval = TimeSpan.FromMinutes(5);
        });

        builder.Services.ConfigureApplicationCookie(c =>
        {
            c.LoginPath = "/sign-in"; // Razor page "Pages/SignIn.cshtml"
        });
    }

#if UserPictures
    private static async Task UpdateUserPhoto(User user, AppDbContext db, HttpClient client, Func<HttpRequestMessage> buildRequest)
    {
        UserPhoto? photo = user.Photo = db.UserPhotos.Where(p => p.UserId == user.Id).FirstOrDefault();
        if (photo is not null && photo.ModifiedOn >= DateTimeOffset.Now.AddDays(-7))
        {
            // User photo already populated and reasonably recent.
            return;
        }

        var request = buildRequest();

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

#if (GoogleAuth || MicrosoftAuth)
    private static Func<TicketReceivedContext, Task> OnTicketReceived(
        Func<User, AppDbContext, TicketReceivedContext, Task>? updateUser = null
    ) => async (TicketReceivedContext ctx) =>
    {
        var db = ctx.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        var signInManager = ctx.HttpContext.RequestServices.GetRequiredService<SignInManager<User>>();

        var (user, remoteLoginInfo) = await GetOrCreateUser(ctx, db, signInManager);
        if (user is null)
        {
            await Results.Text("Forbidden", statusCode: StatusCodes.Status403Forbidden).ExecuteAsync(ctx.HttpContext);
            ctx.HandleResponse();
            return;
        }

        if (updateUser is not null) await updateUser(user, db, ctx);

        await signInManager.UserManager.UpdateAsync(user);

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
    };

    private static async Task<(User? user, UserLoginInfo remoteLoginInfo)> GetOrCreateUser(
        TicketReceivedContext ctx, 
        AppDbContext db, 
        SignInManager<User> signInManager
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
        }

        if (user is null)
        {
            if (!await CanUserSignUpAsync(ctx, db, remoteUser))
            {
                return (null, remoteLoginInfo);
            }

            user = new User { UserName = remoteUserEmail };

            // If this user is the first user, give them all roles so the system has an admin.
            if (!db.Users.Any())
            {
                user.UserRoles = db.Roles.Select(r => new UserRole { Role = r, User = user }).ToList();
            }

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

    private static Task<bool> CanUserSignUpAsync(TicketReceivedContext ctx, AppDbContext db, ClaimsPrincipal remoteUser)
    {
        // OPTIONAL: Examine the properties of `remoteUser` and determine if they're permitted to sign up.
        return Task.FromResult(true);
    }

#endif
}
