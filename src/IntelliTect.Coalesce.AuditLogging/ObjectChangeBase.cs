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
    public long Id { get; set; }

    /// <inheritdoc/>
    [Required, MaxLength(100), Column(TypeName = "varchar(100)")]
    [ListText, Search]
    public string Type { get; set; } = null!;

    /// <inheritdoc/>
    public string? KeyValue { get; set; }

    /// <inheritdoc/>
    public AuditEntryState State { get; set; }

    /// <inheritdoc/>
    public DateTimeOffset Date { get; set; }

    /// <inheritdoc/>
    [ForeignKey(nameof(ObjectChangeProperty.ParentId))]
    public ICollection<ObjectChangeProperty>? Properties { get; set; }
    
    /// <inheritdoc cref="IObjectChange.Populate(DbContext, IServiceProvider, EntityEntry)"/>
    protected virtual void Populate(DbContext db, IServiceProvider serviceProvider, EntityEntry entry) { }

    /// <inheritdoc/>
    void IObjectChange.Populate(DbContext db, IServiceProvider serviceProvider, EntityEntry entry)
        => Populate(db, serviceProvider, entry);
}
