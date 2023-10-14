using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Net;
using System.Security.Claims;

namespace IntelliTect.Coalesce.AuditLogging;

/// <summary>
/// Defines a service that can populate additional fields on the given <typeparamref name="TObjectChange"/> before it is persisted to the database. Declare the implementation of this service by calling <see cref="CoalesceAuditLoggingBuilder{TObjectChange}.WithAugmentation{T}"/>.
/// </summary>
public interface IAuditOperationContext<TObjectChange> : IAuditOperationContext
{
    /// <inheritdoc cref="IAuditOperationContext.Populate(IObjectChange, EntityEntry)"/>
    void Populate(TObjectChange auditEntry, EntityEntry changedEntity);

    void IAuditOperationContext.Populate(IObjectChange auditEntry, EntityEntry changedEntity)
    {
        Populate((TObjectChange) auditEntry, changedEntity);
    }
}

public interface IAuditOperationContext
{
    /// <summary>
    /// A hook that may be overridden to populate additional contextual information on an audit entry.
    /// </summary>
    void Populate(IObjectChange auditEntry, EntityEntry changedEntity);
}
