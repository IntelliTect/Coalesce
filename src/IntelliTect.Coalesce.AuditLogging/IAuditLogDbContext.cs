using Microsoft.EntityFrameworkCore;

namespace IntelliTect.Coalesce.AuditLogging;

/// <summary>
/// An interface representing the <see cref="DbSet{TEntity}"/>s that hold the data produced by Coalesce's audit logging features.
/// </summary>
/// <typeparam name="TAuditLog">The type of entity representing a change to an entity. You are expected to make your own implementation of this entity in your application code, inheriting from <see cref="DefaultAuditLog"/> or <see cref="IAuditLog"/>. 
/// </typeparam>
public interface IAuditLogDbContext<TAuditLog>
    where TAuditLog : class, IAuditLog
{
    DbSet<TAuditLog> AuditLogs { get; }

    DbSet<AuditLogProperty> AuditLogProperties { get; }

    /// <summary>
    /// When <see langword="true"/>, audit operations on the <see cref="DbContext"/> will be skipped.
    /// </summary>
    bool SuppressAudit => false;
}
