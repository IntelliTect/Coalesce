using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Coalesce.Domain;
using Coalesce.Domain.WebShared;
using IntelliTect.Coalesce;
using IntelliTect.Coalesce.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.Webpack;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Coalesce.Web.Vue
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                });

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));


            services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = int.MaxValue; // testing big file uploads/downloads
            });

            services.AddCoalesce(builder =>
            {
                builder
                    .AddContext<AppDbContext>()
                   // .UseDefaultDataSource(typeof(MyDataSource<,>))
                   // .UseDefaultBehaviors(typeof(MyBehaviors<,>))
                    ;

                // This breaks on non-windows platforms, see https://github.com/dotnet/corefx/issues/11897
                builder.UseTimeZone(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time")
                    : TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"));
            });

            services.AddAuthentication(DemoMiddleware.AuthenticationScheme).AddCookie(DemoMiddleware.AuthenticationScheme);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseViteDevelopmentServer();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();

            // *** DEMO ONLY ***
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<DemoMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();

                endpoints.MapCoalesceSecurityOverview("coalesce-security").RequireAuthorization(
                    new AuthorizeAttribute { Roles = env.IsDevelopment() ? null : "Admin" }
                );

                // API fallback to prevent serving SPA fallback to 404 hits on API endpoints.
                endpoints.Map("api/{**any}", ctx => { ctx.Response.StatusCode = StatusCodes.Status404NotFound; return Task.CompletedTask; });

                endpoints.MapFallbackToController("Index", "Home");
            });
        }
    }
}