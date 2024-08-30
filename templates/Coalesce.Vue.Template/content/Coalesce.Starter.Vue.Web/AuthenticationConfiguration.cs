using Coalesce.Starter.Vue.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Coalesce.Starter.Vue.Data.Auth;

public static class AuthenticationConfiguration
{
    public static void ConfigureAuthentication(this WebApplicationBuilder builder)
    {
        var config = builder.Configuration;

        builder.Services
            .AddIdentity<AppUser, AppRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        //.AddClaimsPrincipalFactory<ClaimsPrincipalFactory>(); // TODO

        builder.Services
            .AddAuthentication()
            .AddGoogle(options =>
             {
                 options.ClientId = config["Authentication:Google:ClientId"];
                 options.ClientSecret = config["Authentication:Google:ClientSecret"];
                 options.Events.OnTicketReceived = OnTicketReceived();
             })
            .AddMicrosoftAccount(options =>
            {
                options.ClientId = config["Authentication:Microsoft:ClientId"];
                options.ClientSecret = config["Authentication:Microsoft:ClientSecret"];
                options.Events.OnTicketReceived = OnTicketReceived();
            });

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

    private static Func<OAuthCreatingTicketContext, Task> OnCreatingTicket()
        => async (OAuthCreatingTicketContext ctx) =>
        {
            //var db = ctx.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            //var user = await GetOrCreateUserAsync(db, ctx.Principal!);

            //if (user is null)
            //{
            //    ctx.Fail("Forbidden.");
            //}
            //else
            //{
            //    //var signInManager = ctx.HttpContext.RequestServices.GetRequiredService<SignInManager<AppUser>>();

            //    //var info = await signInManager.GetExternalLoginInfoAsync();
            //    //if (info == null)
            //    //{
            //    //    ctx.Fail("External login info unavailable.");
            //    //}

            //    //var user2 = await signInManager.UserManager.FindByLoginAsync(info.LoginProvider, info.ProviderKey);

            //}
            ctx.Success();
        };

    private static Func<TicketReceivedContext, Task> OnTicketReceived() => async (TicketReceivedContext ctx) =>
    {
        var db = ctx.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
        var signInManager = ctx.HttpContext.RequestServices.GetRequiredService<SignInManager<AppUser>>();

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
            // OPTIONAL: Examine the properties of `remoteUser` and determine if they're permitted to sign up.
            bool userCanSignUp = true; 
            if (!userCanSignUp)
            {
                await Results.Text("Forbidden", statusCode: StatusCodes.Status403Forbidden).ExecuteAsync(ctx.HttpContext);
                ctx.HandleResponse();
                return;
            }

            user = new AppUser { UserName = remoteUserEmail };
            await signInManager.UserManager.CreateAsync(user);
        }

        user.FullName = remoteUser.FindFirstValue(ClaimTypes.Name) ?? user.FullName;
        user.Email = remoteUserEmail ?? user.Email;
        // OPTIONAL: Update any other properties on the user as desired.

        await signInManager.UserManager.UpdateAsync(user);

        if (!foundByLogin)
        {
            await signInManager.UserManager.AddLoginAsync(user, 
                new UserLoginInfo(remoteProvider, remoteUserId, ctx.Scheme.DisplayName));
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
}
