using Microsoft.EntityFrameworkCore;

namespace IntelliTect.Coalesce.AuditLogging;

public interface IAuditLogContext<TObjectChange>
    where TObjectChange : class, IObjectChange
{
    DbSet<TObjectChange> ObjectChanges { get; }
    DbSet<ObjectChangeProperty> ObjectChangeProperties { get; }
    bool SuppressAudit { get; }
}
