using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using IntelliTect.Coalesce.Core.Extensions;

namespace IntelliTect.Coalesce.Analyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class Coalesce0014_InvalidNoAutoIncludeUsage : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _Rule = new(
        id: "COA0014",
        title: "Invalid NoAutoInclude usage on non-navigation property",
        messageFormat: "NoAutoInclude has no effect on non-navigation properties",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Unnecessary],
        description: "NoAutoInclude only affects navigation properties (reference navigation and collection navigation properties). It has no effect on plain data properties like strings, integers, or other value types.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeProperty, SymbolKind.Property);
    }

    private static void AnalyzeProperty(SymbolAnalysisContext context)
    {
        var propertySymbol = (IPropertySymbol)context.Symbol;

        // Only check ReadAttribute on properties
        var readAttribute = propertySymbol.GetAttributeByName("IntelliTect.Coalesce.DataAnnotations.ReadAttribute");
        if (readAttribute is null)
            return;

        // Check if NoAutoInclude is set to true
        if (!HasNoAutoIncludeSetToTrue(readAttribute))
            return;

        // Check if this is a navigation property
        if (IsNavigationProperty(propertySymbol))
            return;

        // Report diagnostic for non-navigation properties with NoAutoInclude = true
        var location = readAttribute.GetLocation() ?? propertySymbol.Locations[0];
        var diagnostic = Diagnostic.Create(_Rule, location);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool HasNoAutoIncludeSetToTrue(AttributeData attributeData)
    {
        // Check named arguments for NoAutoInclude = true
        foreach (var namedArg in attributeData.NamedArguments)
        {
            if (namedArg.Key == "NoAutoInclude")
            {
                if (namedArg.Value.Value is bool boolValue && boolValue)
                    return true;
            }
        }

        return false;
    }

    private static bool IsNavigationProperty(IPropertySymbol propertySymbol)
    {
        // A navigation property is one that:
        // 1. Has a [ForeignKey] attribute pointing to it (reference navigation), OR
        // 2. Has an [InverseProperty] attribute (collection navigation), OR
        // 3. Is a collection type AND has a property on the containing type that has [ForeignKey] pointing to this property

        var propertyType = propertySymbol.Type;

        // Check if the property has ForeignKey attribute
        var foreignKeyAttr = propertySymbol.GetAttributeByName("System.ComponentModel.DataAnnotations.Schema.ForeignKeyAttribute");
        if (foreignKeyAttr is not null)
        {
            // This property is decorated with [ForeignKey], which means it's a reference navigation
            return true;
        }

        // Check if the property has InverseProperty attribute
        var inversePropertyAttr = propertySymbol.GetAttributeByName("System.ComponentModel.DataAnnotations.Schema.InversePropertyAttribute");
        if (inversePropertyAttr is not null)
        {
            // This property is decorated with [InverseProperty], which means it's a collection navigation
            return true;
        }

        // Note: We don't check if another property references this as a foreign key,
        // because that would make the OTHER property a navigation, not this one.
        // For example, if Related has [ForeignKey(nameof(RelatedId))], then Related is
        // the navigation property, not RelatedId.

        // Check if the property type is a complex type (not a primitive or built-in type)
        // This is a heuristic to detect potential navigation properties that aren't explicitly marked
        if (propertyType is INamedTypeSymbol namedType)
        {
            // Check if it's a collection type (ICollection<T>, IEnumerable<T>, etc.)
            if (IsCollectionType(namedType))
            {
                // It's a collection - could be a collection navigation
                // Check if there's a generic argument
                if (namedType.TypeArguments.Length > 0 && namedType.TypeArguments[0] is INamedTypeSymbol elementType)
                {
                    // Check if the element type is a complex type (has properties)
                    if (IsComplexType(elementType))
                    {
                        return true;
                    }
                }
            }
            // Check if the property type is a complex type (not string, not primitive, not built-in value type)
            else if (IsComplexType(namedType) && !namedType.Equals(propertySymbol.ContainingType, SymbolEqualityComparer.Default))
            {
                // It's a complex type - likely a reference navigation
                return true;
            }
        }

        return false;
    }

    private static bool IsCollectionType(INamedTypeSymbol typeSymbol)
    {
        // Check if it implements IEnumerable<T> or ICollection<T>
        return typeSymbol.InheritsFromOrImplements(
            "System.Collections.Generic.IEnumerable`1",
            "System.Collections.Generic.ICollection`1",
            "System.Collections.Generic.IList`1"
        );
    }

    private static bool IsComplexType(INamedTypeSymbol typeSymbol)
    {
        // Check if it's not a primitive type, not string, and not a built-in value type
        if (typeSymbol.SpecialType != SpecialType.None)
        {
            // It's a special type like string, int, DateTime, etc.
            return false;
        }

        // Check if it's an enum
        if (typeSymbol.TypeKind == TypeKind.Enum)
        {
            return false;
        }

        // Check if it's a struct (value type) from System namespace (like Guid, DateTime, etc.)
        if (typeSymbol.TypeKind == TypeKind.Struct && typeSymbol.ContainingNamespace?.ToString().StartsWith("System") == true)
        {
            return false;
        }

        // It's a complex reference type or a user-defined struct
        return true;
    }
}
