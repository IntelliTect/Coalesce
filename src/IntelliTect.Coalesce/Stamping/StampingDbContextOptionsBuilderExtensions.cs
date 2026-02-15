using IntelliTect.Coalesce.Stamping;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;

namespace IntelliTect.Coalesce;

public static class StampingDbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Adds a save interceptor that will perform a simple action during <see cref="DbContext.SaveChanges()"/> 
    /// on all <see cref="EntityState.Added"/> or <see cref="EntityState.Modified"/> entities.
    /// The action will receive the <see cref="ClaimsPrincipal"/> from the current <see cref="HttpContext"/>.
    /// </summary>
    /// <typeparam name="TStampable">The type of entity to be stamped. Can be a base class or interface.</typeparam>
    /// <param name="builder">The db context options to configure.</param>
    /// <param name="stampAction">The action to take against the entity.</param>
    public static DbContextOptionsBuilder UseStamping<TStampable>(
        this DbContextOptionsBuilder builder,
        Action<TStampable, ClaimsPrincipal?> stampAction
    )
        where TStampable : class
    {
        builder.AddInterceptors(new StampInterceptor<TStampable, IHttpContextAccessor>((target, accessor) =>
        {
            var user = accessor?.HttpContext?.User;
            stampAction(target, user);
        }));

        return builder;
    }

    /// <summary>
    /// Adds a save interceptor that will perform a simple action during <see cref="DbContext.SaveChanges()"/> 
    /// on all <see cref="EntityState.Added"/> or <see cref="EntityState.Modified"/> entities.
    /// The action will receive the service specified by <typeparamref name="TService"/>.
    /// </summary>
    /// <typeparam name="TStampable">The type of entity to be stamped. Can be a base class or interface.</typeparam>
    /// <typeparam name="TService">The service to dependency inject into the action.</typeparam>
    /// <param name="builder">The db context options to configure.</param>
    /// <param name="stampAction">The action to take against the entity.</param>
    public static DbContextOptionsBuilder UseStamping<TStampable, TService>(
        this DbContextOptionsBuilder builder,
        Action<TStampable, TService?> stampAction
    )
        where TStampable : class
    {
        builder.AddInterceptors(new StampInterceptor<TStampable, TService>(stampAction));

        return builder;
    }

    /// <summary>
    /// Adds a save interceptor that will perform a simple action during <see cref="DbContext.SaveChanges()"/> 
    /// on all <see cref="EntityState.Added"/> or <see cref="EntityState.Modified"/> entities.
    /// The action will receive the services specified by <typeparamref name="TService1"/> and <typeparamref name="TService2"/>.
    /// </summary>
    /// <typeparam name="TStampable">The type of entity to be stamped. Can be a base class or interface.</typeparam>
    /// <typeparam name="TService1">A service to dependency inject into the action.</typeparam>
    /// <typeparam name="TService2">A second service to dependency inject into the action.</typeparam>
    /// <param name="builder">The db context options to configure.</param>
    /// <param name="stampAction">The action to take against the entity.</param>
    public static DbContextOptionsBuilder UseStamping<TStampable, TService1, TService2>(
        this DbContextOptionsBuilder builder,
        Action<TStampable, TService1?, TService2?> stampAction
    )
        where TStampable : class
    {
        builder.AddInterceptors(new StampInterceptor<TStampable, TService1, TService2>(stampAction));

        return builder;
    }
}
