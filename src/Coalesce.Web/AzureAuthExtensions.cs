using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Coalesce.Web
{
    public static class AzureAuthExtensions
    {
        public static IApplicationBuilder UseAzureAuthMiddleware(this IApplicationBuilder app)
        {
            return app.UseMiddleware<AzureAuthMiddleware>();
        }
    }
}
