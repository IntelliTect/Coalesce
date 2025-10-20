---
title: "Config: ASP.NET Core"
---

# Application Configuration

::: tip
The easiest way to get started with Coalesce is to use the [Coalesce project template](/stacks/vue/getting-started.md), which includes all the necessary configuration out of the box.
:::

## Basic Setup

For Coalesce to work in your application, you must register the needed services in your `Program.cs`. Doing so is simple:

``` c#
// Program.cs
builder.Services.AddCoalesce<AppDbContext>();
```

## Advanced Configuration

This registers all the basic services that Coalesce needs to work with your EF DbContext. However, many more options are available. Here's a more complete invocation of `AddCoalesce` that takes advantage of many of the options available:

``` c#
// Program.cs
builder.Services.AddCoalesce(b => b
    .AddContext<AppDbContext>()
    .UseDefaultDataSource(typeof(MyDataSource<,>))
    .UseDefaultBehaviors(typeof(MyBehaviors<,>))
    .UseTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"))
    .Configure(o =>
    {
        o.ValidateAttributesForMethods = true; // note: true is the default
        o.ValidateAttributesForSaves = true; // note: true is the default
        o.DetailedExceptionMessages = true;
        o.ExceptionResponseFactory = ctx =>
        {
            if (ctx.Exception is FileNotFoundException)
            {
                ctx.HttpContext.Response.StatusCode = 404; // Optional - set a specific response code.
                return new IntelliTect.Coalesce.Models.ApiResult(false, "File not found");
            }
            return null;
        };
    })
);
```

## Builder Methods

Available builder methods include:

<Prop def="public Builder AddContext<TDbContext>()" />

Register services needed by Coalesce to use the specified context. This is done automatically when calling the `services.AddCoalesce<AppDbContext>();` overload.

<Prop def="public Builder UseDefaultDataSource(Type dataSource)" />

Overrides the default data source used, replacing the [Standard Data Source](/modeling/model-components/data-sources.md#standard-data-source). See [Data Sources](/modeling/model-components/data-sources.md) for more details.

<Prop def="public Builder UseDefaultBehaviors(Type behaviors)" />

Overrides the default behaviors used, replacing the [Standard Behaviors](/modeling/model-components/behaviors.md#standard-behaviors). See [Behaviors](/modeling/model-components/behaviors.md) for more details.

<Prop def="public Builder UseTimeZone(TimeZoneInfo timeZone)" />

Specify a static time zone that should be used when Coalesce is performing operations on dates/times that lack timezone information. For example, when a user inputs a search term that contains only a date, Coalesce needs to know which timezone's midnight to use when performing the search.

<Prop def="public Builder UseTimeZone<ITimeZoneResolver>()" />

Specify a service implementation to use to resolve the current timezone. This should be a scoped service, and will be automatically registered if it is not already. This allows retrieving timezone information on a per-request basis from HTTP headers, Cookies, or any other source.

<Prop def="public Builder Configure(Action<CoalesceOptions> setupAction)" />

Configure additional options for Coalesce runtime behavior. Current options include options for server-side validation, and options for exception handling. See [CoalesceOptions Properties](#coalesceoptions-properties) below for details.

## CoalesceOptions Properties

The following properties are available on `CoalesceOptions` when using the `.Configure()` builder method:

<Prop def="public bool DetailedExceptionMessages { get; set; }" lang="c#" />

Determines whether API controllers will return the `Exception.Message` of unhandled exceptions or not. 

Defaults to `true` if `IHostingEnvironment.EnvironmentName` is "Development"; otherwise `false`.

<Prop def="public Func<ActionExecutedContext, ApiResult?>? ExceptionResponseFactory { get; set; }" lang="c#" />

A function that will transform an unhandled exception in API controllers into a custom `ApiResult` object that will be sent to the client. Return `null` to use the default response handling.

This allows you to customize error responses for specific exception types, such as returning a 404 status code for `FileNotFoundException`.

<Prop def="public bool DetailedEfMigrationExceptionMessages { get; set; }" lang="c#" />

Determines whether detailed error messages about EF model/migration errors are returned in error responses. 

Requires `DetailedExceptionMessages` to be enabled, and defaults to that value.

<Prop def="public bool DetailedEfConstraintExceptionMessages { get; set; }" lang="c#" />

If `true`, Coalesce will transform some database exceptions into user-friendly messages when these exceptions occur in Save and Delete operations through `StandardBehaviors<T>`. For SQL Server, this includes foreign key constraint violations and unique index violations.

These messages respect the security configuration of your models. These messages only serve as a fallback to produce a more acceptable user experience in cases where the developer neglects to add appropriate validation or other handling of related entities.

Defaults to `true`.

<Prop def="public bool ValidateAttributesForSaves { get; set; }" lang="c#" />

If `true`, Coalesce will perform validation of incoming data using `ValidationAttribute`s present on your models during save operations (in `StandardBehaviors<T>.ValidateDto(SaveKind, IParameterDto<T>)`).

This can be overridden on individual Behaviors instances by setting `StandardBehaviors<T>.ValidateAttributesForSaves`.

Defaults to `true`.

<Prop def="public bool ValidateAttributesForMethods { get; set; }" lang="c#" />

If `true`, Coalesce will perform validation of incoming parameters using `ValidationAttribute`s present on your parameters and for custom methods.

This can be overridden on individual custom methods using `ExecuteAttribute.ValidateAttributes`.

Defaults to `true`.

## Middleware & Helpers

Coalesce provides several helper extension methods to simplify common application setup tasks:

<Prop def="public static IApplicationBuilder UseViteStaticFiles(this IApplicationBuilder app, ViteStaticFilesOptions? options = null)" />

Configures static file middleware with optimizations for Vite build output. This middleware:

- Serves static files from `wwwroot`
- Applies long-term caching (30 days) to files with cache-busting hashes in their filenames (as produced by Vite)
- Supports optional authorization and response customization hooks

This calls `UseStaticFiles` internally, so it should be used in place of, not in addition to, a call to UseStaticFiles. If you need more advanced control, you should instead use UseStaticFiles directly.

``` c#
app.UseViteStaticFiles();
```

``` c#
// With custom authorization:
app.UseViteStaticFiles(new()
{
    OnAuthorizeAsync = ctx => ctx.Context.Request.Path.StartsWithSegments("/assets") == true
        // Vite compiled assets require authentication
        ? ValueTask.FromResult(ctx.Context.User.Identity?.IsAuthenticated == true)
        // Anything else (e.g. `src/public` directory) do not.
        : ValueTask.FromResult(true)
});
```

<Prop def="public static IApplicationBuilder UseNoCacheResponseHeader(this IApplicationBuilder app)" />

Adds a `Cache-Control: no-cache, no-store` header to all responses that reach this point in the pipeline. This middleware acts as a pre-hook, so the resulting `Cache-Control` header can be overridden by other middleware or individual endpoints.

This is useful for preventing browsers from unexpectedly caching API responses. Usually this is placed just after `.UseViteStaticFiles()`.

``` c#
app.UseNoCacheResponseHeader();
```

<Prop def="public static IEndpointConventionBuilder MapCoalesceSecurityOverview(this IEndpointRouteBuilder builder, string pattern)" />

Maps a route that presents an HTML page with a comprehensive overview of all types exposed by Coalesce and their effective security rules. This page displays:

- Class-level security attributes (`[Read]`, `[Create]`, `[Edit]`, `[Delete]`)
- Property-level security and restrictions
- Custom data sources and behaviors
- Method security rules
- Service endpoints

See the [Security Overview](/topics/security.md#security-overview-page) documentation for more details.

``` c#
app.MapCoalesceSecurityOverview("coalesce-security")
    .RequireAuthorization(new AuthorizeAttribute { Roles = "Admin" });
```


