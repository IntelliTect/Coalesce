using IntelliTect.Coalesce.Api.Behaviors;
using IntelliTect.Coalesce.Api.Controllers;
using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;






#if NET9_0_OR_GREATER
using IntelliTect.Coalesce.Api.OpenApi;
using Microsoft.AspNetCore.OpenApi;
#endif

namespace IntelliTect.Coalesce;

public static class CoalesceServiceCollectionExtensions
{
    public static IServiceCollection AddCoalesce(this IServiceCollection services, Action<CoalesceServiceBuilder> builder)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (builder == null) throw new ArgumentNullException(nameof(builder));

        services.AddOptions<CoalesceOptions>();

        if (services.Any(s => s.ServiceType.IsAssignableFrom(typeof(IWebHostEnvironment))))
        {
            services.AddOptions<CoalesceOptions>().Configure<IWebHostEnvironment>((opts, hosting) =>
            {
                opts.DetailedExceptionMessages = hosting.IsDevelopment();
            });
        }

        builder(new CoalesceServiceBuilder(services));

        // Needed for CrudContext to access the current user.
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.TryAddSingleton(ReflectionRepository.Global);

        var entryAsm = Assembly.GetEntryAssembly();
        if (entryAsm is not null)
        {
            // Needed to discover the generated DTOs for bulk saves:
            ReflectionRepository.Global.AddAssembly(entryAsm);
        }

        services.AddTransient<IConfigureOptions<MvcOptions>, ConfigureMvc>();

        services.Configure<JsonOptions>(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        });

        services.TryAddScoped<IApiActionFilter, ApiActionFilter>();

        void AddFactoryDefaultTypes(IDictionary<Type, Type> types)
        {
            foreach (var type in types) services.TryAddScoped(type.Key, type.Value);
        }

        services.TryAddScoped<IDataSourceFactory, DataSourceFactory>();
        AddFactoryDefaultTypes(DataSourceFactory.DefaultTypes);

        services.TryAddScoped<IBehaviorsFactory, BehaviorsFactory>();
        AddFactoryDefaultTypes(BehaviorsFactory.DefaultTypes);

        services.TryAddScoped<IBehaviorsFactory, BehaviorsFactory>();
        services.TryAddScoped<ITimeZoneResolver>(_ => new StaticTimeZoneResolver(TimeZoneInfo.Local));

        services.TryAddScoped(sp => new CrudContext(
             () => sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.User ?? new System.Security.Claims.ClaimsPrincipal(),
             sp.GetService<ITimeZoneResolver>()?.GetTimeZoneInfo() ?? TimeZoneInfo.Local,
             sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.RequestAborted ?? default,
             sp.GetRequiredService<IOptions<CoalesceOptions>>().Value,
             sp
         ));

        // Workaround for https://github.com/dotnet/aspnetcore/issues/43815
        services.TryAddEnumerable(
            ServiceDescriptor.Transient<IApiDescriptionProvider, FromFormNameFixingApiDescriptionProvider>());

        // Make adjustments to the API explorer so that it doesn't cause .NET 9's OpenAPI generation to implode.
        services.TryAddEnumerable(
            ServiceDescriptor.Transient<IApiDescriptionProvider, IntelliTect.Coalesce.Api.CoalesceApiDescriptionProvider>());
#if NET9_0_OR_GREATER
        services.ConfigureAll<OpenApiOptions>(x =>
        {
            x.AddOperationTransformer<CoalesceApiOperationFilter>();
        });
#endif

        return services;
    }

    public static IServiceCollection AddCoalesce<TContext>(this IServiceCollection services, Action<CoalesceServiceBuilder>? builder = null)
        where TContext : DbContext
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

        services.AddCoalesce(b =>
        {
            b.AddContext<TContext>();
            builder?.Invoke(b);
        });

        return services;
    }

    /// <summary>
    /// Register <see cref="IUrlHelper" /> as a service that can be injected
    /// when operating in the context of an executing action (i.e. a controller endpoint).
    /// </summary>
    public static IServiceCollection AddUrlHelper(this IServiceCollection services)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));

#if NET10_0_OR_GREATER
        services.AddScoped<IUrlHelper>(x =>
        {
            var httpContext = x.GetRequiredService<IHttpContextAccessor>().HttpContext;
            var endpoint = httpContext?.GetEndpoint();
            var actionDescriptor = endpoint?.Metadata.GetMetadata<ActionDescriptor>();

            var factory = x.GetRequiredService<IUrlHelperFactory>();
            return factory.GetUrlHelper(httpContext is not null
                ? new ActionContext(httpContext, httpContext.GetRouteData(), actionDescriptor ?? new())
                : new());
        });
#else
        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();
        services.AddScoped<IUrlHelper>(x =>
        {
            var actionContext = x.GetRequiredService<IActionContextAccessor>().ActionContext;
            var factory = x.GetRequiredService<IUrlHelperFactory>();
            return factory.GetUrlHelper(actionContext!);
        });
#endif

        return services;
    }

    private class ConfigureMvc : IConfigureOptions<MvcOptions>
    {
        public void Configure(MvcOptions options)
        {
            options.ModelBinderProviders.Insert(0, new DataSourceModelBinderProvider());
            options.ModelBinderProviders.Insert(0, new BehaviorsModelBinderProvider());
        }
    }

    /// <summary>
    /// Workaround for https://github.com/dotnet/aspnetcore/issues/43815
    /// </summary>
    private class FromFormNameFixingApiDescriptionProvider : IApiDescriptionProvider
    {
        public int Order => -999; // one after DefaultApiDescriptionProvider

        public void OnProvidersExecuting(ApiDescriptionProviderContext context)
        {
            foreach (var result in context.Results)
            {
                foreach (var parameter in result.ParameterDescriptions)
                {
                    if (
                        parameter.Source == Microsoft.AspNetCore.Mvc.ModelBinding.BindingSource.Form &&
                        parameter.ParameterDescriptor.BindingInfo?.BinderModelName is string prefix &&
                        parameter.ModelMetadata.ContainerType is not null &&
                        !parameter.Name.StartsWith(prefix + ".", StringComparison.InvariantCultureIgnoreCase)
                    )
                    {
                        parameter.Name = prefix + "." + parameter.Name;
                    }
                }
            }
        }

        public void OnProvidersExecuted(ApiDescriptionProviderContext context)
        {
        }
    }
}
