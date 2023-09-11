using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using IntelliTect.Coalesce.Api.Behaviors;
using IntelliTect.Coalesce.Api.Controllers;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using System.Reflection;

namespace IntelliTect.Coalesce
{
    public static class CoalesceServiceCollectionExtensions
    {
        public static IServiceCollection AddCoalesce(this IServiceCollection services, Action<CoalesceServiceBuilder> builder)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (builder == null) throw new ArgumentNullException(nameof(builder));

#if NETCOREAPP
            services.AddOptions<CoalesceOptions>().Configure<IWebHostEnvironment>((opts, hosting) =>
            {
                opts.DetailedExceptionMessages = 
                    Microsoft.Extensions.Hosting.HostEnvironmentEnvExtensions.IsDevelopment(hosting);
            });
#else
            services.AddOptions<CoalesceOptions>().Configure<IHostingEnvironment>((opts, hosting) =>
            {
                opts.DetailedExceptionMessages = hosting.IsDevelopment();
            });
#endif

            builder(new CoalesceServiceBuilder(services));

            // Needed for CrudContext to access the current user.
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton(_ => ReflectionRepository.Global);

            var entryAsm = Assembly.GetEntryAssembly();
            if (entryAsm is not null)
            {
                // Needed to disover the generated DTOs for bulk saves:
                ReflectionRepository.Global.AddAssembly(entryAsm);
            }

            services.AddTransient<IConfigureOptions<MvcOptions>, ConfigureMvc>();

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
                 () => sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.User,
                 sp.GetService<ITimeZoneResolver>()?.GetTimeZoneInfo() ?? TimeZoneInfo.Local,
                 sp.GetRequiredService<IHttpContextAccessor>().HttpContext?.RequestAborted ?? default,
                 sp.GetRequiredService<IOptions<CoalesceOptions>>().Value,
                 sp
             ));

            // Workaround for https://github.com/dotnet/aspnetcore/issues/43815
            services.TryAddEnumerable(
                ServiceDescriptor.Transient<IApiDescriptionProvider, FromFormNameFixingApiDescriptionProvider>());

            return services;
        }

        public static IServiceCollection AddCoalesce<TContext>(this IServiceCollection services, Action<CoalesceServiceBuilder>? builder = null)
            where TContext : DbContext
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddCoalesce(b => {
                b.AddContext<TContext>();
                builder?.Invoke(b);
            });

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
}
