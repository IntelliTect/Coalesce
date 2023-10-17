﻿using IntelliTect.Coalesce.AuditLogging.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace IntelliTect.Coalesce.AuditLogging;

public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Adds Coalesce's audit logging infrastructure to the <see cref="DbContext"/>. The <see cref="DbContext"/> must inherit from <see cref="IAuditLogContext{TObjectChange}"/>, where <typeparamref name="TObjectChange"/> derives from <see cref="IObjectChange"/> or <see cref="DefaultObjectChange"/>
    /// </summary>
    public static DbContextOptionsBuilder UseCoalesceAuditLogging<TObjectChange>(
        this DbContextOptionsBuilder builder,
        Action<CoalesceAuditLoggingBuilder<TObjectChange>>? configure = null
    )
        where TObjectChange : class, IObjectChange
    {
        var options = new AuditOptions();

        configure?.Invoke(new(options));

        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(new AuditExtension(options));

        builder.AddInterceptors(new AuditingInterceptor<TObjectChange>(options));

        return builder;
    }
}