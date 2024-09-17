using Coalesce.Starter.Vue.Data;
using Coalesce.Starter.Vue.Data.Auth;
using Coalesce.Starter.Vue.Web;
using IntelliTect.Coalesce;
#if AppInsights
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging.ApplicationInsights;
#endif
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Console;
#if OpenAPI
using Microsoft.OpenApi.Models;
#endif
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
	Args = args,
	// Explicit declaration prevents ASP.NET Core from erroring if wwwroot doesn't exist at startup:
	WebRootPath = "wwwroot"
});

builder.Logging
	.AddConsole()
	// Filter out Request Starting/Request Finished noise:
	.AddFilter<ConsoleLoggerProvider>("Microsoft.AspNetCore.Hosting.Diagnostics", LogLevel.Warning);

builder.Configuration
	.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
	.AddJsonFile("appsettings.localhost.json", optional: true, reloadOnChange: true)
	.AddEnvironmentVariables();

#region Configure Services

var services = builder.Services;

#if AppInsights
services.AddApplicationInsightsTelemetry(b =>
{
    b.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});
services.AddSingleton<ITelemetryInitializer, AppInsightsTelemetryEnricher>();
services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) => {
	module.EnableSqlCommandTextInstrumentation = true;
});
// App insights filters all logs to Warning by default.
builder.Logging.AddFilter<ApplicationInsightsLoggerProvider>("Coalesce.Starter.Vue", LogLevel.Information);
#endif


services.AddDbContext<AppDbContext>(options => options
	.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), opt => opt
		.EnableRetryOnFailure()
		.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
	)
	// Ignored because it interferes with the construction of Coalesce IncludeTrees via .Include()
	.ConfigureWarnings(warnings => warnings.Ignore(CoreEventId.NavigationBaseIncludeIgnored))
);

services.AddCoalesce<AppDbContext>();

services.AddDataProtection()
	.PersistKeysToDbContext<AppDbContext>();

services
	.AddMvc()
	.AddJsonOptions(options =>
	{
		options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
		options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
		options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
	});

#if Identity
builder.ConfigureAuthentication();
#endif

#if OpenAPI
services.AddSwaggerGen(c =>
{
	c.AddCoalesce();
	c.SwaggerDoc("current", new OpenApiInfo { Title = "Current API", Version = "current" });
});
#endif


services.AddScoped<SecurityService>();


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

var containsFileHashRegex = new Regex(@"[.-][0-9a-zA-Z-_]{8}\.[^\.]*$", RegexOptions.Compiled);
app.UseStaticFiles(new StaticFileOptions
{
	OnPrepareResponse = ctx =>
	{
		// vite puts 8-char hashes before the file extension.
		// Use this to determine if we can send a long-term cache duration.
		if (containsFileHashRegex.IsMatch(ctx.File.Name))
		{
			ctx.Context.Response.GetTypedHeaders().CacheControl = new() { Public = true, MaxAge = TimeSpan.FromDays(30) };
		}
	}
});

// For all requests that aren't to static files, disallow caching by default.
// Individual endpoints may override this.
app.Use(async (context, next) =>
{
	context.Response.GetTypedHeaders().CacheControl = new() { NoCache = true, NoStore = true };
	await next();
});

#if OpenAPI
app.MapSwagger();
app.UseSwaggerUI(c =>
{
	c.SwaggerEndpoint("/swagger/current/swagger.json", "Current API");
});
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
	db.Database.EnsureDeleted();
	db.Database.EnsureCreated();
#else
	db.Database.Migrate();
#endif
    new DatabaseSeeder(db).Seed();
}

app.Run();
#endregion
