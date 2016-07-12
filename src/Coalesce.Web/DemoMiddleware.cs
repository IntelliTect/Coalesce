using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System;
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

            var cookie = context.Request.Cookies.FirstOrDefault(c => c.Key == "SecurityTestRole");
            if (!cookie.Equals(default(KeyValuePair<string, string>))
                && validRoles.Contains(cookie.Value)
                && context.Request.Host.Value.ToLower().Contains("localhost"))
            {
                if (cookie.Value != "None") await SignInUser(context, "SecurityTestUser", cookie.Value);
            }
            else
            {
                cookie = context.Request.Cookies.FirstOrDefault(c => c.Key == "DemoUserRole");
                if (!cookie.Equals(default(KeyValuePair<string, string>))
                    && validRoles.Contains(cookie.Value)
                    && context.Request.Host.Value.ToLower().Contains("localhost"))
                {
                    if (cookie.Value != "None") await SignInUser(context, "DemoUser", cookie.Value);
                }
                else
                {
                    await SignInUser(context, "DemoUser", "User");
                }
            }
            await _next(context);
        }

        private async Task SignInUser(HttpContext context, string name, string role)
        {
            var claims = new[] {
                    new Claim(ClaimTypes.Name, name),
                    new Claim(ClaimTypes.Role, role)
                };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await context.Authentication.SignInAsync(AuthenticationScheme, new ClaimsPrincipal(identity));
        }
    }
}
