using IntelliTect.Coalesce.DataAnnotations;
using System;
using Z.EntityFramework.Plus;

namespace IntelliTect.Coalesce.AuditLogging;

public class AuditOptions
{
    /// <summary>
    /// <para>
    /// For supported DB providers (currently only SQL Server), specifies a length of time
    /// during which repeated changes to the same record that are entirely identical other than
    /// their timestamp and specific <see cref="AuditLogProperty.NewValue"/>s will be merged 
    /// together into a single change. 
    /// </para>
    /// <para>
    /// This allows, for example, repeated auto-save changes to 
    /// large text fields to not cause the audit log to not fill up with hundreds of changes with nearly identical copies of the same string
    /// </para>
    /// <para>
    /// The default is 30 seconds.</para>
    /// </summary>
    public TimeSpan MergeWindow { get; internal set; } = TimeSpan.FromSeconds(30);

    public Type? OperationContextType { get; internal set; }

    /// <summary>
    /// <para>
    /// Controls how <see cref="IAuditLog.Description"/> is populated by the framework.
    /// </para>
    /// <para>
    /// The default behavior, <see cref="DescriptionMode.ListText"/>, will result in audit logs being described by 
    /// list text of the changed entity (as defined by <see cref="ListTextAttribute"/>).
    /// </para>
    /// </summary>
    public DescriptionMode Descriptions { get; internal set; } = DescriptionMode.ListText;

    /// <summary>
    /// <para>
    /// Control how <see cref="AuditLogProperty.OldValueDescription"/> and <see cref="AuditLogProperty.NewValueDescription"/>
    /// are populated by the framework.
    /// </para>
    /// <para>
    /// The default behavior, <see cref="PropertyDescriptionMode.FkListText"/>, will result foreign key properties
    /// being described by the list text (as defined by <see cref="ListTextAttribute"/>) of their referenced principal entity.
    /// </para>
    /// </summary>
    public PropertyDescriptionMode PropertyDescriptions { get; internal set; } = PropertyDescriptionMode.FkListText;

    /// <summary>
    /// Internal so that it cannot be modified in a way that breaks the caching assumptions
    /// that we make in CoalesceAuditLoggingBuilder.
    /// </summary>
    internal AuditConfiguration? AuditConfiguration { get; set; } = null;
}

/// <summary>
/// Controls how <see cref="AuditLogProperty.OldValueDescription"/> and <see cref="AuditLogProperty.NewValueDescription"/>
/// are populated by the framework.
/// </summary>
[Flags]
public enum PropertyDescriptionMode
{
    /// <summary>
    /// Property descriptions will not be populated by the framework.
    /// </summary>
    None = 0,

    // Core modes:
    /// <summary>
    /// Descriptions for foreign key properties will be populated from the List Text property of the principal entity.
    /// The list text of an entity can be customized by placing <see cref="ListTextAttribute"/> on a property,
    /// and defaults to a property named "Name" if one exists.
    /// </summary>
    FkListText = 1 << 0,

    // Future: untrack entities that we had to load because they weren't already in the context?
    //AutoUntrack = 1 << 20,
}

/// <summary>
/// Controls how <see cref="IAuditLog.Description"/> is populated by the framework.
/// </summary>
[Flags]
public enum DescriptionMode
{
    /// <summary>
    /// Description will not be populated by the framework.
    /// </summary>
    None = 0,

    // Core modes:
    /// <summary>
    /// The value of <see cref="IAuditLog.Description"/> will be populated from the List Text property of the changed entity.
    /// The list text of an entity can be customized by placing <see cref="ListTextAttribute"/> on a property,
    /// and defaults to a property named "Name" if one exists.
    /// </summary>
    ListText = 1 << 0,
}