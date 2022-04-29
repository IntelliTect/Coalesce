


Application Configuration
=========================


In order for Coalesce to work in your application, you must register the needed services in your ``Startup.cs`` file. Doing so is simple:

    .. code-block:: c#

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCoalesce<AppDbContext>();
            ...
        }

This registers all the basic services that Coalesce needs in order to work with your EF DbContext. However, there are many more options available. Here's a more complete invocation of :csharp:`AddCoalesce` that takes advantage of many of the options available:

    .. code-block:: c#

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

A summary is as follows:

    :csharp:`.AddContext<AppDbContext>()`
        Register services needed by Coalesce to use the specified context. This is done automatically when calling the :csharp:`services.AddCoalesce<AppDbContext>();` overload.
    :csharp:`.UseDefaultDataSource(typeof(MyDataSource<,>))` 
        Overrides the default data source used, replacing the :ref:`StandardDataSource`. See :ref:`DataSources` for more details.
    :csharp:`.UseDefaultBehaviors(typeof(MyBehaviors<,>))` 
        Overrides the default behaviors used, replacing the :ref:`StandardBehaviors`. See :ref:`Behaviors` for more details.
    :csharp:`.UseTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"))`
        Specify a static time zone that should be used when Coalesce is performing operations on dates/times that lack timezone information. For example, when a user inputs a search term that contains only a date, Coalesce needs to know what timezone's midnight to use when performing the search.
    :csharp:`.UseTimeZone<ITimeZoneResolver>()` 
        Specify a service implementation to use to resolve the current timezone. This should be a scoped service, and will be automatically registered if it is not already. This allows retrieving timezone information on a per-request basis from HTTP headers, Cookies, or any other source.
    :csharp:`.Configure(...)` 
        Configure additional options for Coalesce runtime behavior. Current options include those around exception handling. See individual members for details.


.. _SecurityOverviewPage: 

Security Overview Page
----------------------

Coalesce provides batteries-included page that you can view to review the security rules in place for all the Coalesce-generated code in your project. Add this page to your application by mapping it as a route, either directly on :csharp:`WebHost` in .NET 6+, or in :csharp:`UseEndpoints` for 3.1+.

    .. code-block:: c#

        // .NET 6 Program.cs:

        // If exposing in production, it is strongly encourage to apply an authorization policy like this one.
        // Alternative, make the entire endpoint conditional upon the environment.
        app.MapCoalesceSecurityOverview("coalesce-security").RequireAuthorization(
            new AuthorizeAttribute { Roles = env.IsDevelopment() ? null : "Admin" }
        );

    .. code-block:: c#

        // .NET 3.1+ Startup.cs:

        app.UseEndpoints(endpoints =>
        {
            // If exposing in production, it is strongly encourage to apply an authorization policy like this one.
            // Alternative, make the entire endpoint conditional upon the environment.
            endpoints.MapCoalesceSecurityOverview("coalesce-security").RequireAuthorization(
                new AuthorizeAttribute { Roles = env.IsDevelopment() ? null : "Admin" }
            );

            // ... Other endpoints
        });
