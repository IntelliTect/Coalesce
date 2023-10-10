using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;

namespace IntelliTect.Coalesce.AuditLogging;

/// <summary>
/// A representation of a change performed on an entity in the database.
/// </summary>
public interface IObjectChange
{
    long Id { get; set; }

    /// <summary>
    /// The C# type name of the entity that was affected.
    /// </summary>
    string Type { get; set; }

    /// <summary>
    /// The primary key value of the entity that was affected. For composite PKs, multiple values are delimited by semicolons.
    /// </summary>
    string? KeyValue { get; set; }

    /// <summary>
    /// The date when the change occurred.
    /// </summary>
    DateTimeOffset Date { get; set; }

    /// <summary>
    /// The type of change that occurred.
    /// </summary>
    AuditEntryState State { get; set; }

    /// <summary>
    /// The individual column/property changes made to the entity.
    /// </summary>
    ICollection<ObjectChangeProperty>? Properties { get; set; }

    /// <summary>
    /// A hook that may be overridden to populate additional contextual information.
    /// Can be used in conjunction with, or as an alternative to, a registered <see cref="IAuditOperationContext{TObjectChange}"/> service.
    /// </summary>
    void Populate(DbContext db, IServiceProvider serviceProvider, EntityEntry entry) { }
}
