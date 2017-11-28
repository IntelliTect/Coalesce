using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IntelliTect.Coalesce
{
    public static class CoalesceServiceCollectionExtensions
    {
        public static IServiceCollection AddCoalesce(this IServiceCollection services, Action<CoalesceServiceBuilder> builder)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (builder == null) throw new ArgumentNullException(nameof(builder));

            builder(new CoalesceServiceBuilder(services));
            
            services.TryAddSingleton(_ => ReflectionRepository.Global);
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
    }
}
