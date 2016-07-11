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
                if (cookie.Value != "None")
                {
                    var claims = new[] {
                        new Claim(ClaimTypes.Name, "SecurityTestUser"),
                        new Claim(ClaimTypes.Role, cookie.Value)
                    };
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    await context.Authentication.SignInAsync(AuthenticationScheme, new ClaimsPrincipal(identity));
                }
            }
            else
            {
                var claims = new[] {
                    new Claim(ClaimTypes.Name, "DemoUser"),
                    new Claim(ClaimTypes.Role, "User")
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await context.Authentication.SignInAsync(AuthenticationScheme, new ClaimsPrincipal(identity));
            }
            await _next(context);
        }
    }
}
