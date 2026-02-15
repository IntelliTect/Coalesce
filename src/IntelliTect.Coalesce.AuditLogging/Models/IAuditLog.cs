using System;
using System.Collections.Generic;

namespace IntelliTect.Coalesce.AuditLogging;

/// <summary>
/// A representation of a change performed on an entity in the database.
/// </summary>
public interface IAuditLog
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
    /// A description of the changed entity that may be populated by the framework
    /// according to the configuration of <see cref="AuditOptions.Descriptions"/>.
    /// </summary>
    string? Description { get; set; }

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
    ICollection<AuditLogProperty>? Properties { get; set; }
}
