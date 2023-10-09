using IntelliTect.Coalesce.AuditLogging.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace IntelliTect.Coalesce.AuditLogging;

public static class DbContextOptionsBuilderExtensions
{
    public static DbContextOptionsBuilder UseCoalesceAuditLogging<TObjectChange>(
        this DbContextOptionsBuilder builder
    )
        where TObjectChange : class, IObjectChange
    {
        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(new AuditExtension<TObjectChange>());

        return builder.AddInterceptors(new AuditingInterceptor<TObjectChange>());
    }

    /// <summary>
    /// Registers a service that can be used to populate additional contextual information on audit log entries.
    /// Can be used in conjunction with, or as an alternative to, a customized <see cref="IObjectChange.Populate"/> method.
    /// </summary>
    public static IServiceCollection AddCoalesceAuditLoggingOperationContext<TObjectChange, TOperationContext>(
        this IServiceCollection services
    )
        where TObjectChange : class, IObjectChange
        where TOperationContext : class, IAuditOperationContext<TObjectChange>
    {
        services.TryAddTransient<IAuditOperationContext<TObjectChange>, TOperationContext>();
        return services;
    }

    ///// <summary>
    ///// Add Coalesce audit logging, including a <typeparamref name="TOperationContext"/> whose dependencies can be entirely resolved from the <see cref="DbContext"/>'s own internal service provider, plus <see cref="IHttpContextAccessor"/>.
    ///// </summary>
    //public static DbContextOptionsBuilder UseCoalesceAuditLogging<TObjectChange, TOperationContext>(
    //    this DbContextOptionsBuilder builder
    //)
    //    where TObjectChange : class, IObjectChange
    //    where TOperationContext : class, IAuditOperationContext<TObjectChange>
    //{
    //    ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(new AuditExtension<TObjectChange>(typeof(TOperationContext)));

    //    return builder.AddInterceptors(new AuditingInterceptor<TObjectChange>());
    //}
}
