#if AppInsights
using Azure.Monitor.OpenTelemetry.AspNetCore;
#endif
using Coalesce.Starter.Vue.Data.Auth;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Coalesce.Starter.Vue.Web;

// Adds common .NET Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// To learn more about using this class, see https://aka.ms/dotnet/aspire/service-defaults
// Note that since Coalesce projects usually only have one service, this file is not maintained in a separate project.
// You are encouraged to move it into a dedicated class library project if your solution grows.

public static class ProgramServiceDefaults
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation(tracing =>
                    {
                        // Exclude health check requests from tracing
                        tracing.Filter = context =>
                            !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath);

                        tracing.EnrichWithHttpResponse = (activity, response) =>
                        {
                            activity.SetTag("enduser.id", response.HttpContext.User.GetUserId());
                        };
                    })
                    .AddHttpClientInstrumentation()
                    .AddSqlClientInstrumentation(tracing =>
                    {
                        tracing.SetDbStatementForText = true;
                    });
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        if (builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"] is string { Length: > 0 })
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

#if AppInsights
        if (builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"] is string { Length: > 0 } aiConnStr)
        {
            builder.Services.AddOpenTelemetry().UseAzureMonitor(opt => opt.ConnectionString = aiConnStr );
        }
#endif

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        // Adding health checks endpoints to applications in non-development environments has security implications.
        // See https://aka.ms/dotnet/aspire/healthchecks for details before enabling these endpoints in non-development environments.
        if (app.Environment.IsDevelopment())
        {
            // All health checks must pass for app to be considered ready to accept traffic after starting
            app.MapHealthChecks(HealthEndpointPath).AllowAnonymous();

            // Only health checks tagged with the "live" tag must pass for app to be considered alive
            app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            }).AllowAnonymous();
        }

        return app;
    }
}
