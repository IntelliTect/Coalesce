using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IntelliTect.Coalesce.DataAnnotations;
using static IntelliTect.Coalesce.DataAnnotations.SecurityPermissionLevels;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace IntelliTect.Coalesce.AuditLogging;

[Edit(DenyAll)]
[Delete(DenyAll)]
[Create(DenyAll)]
public class ObjectChangeProperty
{
    public long ObjectChangePropertyId { get; set; }

    public long ObjectChangeId { get; set; }

    [ListText, Search, MaxLength(100), Column(TypeName = "varchar(100)")]
#if NET7_0_OR_GREATER 
required 
#endif
    public string PropertyName { get; set; } = null!;

    public string? OldValue { get; set; }

    public string? NewValue { get; set; }
}
