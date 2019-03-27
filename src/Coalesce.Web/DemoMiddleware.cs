using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Coalesce.Web
{
    public class DemoMiddleware
    {
        public const string AuthenticationScheme = "DemoMiddleware";

        RequestDelegate _next;

        public DemoMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var validRoles = new List<string> { "Admin", "User", "None" };
            var wasLoggedIn = context.User.Identity.IsAuthenticated;

            var cookie = context.Request.Cookies.FirstOrDefault(c => c.Key == "SecurityTestRole");
            if (!cookie.Equals(default(KeyValuePair<string, string>))
                && validRoles.Contains(cookie.Value)
                && context.Request.Host.Value.ToLower().Contains("localhost"))
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
                    && context.Request.Host.Value.ToLower().Contains("localhost"))
                {
                    await SignInUser(context, "DemoUser", cookie.Value);
                }
                else
                {
                    await SignInUser(context, "DemoUser", "Admin");
                }
            }

            var isLoggedIn = context.User.Identity.IsAuthenticated;
            if (!wasLoggedIn && isLoggedIn) context.Response.Redirect("/");
            else await _next(context);
        }

        private async Task SignInUser(HttpContext context, string name, string role)
        {
            Claim[] claims;
            if (string.IsNullOrEmpty(role)) claims = new[] { new Claim(ClaimTypes.Name, name) };
            else claims = new[] {
                    new Claim(ClaimTypes.Name, name),
                    new Claim(ClaimTypes.Role, role)
                };

            var identity = new ClaimsIdentity(claims, "AutoSignIn");
            await context.SignInAsync(AuthenticationScheme, new ClaimsPrincipal(identity));
        }
    }
}
