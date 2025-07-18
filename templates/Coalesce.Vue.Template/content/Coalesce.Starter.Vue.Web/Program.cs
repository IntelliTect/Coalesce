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
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
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
#if Hangfire
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authorization;
#if AIChat
using System.Reflection;
using Microsoft.SemanticKernel;
using Coalesce.Starter.Vue.Data.Services;
#endif
#endif

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    // Explicit declaration prevents ASP.NET Core from erroring if wwwroot doesn't exist at startup:
    WebRootPath = "wwwroot"
});

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.localhost.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.AddServiceDefaults();

#region Configure Services

var services = builder.Services;

services.AddDbContext<AppDbContext>(options => options
    .UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), opt => opt
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
    c.SwaggerDoc("current", new OpenApiInfo
    {
        Title = "Current API",
        Version = "current",
        Description = "This API surface is auto-generated and is subject to change at any time."
    });
});

#endif

services.AddScoped<SecurityService>();

#if (LocalAuth || TenantMemberInvites || TenantCreateAdmin)
// Register IUrlHelper to allow for invite link generation.
services.AddUrlHelper();
services.AddScoped<InvitationService>();
#endif

#if Hangfire
services.AddHangfire((config) =>
{
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new()
    {
        // The Hangfire schema is installed manually below after DB migrations are ran
        // so that the database has a chance to be created before Hangfire starts connecting to it.
        PrepareSchemaIfNecessary = false,
        TryAutoDetectSchemaDependentOptions = false,
        DisableGlobalLocks = true,
        UseIgnoreDupKeyOption = true,
    });
});
services.AddHangfireServer(c =>
{
    // Hangfire default worker count is 20, which is usually excessive for most needs.
    c.WorkerCount = 5;
});
#endif

#if AIChat
// Dynamically register all Coalesce-generated kernel plugins
foreach (var pluginType in Assembly.GetExecutingAssembly().GetTypes().Where(t => t.BaseType?.Name == "KernelPluginBase`1"))
    services.AddScoped(sp => KernelPluginFactory.CreateFromType(pluginType, pluginType.Name, sp));

services.AddScoped<AIAgentService>();
builder.AddAzureOpenAIClient(connectionName: "OpenAI").AddChatClient(deploymentName: "chat");
services.AddAzureOpenAIChatCompletion(deploymentName: "chat");
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

app.UseViteStaticFiles();
app.UseNoCacheResponseHeader();

#if OpenAPI
app.MapSwagger();
app.MapScalarApiReference(c => c.OpenApiRoutePattern = "/swagger/{documentName}/swagger.json");
#endif
#if Hangfire
app.MapHangfireDashboard("/hangfire", new() { Authorization = [] }).RequireAuthorization(
#if Tenancy
    new AuthorizeAttribute { Roles = builder.Environment.IsDevelopment() ? null : AppClaimValues.GlobalAdminRole }
#else
    new AuthorizeAttribute { Roles = builder.Environment.IsDevelopment() ? null : nameof(Permission.Admin) }
#endif
);
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
#if Hangfire
    // Install Hangfire storage only after the database has definitely been created.
    // https://github.com/HangfireIO/Hangfire/issues/2139
    SqlServerObjectsInstaller.Install(db.Database.GetDbConnection(), null, true);

#endif
    ActivatorUtilities.GetServiceOrCreateInstance<DatabaseSeeder>(serviceScope).Seed();
}

app.Run();
#endregion