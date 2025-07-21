using System;

namespace IntelliTect.Coalesce.DataAnnotations;

/// <summary>
/// Identifies a property as a many to many relationship and creates a secondary property on the view model for 
/// viewing the collection of target items directly.
/// </summary>
[System.AttributeUsage(AttributeTargets.Property)]
public class ManyToManyAttribute : System.Attribute
{
    public string CollectionName { get; }

    /// <summary>
    /// The name of the navigation property on the middle entity that points at the far side of the many-to-many relationship.
    /// </summary>
    public string? FarNavigationProperty { get; set; }

    public ManyToManyAttribute(string collectionName)
    {
        CollectionName = collectionName;
    }
}
