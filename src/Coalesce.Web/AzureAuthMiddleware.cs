using Microsoft.AspNet.Authentication.Cookies;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Coalesce.Web
{
    public class AzureAuthMiddleware
    {
        RequestDelegate _next;

        public AzureAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Host.Value.ToLower().Contains("azurewebsites.net"))
            {
                var claims = new[] { new Claim(ClaimTypes.Name, "AzureUser") };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                try
                {
                    await context.Authentication.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
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
