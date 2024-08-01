using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace IntelliTect.Coalesce.AuditLogging;

public class AuditEntryProperty
{
    public required PropertyEntry PropertyEntry { get; init; }

    public required AuditEntry Parent { get; init; }

    public string PropertyName => PropertyEntry.Metadata.Name;

    public bool IsKey => PropertyEntry.Metadata.IsKey();

    public object? NewValue { get; set; }
    public string? NewValueDescription { get; set; }

    public string? NewValueFormatted => Parent.State == AuditEntryState.EntityDeleted
        ? null
        : Parent.Parent.Configuration.GetFormattedValue(PropertyEntry, NewValue);

    public object? OldValue { get; set; }
    public string? OldValueDescription { get; set; }

    public string? OldValueFormatted => Parent.State == AuditEntryState.EntityAdded
        ? null
        : Parent.Parent.Configuration.GetFormattedValue(PropertyEntry, OldValue);


}
