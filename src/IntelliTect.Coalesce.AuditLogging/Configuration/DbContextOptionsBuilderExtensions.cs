using IntelliTect.Coalesce.AuditLogging.Internal;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;

namespace IntelliTect.Coalesce.AuditLogging;

public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Adds Coalesce's audit logging infrastructure to the <see cref="DbContext"/>. The <see cref="DbContext"/> must inherit from <see cref="IAuditLogContext{TAuditLog}"/>, where <typeparamref name="TAuditLog"/> derives from <see cref="IAuditLog"/> or <see cref="DefaultAuditLog"/>
    /// </summary>
    public static DbContextOptionsBuilder UseCoalesceAuditLogging<TAuditLog>(
        this DbContextOptionsBuilder builder,
        Action<CoalesceAuditLoggingBuilder<TAuditLog>>? configure = null
    )
        where TAuditLog : class, IAuditLog
    {
        var options = new AuditOptions();

        CoalesceAuditLoggingBuilder<TAuditLog> auditBuilder = new(options);
        auditBuilder = auditBuilder.ConfigureAudit(c =>
        {
            c.Exclude<TAuditLog>();
            c.Exclude<AuditLogProperty>();
        });
        configure?.Invoke(auditBuilder);

        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(new AuditExtension(options));

        builder.AddInterceptors(new AuditingInterceptor<TAuditLog>(options));

        return builder;
    }
}
