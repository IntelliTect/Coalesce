using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Net;
using System.Security.Claims;

namespace IntelliTect.Coalesce.AuditLogging;

/// <summary>
/// Defines a service that can populate additional fields on the given <typeparamref name="TAuditLog"/> before it is persisted to the database. Declare the implementation of this service by calling <see cref="CoalesceAuditLoggingBuilder{TAuditLog}.WithAugmentation{T}"/>.
/// </summary>
public interface IAuditOperationContext<TAuditLog> : IAuditOperationContext
{
    /// <inheritdoc cref="IAuditOperationContext.Populate(IAuditLog, EntityEntry)"/>
    void Populate(TAuditLog auditEntry, EntityEntry changedEntity);

    void IAuditOperationContext.Populate(IAuditLog auditEntry, EntityEntry changedEntity)
    {
        Populate((TAuditLog) auditEntry, changedEntity);
    }
}

public interface IAuditOperationContext
{
    /// <summary>
    /// A hook that may be overridden to populate additional contextual information on an audit entry.
    /// </summary>
    void Populate(IAuditLog auditEntry, EntityEntry changedEntity);
}
