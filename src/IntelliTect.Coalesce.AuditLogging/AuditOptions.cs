using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;

namespace IntelliTect.Coalesce.AuditLogging;

public class AuditOptions
{
    /// <summary>
    /// <para>
    /// For supported DB providers (currently only SQL Server), specifies a length of time
    /// during which repeated changes to the same record that are entirely identical other than
    /// their timestamp and specific <see cref="ObjectChangeProperty.NewValue"/>s will be merged 
    /// together into a single change. 
    /// </para>
    /// <para>
    /// This allows, for example, repeated auto-save changes to 
    /// large text fields to not cause the audit log to not fill up with hundreds of changes with nearly identical copies of the same string
    /// </para>
    /// <para>
    /// The default is 30 seconds.</para>
    /// </summary>
    public TimeSpan MergeWindow { get; set; } = TimeSpan.FromSeconds(30);

    public Type? OperationContextType { get; internal set; }
}