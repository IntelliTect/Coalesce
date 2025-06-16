using Coalesce.Starter.Vue.Data;
using Coalesce.Starter.Vue.Data.Auth;
using Coalesce.Starter.Vue.Web;
using IntelliTect.Coalesce;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
#if OpenAPI
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
#endif
using System.Text.RegularExpressions;
#if Identity
#endif
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc;
#if EmailAzure
using Azure.Core;
using Azure.Identity;
#endif
#if (LocalAuth || TenantMemberInvites || TenantCreateAdmin || EmailSendGrid || EmailAzure)
using Coalesce.Starter.Vue.Data.Communication;
#endif

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    // Explicit declaration prevents ASP.NET Core from erroring if wwwroot doesn't exist at startup:
    WebRootPath = "wwwroot"
});

builder.AddServiceDefaults();

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.localhost.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

#region Configure Services

var services = builder.Services;

services.AddDbContext<AppDbContext>(options => options
    .UseSqlServer(builder.Configuration.GetConnectionString("sqldb"), opt => opt
        .EnableRetryOnFailure()
        .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
    )
    // Ignored because it interferes with the construction of Coalesce IncludeTrees via .Include()
    .ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.NavigationBaseIncludeIgnored))
);

services.AddCoalesce<AppDbContext>();
services.AddDataProtection().PersistKeysToDbContext<AppDbContext>();
services.AddMvc();

#if Identity
builder.ConfigureAuthentication();

#endif
#if EmailSendGrid
services.Configure<SendGridEmailOptions>(builder.Configuration.GetSection("Communication:SendGrid"));
services.AddTransient<IEmailService, SendGridEmailService>();

#elif EmailAzure
services.AddSingleton<TokenCredential, DefaultAzureCredential>();
services.Configure<AzureEmailOptions>(builder.Configuration.GetSection("Communication:Azure"));
services.AddTransient<IEmailService, AzureEmailService>();

#elif (LocalAuth || TenantMemberInvites || TenantCreateAdmin)
services.AddTransient<IEmailService, NoOpEmailService>();

#endif
#if OpenAPI
services.AddSwaggerGen(c =>
{
    c.AddCoalesce();
    c.SwaggerDoc("current", new OpenApiInfo { Title = "Current API", Version = "current" });
});

#endif

services.AddScoped<SecurityService>();

#if (LocalAuth || TenantMemberInvites || TenantCreateAdmin)
// Register IUrlHelper to allow for invite link generation.
services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
services.AddScoped<IUrlHelper>(x =>
{
    var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
    var factory = x.GetRequiredService<IUrlHelperFactory>();
    return factory.GetUrlHelper(actionContext!);
});

services.AddScoped<InvitationService>();
#endif

#endregion

#region Configure HTTP Pipeline

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseViteDevelopmentServer(c =>
    {
        c.DevServerPort = 5002;
    });

    app.MapCoalesceSecurityOverview("coalesce-security");

#if (!Identity)
    // TODO: Dummy authentication for initial development.
    // Replace this with a proper authentication scheme like
    // Windows Authentication, or an OIDC provider, or something else.
    // If you wanted to use ASP.NET Core Identity, you're recommended
    // to keep the "--Identity" parameter to the Coalesce template enabled.
    app.Use(async (context, next) =>
    {
        Claim[] claims = [new Claim(ClaimTypes.Name, "developmentuser")];

        var identity = new ClaimsIdentity(claims, "dummy-auth");
        context.User = new ClaimsPrincipal(identity);

        await next.Invoke();
    });
    // End Dummy Authentication.
#endif
}

app.UseAuthentication();
app.UseAuthorization();

// TODO: Use new extensions

var containsFileHashRegex = new Regex(@"[.-][0-9a-zA-Z-_]{8}\.[^\.]*$", RegexOptions.Compiled);
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        // vite puts 8-char hashes before the file extension.
        // Use this to determine if we can send a long-term cache duration.
        if (containsFileHashRegex.IsMatch(ctx.File.Name))
        {
            ctx.Context.Response.Headers.CacheControl = "max-age=2592000, public";
        }
    }
});

// For all requests that aren't to static files, disallow caching by default.
// Individual endpoints may override this.
app.Use(async (context, next) =>
{
    context.Response.Headers.CacheControl = "no-cache, no-store";
    await next();
});

#if OpenAPI
app.MapSwagger();
app.MapScalarApiReference(c => c.OpenApiRoutePattern = "/swagger/{documentName}/swagger.json");
#endif

app.MapRazorPages();
app.MapDefaultControllerRoute();

// API fallback to prevent serving SPA fallback to 404 hits on API endpoints.
app.Map("/api/{**any}", () => Results.NotFound());

app.MapFallbackToController("Index", "Home");

#endregion

#region Launch

// Initialize/migrate database.
using (var scope = app.Services.CreateScope())
{
    var serviceScope = scope.ServiceProvider;

    // Run database migrations.
    using var db = serviceScope.GetRequiredService<AppDbContext>();
    db.Database.SetCommandTimeout(TimeSpan.FromMinutes(10));
#if KeepTemplateOnly
    //db.Database.EnsureDeleted();
    db.Database.EnsureCreated();
#else
    db.Database.Migrate();
#endif
    ActivatorUtilities.GetServiceOrCreateInstance<DatabaseSeeder>(serviceScope).Seed();
}

app.Run();
#endregion
