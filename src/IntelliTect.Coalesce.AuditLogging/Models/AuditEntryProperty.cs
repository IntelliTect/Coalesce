using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace IntelliTect.Coalesce.AuditLogging;

public class AuditEntryProperty
{

    public required PropertyEntry PropertyEntry { get; init; }

    public required AuditEntry Parent { get; init; }

    public object? NewValue { get; set; }

    public object? OldValue { get; set; }

    public string PropertyName => PropertyEntry.Metadata.Name;

    public bool IsKey => PropertyEntry.Metadata.IsKey();

    public string? NewValueFormatted => Parent.State == AuditEntryState.EntityDeleted ? null : Parent.Parent.Configuration.GetFormattedValue(PropertyEntry, NewValue);

    public string? OldValueFormatted => Parent.State == AuditEntryState.EntityAdded ? null : Parent.Parent.Configuration.GetFormattedValue(PropertyEntry, OldValue);
}
