# Application Configuration


In order for Coalesce to work in your application, you must register the needed services in your ``Startup.cs`` file. Doing so is simple:

``` c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddCoalesce<AppDbContext>();
    ...
}
```

This registers all the basic services that Coalesce needs in order to work with your EF DbContext. However, there are many more options available. Here's a more complete invocation of `AddCoalesce` that takes advantage of many of the options available:

``` c#
public void ConfigureServices(IServiceCollection services)
{
    services.AddCoalesce(builder => builder
        .AddContext<AppDbContext>()
        .UseDefaultDataSource(typeof(MyDataSource<,>))
        .UseDefaultBehaviors(typeof(MyBehaviors<,>))
        .UseTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"))
        .Configure(o =>
        {
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
        });
    );
}
```

A summary is as follows:

`.AddContext<AppDbContext>()`

Register services needed by Coalesce to use the specified context. This is done automatically when calling the `services.AddCoalesce<AppDbContext>();` overload.

`.UseDefaultDataSource(typeof(MyDataSource<,>))` 

Overrides the default data source used, replacing the [Standard Data Source](/modeling/model-components/data-sources.md#standard-data-source). See [Data Sources](/modeling/model-components/data-sources.md) for more details.

`.UseDefaultBehaviors(typeof(MyBehaviors<,>))` 

Overrides the default behaviors used, replacing the [Standard Behaviors](/modeling/model-components/behaviors.md#standard-behaviors). See [Behaviors](/modeling/model-components/behaviors.md) for more details.

`.UseTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"))`

Specify a static time zone that should be used when Coalesce is performing operations on dates/times that lack timezone information. For example, when a user inputs a search term that contains only a date, Coalesce needs to know what timezone's midnight to use when performing the search.

`.UseTimeZone<ITimeZoneResolver>()` 

Specify a service implementation to use to resolve the current timezone. This should be a scoped service, and will be automatically registered if it is not already. This allows retrieving timezone information on a per-request basis from HTTP headers, Cookies, or any other source.

`.Configure(...)` 

Configure additional options for Coalesce runtime behavior. Current options include those around exception handling. See individual members for details.


