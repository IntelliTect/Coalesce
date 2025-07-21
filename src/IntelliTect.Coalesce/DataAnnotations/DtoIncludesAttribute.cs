using System;

namespace IntelliTect.Coalesce.DataAnnotations;

/// <summary>
/// Specify that this property is only used on specific views of the content.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class DtoIncludesAttribute : Attribute
{
    /// <summary>
    /// Comma-delimited list of content views this property should be included on.
    /// </summary>
    public string ContentViews { get; set; } = "";

    public DtoIncludesAttribute()
    {

    }

    public DtoIncludesAttribute( string contentViews )
    {
        ContentViews = contentViews;
    }
}
