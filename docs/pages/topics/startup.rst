


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
            );
        }

A summary is as follows:

    :csharp:`.AddContext<AppDbContext>()`
        Register services needed by Coalesce to use the specified context. This is done automatically when calling the :csharp:`services.AddCoalesce<AppDbContext>();` overload.
    :csharp:`.UseDefaultDataSource(typeof(MyDataSource<,>))` 
        Overrides the default data source used, replacing the :ref:`StandardDataSource`. See :ref:`CustomDataSources` for more details.
    :csharp:`.UseDefaultBehaviors(typeof(MyBehaviors<,>))` 
        Overrides the default behaviors used, replacing the :ref:`StandardBehaviors`. See :ref:`CustomBehaviors` for more details.
    :csharp:`.UseTimeZone(TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time"))`
        Specify a static time zone that should be used when Coalesce is performing operations on dates/times that lack timezone information. For example, when a user inputs a search term that contains only a date, Coalesce needs to know what timezone's midnight to use when performing the search.
    :csharp:`.UseTimeZone<ITimeZoneResolver>()` 
        Specify a service implementation to use to resolve the current timezone. This should be a scoped service, and will be automatically registered if it is not already. This allows retrieving timezone information on a per-request basis from HTTP headers, Cookies, or any other source.

