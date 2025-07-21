using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace Coalesce.Domain.WebShared;

public class DemoMiddleware
{
    public const string AuthenticationScheme = "DemoMiddleware";

    private readonly RequestDelegate _next;

    public DemoMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var validRoles = new List<string> { "Admin", "User", "None" };
        var wasLoggedIn = context.User.Identity?.IsAuthenticated == true;

        var cookie = context.Request.Cookies.FirstOrDefault(c => c.Key == "SecurityTestRole");
        if (!cookie.Equals(default(KeyValuePair<string, string>))
            && validRoles.Contains(cookie.Value)
            && context.Request.Host.Value?.Contains("localhost", System.StringComparison.OrdinalIgnoreCase) == true)
        {
            if (cookie.Value != "None")
            {
                await SignInUser(context, "SecurityTestUser", cookie.Value);
                if (!wasLoggedIn) context.Response.Redirect(context.Request.Path);
            }
        }
        else
        {
            cookie = context.Request.Cookies.FirstOrDefault(c => c.Key == "DemoUserRole");
            if (!cookie.Equals(default(KeyValuePair<string, string>))
                && validRoles.Contains(cookie.Value)
                && context.Request.Host.Value?.Contains("localhost", System.StringComparison.OrdinalIgnoreCase) == true)
            {
                await SignInUser(context, "DemoUser", cookie.Value);
            }
            else
            {
                await SignInUser(context, "DemoUser", "Admin");
            }
        }

        var isLoggedIn = context.User.Identity?.IsAuthenticated == true;
        await _next(context);
    }

    private async Task SignInUser(HttpContext context, string name, string role)
    {
        Claim[] claims;
        if (string.IsNullOrEmpty(role))
        {
            claims = new[] { new Claim(ClaimTypes.Name, name) };
        }
        else
        {
            claims = new[] {
                new Claim(ClaimTypes.Name, name),
                new Claim(ClaimTypes.Role, role)
            };
        }

        var identity = new ClaimsIdentity(claims, "AutoSignIn");
        if (
            context.User.Claims.All(c1 => claims.Any(c2 => c1.Type == c2.Type && c1.Value == c2.Value)) &&
            claims.All(c1 => context.User.Claims.Any(c2 => c1.Type == c2.Type && c1.Value == c2.Value))
        )
        {
            // Already signed in with the same claims. Do nothing. 
            // Signing in with cookies force sets all caching headers to no-cache, 
            // which we don't want to do unless we really need to change user.
            return;
        }
        var user = new ClaimsPrincipal(identity);
        await context.SignInAsync(AuthenticationScheme, user);
        context.User = user;
    }
}
