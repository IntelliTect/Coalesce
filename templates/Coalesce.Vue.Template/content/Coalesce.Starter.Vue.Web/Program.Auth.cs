using Coalesce.Starter.Vue.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Coalesce.Starter.Vue.Data.Auth;

public static class AuthenticationConfiguration
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
#if UserPictures
                options.ClaimActions.MapJsonKey("pictureUrl", "picture");
#endif

                options.Events.OnTicketReceived = OnTicketReceived(async (user, db, ctx) =>
                {
                    // OPTIONAL: Populate fields on `user` specific to Google, if any.

#if UserPictures
                    UserPhoto? photo = db.UserPhotos.Where(p => p.UserId == user.Id).FirstOrDefault();
                    if (photo is null || photo.ModifiedOn < DateTimeOffset.Now.AddDays(-7))
                    {
                        var photoUrl = ctx.Principal!.FindFirstValue("pictureUrl");
                        var request = new HttpRequestMessage(HttpMethod.Get, photoUrl);
                        var response = await ctx.Options.Backchannel.SendAsync(request);
                        if (response.IsSuccessStatusCode)
                        {
                            byte[] content = await response.Content.ReadAsByteArrayAsync();
                            if (photo is null)
                            {
                                db.Add(photo = new UserPhoto { UserId = user.Id, Content = content });
                            }
                            else
                            {
                                photo.Content = content;
                                photo.SetTracking(user.Id);
                            }
                            user.PhotoMD5 = MD5.HashData(content);
                        }
                    }
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

#if UserPictures
                    UserPhoto? photo = db.UserPhotos.Where(p => p.UserId == user.Id).FirstOrDefault();
                    if (photo is null || photo.ModifiedOn < DateTimeOffset.Now.AddDays(-7))
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, "https://graph.microsoft.com/v1.0/me/photos/96x96/$value");
                        var token = ctx.Properties!.GetTokenValue(OpenIdConnectParameterNames.AccessToken);
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                        var response = await ctx.Options.Backchannel.SendAsync(request);
                        if (response.IsSuccessStatusCode)
                        {
                            byte[] content = await response.Content.ReadAsByteArrayAsync();
                            if (photo is null)
                            {
                                db.Add(photo = new UserPhoto { UserId = user.Id, Content = content });
                            }
                            else
                            {
                                photo.Content = content;
                                photo.SetTracking(user.Id);
                            }
                            user.PhotoMD5 = MD5.HashData(content);
                        }
                    }
#endif
                });
            })
#endif
            ;

        builder.Services.Configure<SecurityStampValidatorOptions>(o =>
        {
            // Configure how often to refresh user claims and validate
            // that the user is still allowed to sign in.
            o.ValidationInterval = TimeSpan.FromMinutes(10);
        });

        builder.Services.ConfigureApplicationCookie(c =>
        {
            c.LoginPath = "/sign-in"; // Razor page "Pages/SignIn.cshtml"
        });
    }

#if GoogleAuth || MicrosoftAuth
    private static Func<TicketReceivedContext, Task> OnTicketReceived(
        Func<User, AppDbContext, TicketReceivedContext, Task>? updateUser = null
    ) => async (TicketReceivedContext ctx) =>
    {
        var db = ctx.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        var signInManager = ctx.HttpContext.RequestServices.GetRequiredService<SignInManager<User>>();

        var remoteProvider = ctx.Scheme.Name;
        var remoteUser = ctx.Principal!;
        var remoteUserId = remoteUser.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var remoteUserEmail = remoteUser.FindFirstValue(ClaimTypes.Email);

        // Find by the external user ID
        bool foundByLogin = false;
        var user = await signInManager.UserManager.FindByLoginAsync(remoteProvider, remoteUserId);

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
                await Results.Text("Forbidden", statusCode: StatusCodes.Status403Forbidden).ExecuteAsync(ctx.HttpContext);
                ctx.HandleResponse();
                return;
            }

            user = new User { UserName = remoteUserEmail };
            await signInManager.UserManager.CreateAsync(user);
        }

        user.FullName = remoteUser.FindFirstValue(ClaimTypes.Name) ?? user.FullName;
        user.Email = remoteUserEmail ?? user.Email;
        // OPTIONAL: Update any other properties on the user as desired.

        if (updateUser is not null) await updateUser(user, db, ctx);

        await signInManager.UserManager.UpdateAsync(user);

        if (!foundByLogin)
        {
            await signInManager.UserManager.AddLoginAsync(user, new UserLoginInfo(remoteProvider, remoteUserId, ctx.Scheme.DisplayName));
        }

        var result = await signInManager.ExternalLoginSignInAsync(remoteProvider, remoteUserId, isPersistent: true, bypassTwoFactor: true);
        if (!result.Succeeded)
        {
            await Results.Text("Forbidden", statusCode: StatusCodes.Status403Forbidden).ExecuteAsync(ctx.HttpContext);
            ctx.HandleResponse();
            return;
        }

        ctx.HttpContext.Response.Redirect(ctx.ReturnUri ?? "/");
        ctx.HandleResponse();
    };

    private static Task<bool> CanUserSignUpAsync(TicketReceivedContext ctx, AppDbContext db, ClaimsPrincipal remoteUser)
    {
        // OPTIONAL: Examine the properties of `remoteUser` and determine if they're permitted to sign up.
        return Task.FromResult(true);
    }
#endif
}
