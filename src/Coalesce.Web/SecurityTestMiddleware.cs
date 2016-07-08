using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Coalesce.Web
{
    public class SecurityTestMiddleware
    {
        RequestDelegate _next;

        public SecurityTestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var validRoles = new List<string> { "Admin", "User" };
            var cookie = context.Request.Cookies.FirstOrDefault(c => c.Key == "SecurityTestRole");

            if (!cookie.Equals(default(KeyValuePair<string, string>))
                && validRoles.Contains(cookie.Value)
                && context.Request.Host.Value.ToLower().Contains("localhost"))
            {
                var claims = new[] {
                    new Claim(ClaimTypes.Name, "SecurityTestUser"),
                    new Claim(ClaimTypes.Role, cookie.Value)
                };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                try
                {
                    await context.Authentication.SignInAsync("SecurityTestMiddleware", new ClaimsPrincipal(identity));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            await _next(context);
        }
    }
}
