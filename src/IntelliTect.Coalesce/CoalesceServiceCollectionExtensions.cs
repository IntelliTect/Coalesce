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
        public static IServiceCollection AddCoalesce(this IServiceCollection services)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            var dbContexts = services.Where(s => typeof(DbContext).IsAssignableFrom(s.ServiceType)).ToList();
            if (dbContexts.Count == 0)
            {
                throw new InvalidOperationException("When adding Coalesce to an IServiceCollection that doesn't already contain an EF Core DbContext, " +
                    $"you must use the {nameof(AddCoalesce)} overload that accepts a generic parameter specifying the type of the context to use.");
            }

            foreach (var context in dbContexts) ReflectionRepository.Global.AddContext(context.ImplementationType);

            services.TryAddSingleton(_ => ReflectionRepository.Global);

            return services;
        }

        public static IServiceCollection AddCoalesce<TContext>(this IServiceCollection services)
            where TContext : DbContext
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            ReflectionRepository.Global.AddContext<TContext>();

            services.TryAddSingleton(_ => ReflectionRepository.Global);

            //services.TryAddSingleton(_ => {
            //    ReflectionRepository.Global.AddContext
            //    return ReflectionRepository.Global;
            //});

            return services;
        }
    }
}
