using IntelliTect.Coalesce.AuditLogging.Internal;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Collections.Generic;

namespace IntelliTect.Coalesce.AuditLogging;

public class AuditEntry
{
    public required EntityEntry Entry { get; init; }

    public required CoalesceAudit Parent { get; init; }

    public required AuditEntryState State { get; set; }

    public List<AuditEntryProperty> Properties { get; set; } = new();

    public object Entity => Entry.Entity;
}
