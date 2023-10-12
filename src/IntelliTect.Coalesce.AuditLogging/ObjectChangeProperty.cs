using IntelliTect.Coalesce.DataAnnotations;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static IntelliTect.Coalesce.DataAnnotations.SecurityPermissionLevels;

namespace IntelliTect.Coalesce.AuditLogging;

[Edit(DenyAll)]
[Delete(DenyAll)]
[Create(DenyAll)]
public class ObjectChangeProperty
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
    /// For add or modify operations, holds the new value of the property.
    /// </summary>
    public string? NewValue { get; set; }

    // FUTURE?: Add additional fields that get filled with the [ListText] of FK values? Could be used for other descriptive purposes too.
}
