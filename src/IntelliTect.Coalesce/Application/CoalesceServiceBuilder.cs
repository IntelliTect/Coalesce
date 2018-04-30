using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Collections.Generic;

namespace IntelliTect.Coalesce
{
    public class CoalesceServiceBuilder
    {
        internal CoalesceServiceBuilder(IServiceCollection services)
        {
            Services = services;
        }

        internal IServiceCollection Services { get; }

        /// <summary>
        /// Add services related to the given context to the service container.
        /// This allows the given context to work with the standard data sources and behaviors.
        /// </summary>
        /// <typeparam name="TContext">The DbContext that Coalesce needs to be able to handle.</typeparam>
        public CoalesceServiceBuilder AddContext<TContext>()
            where TContext : DbContext
        {
            ReflectionRepository.Global.AddAssembly<TContext>();
            Services.AddScoped(sp => new CrudContext<TContext>(
                sp.GetRequiredService<TContext>(),
                sp.GetRequiredService<Microsoft.AspNetCore.Http.IHttpContextAccessor>().HttpContext.User,
                sp.GetService<ITimeZoneResolver>()?.GetTimeZoneInfo() ?? TimeZoneInfo.Local
            ));

            return this;
        }

        /// <summary>
        /// Specify the TimeZoneInfo that Coalesce will use when performing operations on dates
        /// that lack a Time Zone component (DateTime objects, for example).
        /// </summary>
        /// <param name="timeZoneInfo">A static TimeZoneInfo to use.</param>
        public CoalesceServiceBuilder UseTimeZone(TimeZoneInfo timeZoneInfo)
        {
            Services.TryAddScoped<ITimeZoneResolver>(_ => new StaticTimeZoneResolver(timeZoneInfo));
            return this;
        }

        /// <summary>
        /// Specify a scoped service for resolving the TimeZoneInfo that Coalesce will use when performing operations on dates
        /// that lack a Time Zone component (DateTime objects, for example).
        /// </summary>
        /// <typeparam name="TResolver"></typeparam>
        /// <returns></returns>
        public CoalesceServiceBuilder UseTimeZone<TResolver>()
            where TResolver : class, ITimeZoneResolver
        {
            Services.TryAddScoped<ITimeZoneResolver, TResolver>();
            return this;
        }

        private CoalesceServiceBuilder UseDefaultCrudStrategy(Type implementationType, IEnumerable<Type> serviceCandidates)
        {
            if (implementationType.IsInterface || implementationType.IsAbstract)
            {
                throw new ArgumentException(
                    "Can't register an interface or abstract class as an implementation type.",
                    nameof(implementationType));
            }


            bool foundMatch = false;
            foreach (var serviceType in serviceCandidates)
            {
                if (new ReflectionTypeViewModel(implementationType).IsA(serviceType))
                {
                    Services.AddScoped(serviceType, implementationType);
                    foundMatch = true;
                }
            }

            if (!foundMatch)
            {
                throw new ArgumentException(
                    $"The supplied type {implementationType} did not match any known marker interfaces. " +
                    $"Valid interfaces are: {string.Join(",", serviceCandidates)}");
            }

            return this;
        }


        /// <summary>
        /// Use the given data source type as the default implementation where suitable.
        /// The data source will be used whenever no custom data source is found.
        /// This type should be an non-constructed generic class that is compatible with a known marker interface.
        /// </summary>
        /// <param name="implementationType"></param>
        /// <returns></returns>
        public CoalesceServiceBuilder UseDefaultDataSource(Type implementationType)
            => UseDefaultCrudStrategy(implementationType, Api.DataSources.DataSourceFactory.DefaultTypes.Keys);


        /// <summary>
        /// Use the given behaviors type as the default implementation where suitable.
        /// The behaviors will be used whenever no custom behaviors are found.
        /// This type should be an non-constructed generic class that is compatible with a known marker interface.
        /// </summary>
        /// <param name="implementationType"></param>
        /// <returns></returns>
        public CoalesceServiceBuilder UseDefaultBehaviors(Type implementationType)
            => UseDefaultCrudStrategy(implementationType, Api.Behaviors.BehaviorsFactory.DefaultTypes.Keys);
    }
}
