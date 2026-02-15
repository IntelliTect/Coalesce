using System;

namespace IntelliTect.Coalesce;

/// <summary>
/// The targeted class or member should be exposed by Coalesce.
/// Different types will be exposed in different ways. See documentation for details.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Field)]
public sealed class CoalesceAttribute : Attribute
{
    /// <summary>
    /// When placed on a type, overrides the name of the type used in client-side code.
    /// </summary>
    public string? ClientTypeName { get; set; }
}
