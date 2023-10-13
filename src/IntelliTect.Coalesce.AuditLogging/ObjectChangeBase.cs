using IntelliTect.Coalesce.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static IntelliTect.Coalesce.DataAnnotations.SecurityPermissionLevels;

namespace IntelliTect.Coalesce.AuditLogging;

[Edit(DenyAll)]
[Delete(DenyAll)]
[Create(DenyAll)]
public abstract class ObjectChangeBase : IObjectChange
{
    /// <inheritdoc/>
    [Key, DefaultOrderBy(OrderByDirection = DefaultOrderByAttribute.OrderByDirections.Descending)]
    public virtual long Id { get; set; }

    /// <inheritdoc/>
    [Required, MaxLength(100), Column(TypeName = "varchar(100)")]
    [ListText, Search]
    public virtual string Type { get; set; } = null!;

    /// <inheritdoc/>
    public virtual string? KeyValue { get; set; }

    /// <inheritdoc/>
    [Display(Name = "Change Type")]
    public virtual AuditEntryState State { get; set; }

    /// <inheritdoc/>
    public virtual DateTimeOffset Date { get; set; }

    /// <inheritdoc/>
    [ForeignKey(nameof(ObjectChangeProperty.ParentId))]
    public virtual ICollection<ObjectChangeProperty>? Properties { get; set; }
}
