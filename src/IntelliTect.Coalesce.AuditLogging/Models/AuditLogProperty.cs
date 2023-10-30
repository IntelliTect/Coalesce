using IntelliTect.Coalesce.DataAnnotations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static IntelliTect.Coalesce.DataAnnotations.SecurityPermissionLevels;

namespace IntelliTect.Coalesce.AuditLogging;

// All API endpoints are disabled.
// These records are meant to only be read through IAuditLog.Properties.
[Read(DenyAll)]
[Edit(DenyAll)]
[Delete(DenyAll)]
[Create(DenyAll)]
public class AuditLogProperty
{
    public long Id { get; set; }

    public long ParentId { get; set; }

    /// <summary>
    /// The name of the a property on the parent entity that was changed.
    /// </summary>
    [Required, ListText, Search, MaxLength(100), Column(TypeName = "varchar(100)")]
#if NET7_0_OR_GREATER 
required 
#endif
    public string PropertyName { get; set; } = null!;

    /// <summary>
    /// For modify or delete operations, holds the old value of the property.
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// Additional descriptive information about <see cref="OldValue"/>. For example, a string describing the value of a foreign key.
    /// </summary>
    public string? OldValueDescription { get; set; }

    /// <summary>
    /// For add or modify operations, holds the new value of the property.
    /// </summary>
    public string? NewValue { get; set; }

    /// <summary>
    /// Additional descriptive information about <see cref="NewValue"/>. For example, a string describing the value of a foreign key.
    /// </summary>
    public string? NewValueDescription { get; set; }
}
