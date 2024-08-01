namespace IntelliTect.Coalesce.AuditLogging;

/// <summary>
/// Represents the kind of operation that was performed on an entity.
/// </summary>
public enum AuditEntryState : byte
{
    /// <summary>
    /// The entity was added
    /// </summary>
    EntityAdded = 0,

    /// <summary>
    /// The entity was deleted
    /// </summary>
    EntityDeleted = 1,

    /// <summary>
    /// The entity was modified
    /// </summary>
    EntityModified = 2
}
