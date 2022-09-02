using Coalesce.Domain;
using Coalesce.Domain.Services;
using Coalesce.Domain.WebShared;
using IntelliTect.Coalesce;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace Coalesce.Web
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; set; }

#if NETCOREAPP
        public Startup(IWebHostEnvironment env)
#else
        public Startup(IHostingEnvironment env)
#endif
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Add Entity Framework services to the services container.
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddCoalesce(builder =>
            {
                builder
                    .AddContext<AppDbContext>()
                    .UseDefaultDataSource(typeof(MyDataSource<,>))
                    .UseDefaultBehaviors(typeof(MyBehaviors<,>))
                    .Configure(o =>
                    {
                        o.DetailedExceptionMessages = true;
                        o.ExceptionResponseFactory = ctx =>
                        {
                            if (ctx.Exception is FileNotFoundException)
                            {
                                ctx.HttpContext.Response.StatusCode = 404;
                                return new IntelliTect.Coalesce.Models.ApiResult(false, "File not found");
                            }
                            return null;
                        };
                    });

                // This breaks on non-windows platforms, see https://github.com/dotnet/corefx/issues/11897
                builder.UseTimeZone(RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                    ? TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time")
                    : TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles"));
            });

            services.AddCors();

            services.AddSwaggerGen(c =>
            {
                c.AddCoalesce();
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
            });

            services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = int.MaxValue; // testing big file uploads/downloads
            });

#if NETCOREAPP3_1
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddNewtonsoftJson(options =>
                 {
                     options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                     options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                     options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                 });
#else
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
#endif

            services.AddScoped<IWeatherService, WeatherService>();

            services.AddAuthentication(DemoMiddleware.AuthenticationScheme)
                .AddCookie(DemoMiddleware.AuthenticationScheme, options => {
                    options.AccessDeniedPath = "/Account/AccessDenied/";
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/LogOff";
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles();

#if NETCOREAPP
            app.UseRouting();

            // *** DEMO ONLY ***
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<DemoMiddleware>();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapDefaultControllerRoute();
            });
#else

            // *** DEMO ONLY ***
            app.UseAuthentication();
            app.UseMiddleware<DemoMiddleware>();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "Area Route",
                    template: "{area:exists}/{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });
                routes.MapRoute(
                    name: "Default Route",
                    template: "{controller}/{action}/{id?}",
                    defaults: new { controller = "Home", action = "Index" });
            });
#endif
        }

    }
}
