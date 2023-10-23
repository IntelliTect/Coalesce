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
public abstract class DefaultAuditLog : IAuditLog
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
    [ForeignKey(nameof(AuditLogProperty.ParentId))]
    public virtual ICollection<AuditLogProperty>? Properties { get; set; }

    /// <summary>
    /// The IP address of the client, if the change resulted from an incoming request.
    /// </summary>
    [Display(Name = "Client IP")]
    public string? ClientIp { get; set; }

    /// <summary>
    /// The value of the Referrer header, if the change resulted from an incoming request.
    /// </summary>
    public string? Referrer { get; set; }

    /// <summary>
    /// The URL of the endpoint that caused the change, if the change resulted from an incoming request.
    /// </summary>
    public string? Endpoint { get; set; }
}
