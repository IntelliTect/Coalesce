using Coalesce.Starter.Vue.Data;
using Coalesce.Starter.Vue.Data.Auth;
using Coalesce.Starter.Vue.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Security.Claims;

namespace Coalesce.Starter.Vue.Web.Auth;

public static class AuthenticationConfiguration
{
    public static void ConfigureAuthentication(this WebApplicationBuilder builder)
    {
        var config = builder.Configuration;

        builder.Services
            .AddIdentity<User, Role>(c =>
            {
                c.ClaimsIdentity.RoleClaimType = AppClaimTypes.Role;
                c.ClaimsIdentity.EmailClaimType = AppClaimTypes.Email;
                c.ClaimsIdentity.UserIdClaimType = AppClaimTypes.UserId;
                c.ClaimsIdentity.UserNameClaimType = AppClaimTypes.UserName;

                c.User.RequireUniqueEmail = true;
#if LocalAuth
                // https://pages.nist.gov/800-63-4/sp800-63b.html#passwordver
                c.Password.RequireNonAlphanumeric = false;
                c.Password.RequireDigit = false;
                c.Password.RequireUppercase = false;
                c.Password.RequireLowercase = false;
                c.Password.RequiredLength = 15;
#endif
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders()
            .AddClaimsPrincipalFactory<ClaimsPrincipalFactory>();

#if LocalAuth
        // todo: different implementation that throws?
        builder.Services.TryAddTransient<IEmailSender, NoOpEmailSender>();
        builder.Services.AddScoped<UserManagementService>();
#endif

#if (GoogleAuth || MicrosoftAuth)
        builder.Services.AddScoped<SignInService>();
#endif

        builder.Services
            .AddAuthentication()
#if GoogleAuth
            .AddGoogle(options =>
            {
                options.ClientId = config["Authentication:Google:ClientId"]!;
                options.ClientSecret = config["Authentication:Google:ClientSecret"]!;
#if TenantCreateExternal
                options.ClaimActions.MapJsonKey("hd", "hd"); // Hosted domain (i.e. GSuite domain).
#endif
#if UserPictures
                options.ClaimActions.MapJsonKey("pictureUrl", "picture");
#endif
                options.Events.OnTicketReceived = async ctx =>
                {
                    await ctx.HttpContext.RequestServices
                        .GetRequiredService<SignInService>()
                        .OnGoogleTicketReceived(ctx);
                };
            })
#endif
#if MicrosoftAuth
            .AddMicrosoftAccount(options =>
            {
                options.ClientId = config["Authentication:Microsoft:ClientId"]!;
                options.ClientSecret = config["Authentication:Microsoft:ClientSecret"]!;
#if (UserPictures || TenantCreateExternal)
                options.SaveTokens = true;
#endif

                options.Events.OnTicketReceived = async ctx =>
                {
                    await ctx.HttpContext.RequestServices
                        .GetRequiredService<SignInService>()
                        .OnMicrosoftTicketReceived(ctx);
                };
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

#if Tenancy
            var oldOnValidate = c.Events.OnValidatePrincipal;
            c.Events.OnValidatePrincipal = async ctx =>
            {
                // Make the current tenantID of the user available to the rest of the request.
                // This is the earliest possible point to do so after the auth cookie has been read.
                ctx.HttpContext.RequestServices
                    .GetRequiredService<AppDbContext>()
                    .TenantId = ctx.Principal?.GetTenantId();

                await oldOnValidate(ctx);
            };
#endif
        });
    }

}
