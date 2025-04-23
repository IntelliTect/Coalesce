using Coalesce.Starter.Vue.Data;
using Coalesce.Starter.Vue.Data.Auth;
using Coalesce.Starter.Vue.Data.Communication;
using Coalesce.Starter.Vue.Data.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Coalesce.Starter.Vue.Web;

public static class ProgramAuthConfiguration
{
#if (GoogleAuth || MicrosoftAuth || OtherOAuth)
    private const string SwitchingAccountCookieName = "switching-account";

#endif
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
        builder.Services.AddScoped<UserManagementService>();
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

                var old = options.Events.OnRedirectToAuthorizationEndpoint;
                options.Events.OnRedirectToAuthorizationEndpoint = ctx =>
                {
                    if (ctx.Request.Cookies.ContainsKey(SwitchingAccountCookieName))
                    {
                        ctx.RedirectUri += "&prompt=select_account";
                    }
                    return old(ctx);
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

                var oldOnRedirect = options.Events.OnRedirectToAuthorizationEndpoint;
                options.Events.OnRedirectToAuthorizationEndpoint = ctx =>
                {
                    if (ctx.Request.Cookies.ContainsKey(SwitchingAccountCookieName))
                    {
                        ctx.RedirectUri += "&prompt=select_account";
                    }
                    return oldOnRedirect(ctx);
                };
            })
#endif
#if OtherOAuth
            .AddOAuth("MyOtherOAuth", displayName: "My Unconfigured OAuth Provider", options =>
            {
                // TODO: Update/replace this configuration with the settings needed for your OAuth provider.
                // The scheme name "MyOtherOAuth" is also referenced in ExternalLogin.cshtml.cs.

                // If your provider offers a NuGet package (e.g. Microsoft.AspNetCore.Authentication.MicrosoftAccount)
                // you'll almost certainly be better served by using that instead of this generic .AddOAuth() configuration;
                // it will eliminate the need for most of the configuration besides ClientId/ClientSecret.

                options.ClientId = config["Authentication:MyOtherOAuth:ClientId"]!;
                options.ClientSecret = config["Authentication:MyOtherOAuth:ClientSecret"]!;

                options.CallbackPath = "/signin-my-other-oauth"; // This path can be anything, as long as it doesn't conflict with another route in your app.
                options.AuthorizationEndpoint = "https://example.com/api/oauth2/authorize";
                options.TokenEndpoint = "https://example.com/api/oauth2/token";
                options.UsePkce = true;
                options.Scope.Add("openid");
                options.Scope.Add("profile");
                options.Scope.Add("email");
#if (UserPictures || TenantCreateExternal)
                // Capturing tokens is likely necessary for profile pictures and/or external tenant identification.
                options.SaveTokens = true;
#endif

                // NOTE: Your OAuth provider might not respect the prompt=select_account flag.
                var oldOnRedirect = options.Events.OnRedirectToAuthorizationEndpoint;
                options.Events.OnRedirectToAuthorizationEndpoint = ctx =>
                {
                    if (ctx.Request.Cookies.ContainsKey(SwitchingAccountCookieName))
                    {
                        ctx.RedirectUri += "&prompt=select_account";
                    }
                    return oldOnRedirect(ctx);
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
            c.LoginPath = "/SignIn"; // Razor page "Pages/SignIn.cshtml"
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
#if (GoogleAuth || MicrosoftAuth || OtherOAuth)

            c.Events.OnSigningOut = ctx =>
            {
                // Enable prompting to switch external accounts on re-login.
                ctx.Response.Cookies.Append(SwitchingAccountCookieName, "1", new()
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.UtcNow.AddMinutes(5),
                    IsEssential = true,
                    SameSite = SameSiteMode.Lax
                });
                return Task.CompletedTask;
            };
#endif
        });
    }

}
