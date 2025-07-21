using System;

namespace IntelliTect.Coalesce.DataAnnotations;

/// <summary>
/// Allows this property to be hidden on the list or editor or both.
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Method)]
public class HiddenAttribute : System.Attribute
{
    [Flags]
    public enum Areas
    {
        None = 0,
        List = 1 << 0,
        Edit = 1 << 1,
        All = List | Edit,
    }

    public Areas Area { get; set; }

    public HiddenAttribute(Areas area = Areas.All)
    {
        this.Area = area;
    }
}
