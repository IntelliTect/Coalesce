using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace IntelliTect.Coalesce.AuditLogging;

/// <summary>
/// A service that may be registered with <see cref="DbContextOptionsBuilderExtensions.AddCoalesceAuditLoggingOperationContext{TObjectChange, TOperationContext}(Microsoft.Extensions.DependencyInjection.IServiceCollection)"/> that can populate additional fields on the given <typeparamref name="TObjectChange"/> before it is persisted to the database.
/// </summary>
public interface IAuditOperationContext<TObjectChange>
    where TObjectChange : class, IObjectChange
{
    /// <summary>
    /// A hook that may be overridden to populate additional contextual information.
    /// Can be used in conjunction with, or as an alternative to, a customized <see cref="IObjectChange.Populate"/> method.
    /// </summary>
    void Populate(TObjectChange auditEntry, EntityEntry entity);
}
