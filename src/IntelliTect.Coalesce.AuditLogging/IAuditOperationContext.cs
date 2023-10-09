using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace IntelliTect.Coalesce.AuditLogging;

public interface IAuditOperationContext<TObjectChange>
    where TObjectChange : class, IObjectChange
{
    /// <summary>
    /// A hook that may be overridden to populate additional contextual information.
    /// Can be used in conjunction with, or as an alternative to, a customized <see cref="IObjectChange.Populate"/> method.
    /// </summary>
    void Populate(TObjectChange auditEntry, EntityEntry entity);
}
