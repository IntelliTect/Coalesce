using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;

namespace IntelliTect.Coalesce.AuditLogging;

public interface IObjectChange
{
    long ObjectChangeId { get; set; }

    string EntityTypeName { get; set; }

    string? EntityKeyValue { get; set; }

    DateTimeOffset Date { get; set; }

    string State { get; set; }

    ICollection<ObjectChangeProperty>? Properties { get; set; }

    /// <summary>
    /// A hook that may be overridden to populate additional contextual information.
    /// Can be used in conjunction with, or as an alternative to, a registered <see cref="IAuditOperationContext{TObjectChange}"/> service.
    /// </summary>
    void Populate(DbContext db, IServiceProvider serviceProvider, EntityEntry entry) { }
}
