using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace IntelliTect.Coalesce.AuditLogging;

/// <summary>
/// Defines a service that can populate additional fields on the given <typeparamref name="TObjectChange"/> before it is persisted to the database. Declare the implementation of this service by calling <see cref="CoalesceAuditLoggingBuilder{TObjectChange}.WithAugmentation{T}"/>.
/// </summary>
public interface IAuditOperationContext<TObjectChange> : IAuditOperationContext
{
    /// <inheritdoc cref="IAuditOperationContext.Populate(IObjectChange, EntityEntry)"/>
    void Populate(TObjectChange auditEntry, EntityEntry entity);

    void IAuditOperationContext.Populate(IObjectChange auditEntry, EntityEntry entity)
    {
        Populate((TObjectChange) auditEntry, entity);
    }
}

public interface IAuditOperationContext
{
    /// <summary>
    /// A hook that may be overridden to populate additional contextual information on an audit entry.
    /// </summary>
    void Populate(IObjectChange auditEntry, EntityEntry entity);
}