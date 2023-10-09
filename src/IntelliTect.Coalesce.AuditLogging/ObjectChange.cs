using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;
using IntelliTect.Coalesce.DataAnnotations;
using static IntelliTect.Coalesce.DataAnnotations.SecurityPermissionLevels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Z.EntityFramework.Plus;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace IntelliTect.Coalesce.AuditLogging;

[Edit(DenyAll)]
[Delete(DenyAll)]
[Create(DenyAll)]
public abstract class ObjectChange : IObjectChange
{
    [DefaultOrderBy(OrderByDirection = DefaultOrderByAttribute.OrderByDirections.Descending)]
    [Key]
    public long ObjectChangeId { get; set; }

    [MaxLength(100), Column(TypeName = "varchar(100)")]
    [ListText, Search]
    public string EntityTypeName { get; set; } = null!;

    public string? EntityKeyValue { get; set; }

    public DateTimeOffset Date { get; set; }

    public ICollection<ObjectChangeProperty>? Properties { get; set; }

    /// <summary>
    /// Stringified value of <see cref="AuditEntryState"/>
    /// </summary>
    [MaxLength(30), Column(TypeName = "varchar(30)")]
    public string State { get; set; } = null!;

    /// <summary>
    /// A hook that may be overridden to populate additional contextual information.
    /// Can be used in conjunction with, or as an alternative to, a registered <see cref="IAuditOperationContext{TObjectChange}"/> service.
    /// </summary>
    protected virtual void Populate(DbContext db, IServiceProvider serviceProvider, EntityEntry entry) { }

    void IObjectChange.Populate(DbContext db, IServiceProvider serviceProvider, EntityEntry entry)
        => Populate(db, serviceProvider, entry);
}
