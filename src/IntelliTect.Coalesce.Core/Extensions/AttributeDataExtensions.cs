using Microsoft.CodeAnalysis;

namespace IntelliTect.Coalesce.Core.Extensions;

/// <summary>
/// Extension methods for <see cref="AttributeData"/>.
/// </summary>
public static class AttributeDataExtensions
{
    /// <summary>
    /// Gets the location of the attribute in the source code.
    /// </summary>
    /// <param name="attributeData">The attribute data to get the location for.</param>
    /// <returns>The location of the attribute, or null if not available.</returns>
    public static Location? GetLocation(this AttributeData attributeData)
    {
        return attributeData.ApplicationSyntaxReference?.GetSyntax().GetLocation();
    }
}
