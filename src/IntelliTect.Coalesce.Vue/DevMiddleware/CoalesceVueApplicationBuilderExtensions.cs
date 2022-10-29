using IntelliTect.Coalesce.Vue.DevMiddleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            => app.UseViteDevelopmentServer(_ => { });

        public static IApplicationBuilder UseViteDevelopmentServer(this IApplicationBuilder app, Action<ViteServerOptions> configure)
        {
            var options = app.ApplicationServices.GetService<IOptions<ViteServerOptions>>()?.Value ?? new();
            configure(options);
            return app.UseViteDevelopmentServer(options);
        }

        public static IApplicationBuilder UseViteDevelopmentServer(this IApplicationBuilder app, ViteServerOptions options) 
        {
            return app
                .UseWhen(c => 
                    c.Request.Path.StartsWithSegments(options.PathBase) || 
                    // Vite does not prefix the import of '@vite/env' that is emitted into web worker scripts...
                    c.Request.Path.StartsWithSegments("/@vite"), 
                filteredApp =>
                {
                    filteredApp.UseSpa(spa =>
                    {
                        spa.Options.SourcePath = ".";
                        var getPortTask = ViteDevelopmentServerMiddleware.Attach(spa, options);

                        if (options.WaitForReady)
                        {
                            // Add middleware to the main app pipeline that will wait for the vite server
                            // to start before serving requests for HTML files (e.g. our SPA fallback route).
                            app.Use(async (context, next) =>
                            {
                                if (getPortTask().IsCompleted)
                                {
                                    await next();
                                    return;
                                }

                                // Browser document request always include text/html as the very first Accept header value.
                                if (context.Request.Headers.Accept.Any(s => s.StartsWith("text/html")))
                                {
                                    // Don't try and serve the SPA fallback route if the server hasn't started,
                                    // since it won't have written index.html to disk yet.
                                    ViteDevelopmentServerMiddleware
                                        .GetOrCreateLogger(app)
                                        .LogInformation($"Waiting for vite server to start listening...");
                                    await getPortTask();
                                }

                                await next();
                            });
                        }
                    });
                });
        }

        public static void UseProxyToViteDevelopmentServer(this ISpaBuilder spaBuilder, ViteServerOptions options)
        {
            ViteDevelopmentServerMiddleware.Attach(spaBuilder, options);
        }
    }
}
