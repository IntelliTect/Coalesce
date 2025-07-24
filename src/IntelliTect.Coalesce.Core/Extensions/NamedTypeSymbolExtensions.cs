using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace IntelliTect.Coalesce.Core.Extensions;

/// <summary>
/// Extension methods for <see cref="INamedTypeSymbol"/>.
/// </summary>
public static class NamedTypeSymbolExtensions
{
    /// <summary>
    /// Determines whether the type inherits from any of the specified base classes or implements any of the specified interfaces.
    /// </summary>
    /// <param name="typeSymbol">The type symbol to check.</param>
    /// <param name="typesToCheck">
    /// An array of fully qualified type names to check against. 
    /// Each should be in the format "Namespace.TypeName" (e.g., "Microsoft.EntityFrameworkCore.DbContext").
    /// The method checks both base classes (inheritance chain) and implemented interfaces.
    /// </param>
    /// <returns>
    /// <c>true</c> if the type inherits from any of the specified base classes or implements any of the specified interfaces; 
    /// otherwise, <c>false</c>.
    /// </returns>
    public static bool InheritsFromOrImplements(this INamedTypeSymbol typeSymbol, params ReadOnlySpan<string> typesToCheck)
    {
        if (typesToCheck.Length == 0)
            return false;

        // Check if the type itself matches
        if (TypeMatches(typeSymbol, typesToCheck))
        {
            return true;
        }

        // Check inheritance chain
        var current = typeSymbol.BaseType;
        while (current is not null)
        {
            if (TypeMatches(current, typesToCheck))
            {
                return true;
            }
            current = current.BaseType;
        }

        // Check implemented interfaces
        var interfaces = typeSymbol.AllInterfaces;
        for (int i = 0; i < interfaces.Length; i++)
        {
            if (TypeMatches(interfaces[i], typesToCheck))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets all attributes that match any of the specified fully qualified type names.
    /// </summary>
    /// <param name="symbol">The symbol to check for attributes.</param>
    /// <param name="attributeTypeNames">
    /// An array of fully qualified attribute type names to match against.
    /// Each should be in the format "Namespace.TypeName" (e.g., "IntelliTect.Coalesce.CoalesceAttribute").
    /// </param>
    /// <returns>A collection of matching attributes.</returns>
    public static IEnumerable<AttributeData> GetAttributesByName(this ISymbol symbol, params IReadOnlyCollection<string> attributeTypeNames)
    {
        if (attributeTypeNames == null || attributeTypeNames.Count == 0)
            yield break;

        var attributes = symbol.GetAttributes();
        for (int i = 0; i < attributes.Length; i++)
        {
            var attr = attributes[i];
            if (attr.AttributeClass is { } attrClass && TypeMatches(attrClass, [.. attributeTypeNames]))
            {
                yield return attr;
            }
        }
    }

    /// <summary>
    /// Gets the first attribute that matches any of the specified fully qualified type names.
    /// </summary>
    /// <param name="symbol">The symbol to check for attributes.</param>
    /// <param name="attributeTypeNames">
    /// An array of fully qualified attribute type names to match against.
    /// Each should be in the format "Namespace.TypeName" (e.g., "IntelliTect.Coalesce.CoalesceAttribute").
    /// </param>
    /// <returns>The first matching attribute, or null if none found.</returns>
    public static AttributeData? GetAttributeByName(this ISymbol symbol, params ReadOnlySpan<string> attributeTypeNames)
    {
        if (attributeTypeNames.Length == 0)
            return null;

        var attributes = symbol.GetAttributes();
        for (int i = 0; i < attributes.Length; i++)
        {
            var attr = attributes[i];
            if (attr.AttributeClass is { } attrClass && TypeMatches(attrClass, attributeTypeNames))
            {
                return attr;
            }
        }

        return null;
    }

    /// <summary>
    /// Checks if a type matches any of the specified type names.
    /// </summary>
    /// <param name="typeSymbol">The type symbol to check.</param>
    /// <param name="typesToCheck">The array of type names to check against.</param>
    /// <returns>True if the type matches any of the specified names.</returns>
    private static bool TypeMatches(INamedTypeSymbol typeSymbol, ReadOnlySpan<string> typesToCheck)
    {
        if (typeSymbol?.ContainingNamespace == null)
            return false;

        var typeName = typeSymbol.MetadataName.AsSpan();

        for (int i = 0; i < typesToCheck.Length; i++)
        {
            var typeToCheck = typesToCheck[i].AsSpan();

            // Quick check: if the type name doesn't end with our MetadataName, skip
            if (!typeToCheck.EndsWith(typeName, StringComparison.Ordinal))
                continue;

            // If it's just the type name without namespace
            if (typeToCheck.SequenceEqual(typeName))
                return true;

            // Check if it matches namespace.typename pattern
            if (typeToCheck.Length > typeName.Length + 1 &&
                typeToCheck[typeToCheck.Length - typeName.Length - 1] == '.' &&
                NamespaceMatches(typeSymbol.ContainingNamespace, typeToCheck, typeToCheck.Length - typeName.Length - 1))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if a namespace matches the beginning portion of a full type name.
    /// </summary>
    /// <param name="namespaceSymbol">The namespace symbol to check.</param>
    /// <param name="fullTypeName">The full type name span to match against.</param>
    /// <param name="namespaceEndIndex">The index where the namespace portion ends in fullTypeName.</param>
    /// <returns>True if the namespace matches.</returns>
    private static bool NamespaceMatches(INamespaceSymbol namespaceSymbol, ReadOnlySpan<char> fullTypeName, int namespaceEndIndex)
    {
        if (namespaceSymbol.IsGlobalNamespace)
            return namespaceEndIndex == 0;

        // Work backwards through the string, matching namespace parts from innermost to outermost
        int currentIndex = namespaceEndIndex;
        var current = namespaceSymbol;

        while (current != null && !current.IsGlobalNamespace)
        {
            var currentName = current.Name.AsSpan();
            var nameLength = currentName.Length;

            // Check if we have enough characters left
            if (currentIndex < nameLength)
                return false;

            // Move back by the length of this part
            currentIndex -= nameLength;

            // Check if the part matches using span comparison
            if (!fullTypeName.Slice(currentIndex, nameLength).SequenceEqual(currentName))
                return false;

            // Move to the containing namespace
            current = current.ContainingNamespace;

            // If there's still a containing namespace (not global), there should be a dot before this part
            if (current != null && !current.IsGlobalNamespace)
            {
                if (currentIndex == 0 || fullTypeName[currentIndex - 1] != '.')
                    return false;
                currentIndex--; // Move past the dot
            }
        }

        // We should have consumed the entire namespace portion
        return currentIndex == 0;
    }
}
