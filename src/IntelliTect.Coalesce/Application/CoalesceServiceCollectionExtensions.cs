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

namespace IntelliTect.Coalesce
{
    public static class CoalesceServiceCollectionExtensions
    {
        public static IServiceCollection AddCoalesce(this IServiceCollection services, Action<CoalesceServiceBuilder> builder)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder(new CoalesceServiceBuilder(services));

            // Needed for CrudContext to access the current user.
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton(_ => ReflectionRepository.Global);

            services.AddTransient<IConfigureOptions<MvcOptions>, ConfigureMvc>();

            services.TryAddScoped<IApiActionFilter, ApiActionFilter>();
            services.TryAddScoped<IDataSourceFactory, DataSourceFactory>();
            services.TryAddScoped<IBehaviorsFactory, BehaviorsFactory>();
            services.TryAddScoped<ITimeZoneResolver>(_ => new StaticTimeZoneResolver(TimeZoneInfo.Local));
            

            return services;
        }

        public static IServiceCollection AddCoalesce<TContext>(this IServiceCollection services)
            where TContext : DbContext
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            services.AddCoalesce(builder => builder.AddContext<TContext>());

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
    }
}
