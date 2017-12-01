using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace IntelliTect.Coalesce
{
    public class CoalesceServiceBuilder
    {
        internal CoalesceServiceBuilder(IServiceCollection services)
        {
            Services = services;
        }

        internal IServiceCollection Services { get; }

        public CoalesceServiceBuilder AddContext<TContext>()
            where TContext : DbContext
        {
            ReflectionRepository.Global.AddAssembly<TContext>();
            Services.AddScoped(sp => new CrudContext<TContext>(
                sp.GetRequiredService<TContext>(),
                sp.GetRequiredService<Microsoft.AspNetCore.Http.HttpContext>().User,
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
    }
}
