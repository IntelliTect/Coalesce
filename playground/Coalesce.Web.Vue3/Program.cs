using Coalesce.Domain;
using Coalesce.Domain.WebShared;
using IntelliTect.Coalesce;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Console;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using Scalar.AspNetCore;
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
    .AddEnvironmentVariables();

#region Configure Services

var services = builder.Services;

services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), opt => opt
        .EnableRetryOnFailure()
        .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
    ));

services.AddCoalesce<AppDbContext>(c => c.Configure(o =>
{
    o.ValidateAttributesForMethods = true;
    o.ValidateAttributesForSaves = true;
}));
services.AddScoped<Person.WithoutCases>();

services.AddMvc();

services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = int.MaxValue; // testing big file uploads/downloads
});

services.AddUrlHelper();
services.AddOpenApi();

services.AddSwaggerGen(c =>
{
    c.AddCoalesce();
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});


services.AddAuthentication(DemoMiddleware.AuthenticationScheme).AddCookie(DemoMiddleware.AuthenticationScheme);

#endregion



#region Configure HTTP Pipeline

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseViteDevelopmentServer();

    app.MapCoalesceSecurityOverview("coalesce-security");
}

// *** DEMO ONLY ***
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<DemoMiddleware>();

app.UseViteStaticFiles();
app.UseNoCacheResponseHeader();

app.UseCors(c => c.AllowAnyOrigin().AllowAnyHeader());
app.MapControllers();

app.MapOpenApi();
app.MapScalarApiReference();

app.MapSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});

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
    SampleData.Initialize(serviceScope.GetRequiredService<AppDbContext>());
}

app.Run();

#endregion
