using IntelliTect.Coalesce.Vue.DevMiddleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SpaServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce
{
    public static class CoalesceVueApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseViteDevelopmentServer(this IApplicationBuilder app)
            => app.UseViteDevelopmentServer(new ViteServerOptions());

        public static IApplicationBuilder UseViteDevelopmentServer(this IApplicationBuilder app, ViteServerOptions options)
        {
            return app.UseWhen(c => c.Request.Path.StartsWithSegments(options.PathBase), app =>
            {
                app.UseSpa(spa =>
                {
                    spa.Options.SourcePath = ".";
                    spa.UseProxyToViteDevelopmentServer(options);
                });
            });
        }

        public static void UseProxyToViteDevelopmentServer(this ISpaBuilder spaBuilder, ViteServerOptions options)
        {
            ViteDevelopmentServerMiddleware.Attach(spaBuilder, options);
        }
    }
}
