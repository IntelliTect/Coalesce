
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using IntelliTect.Coalesce.Core.Extensions;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class Coalesce0001_InvalidPermissionLevel : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _Rule = new(
        id: "COA0001",
        title: "Invalid PermissionLevel usage on property security attributes",
        messageFormat: "Property-level {0} does not support PermissionLevel",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "PermissionLevel is only valid for class-level security attributes. For property-level security, use the Roles property to specify role-based access control.");

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

        var securityAttributes = propertySymbol.GetAttributesByName(
            "IntelliTect.Coalesce.DataAnnotations.ReadAttribute",
            "IntelliTect.Coalesce.DataAnnotations.EditAttribute");

        foreach (var attributeData in securityAttributes)
        {
            var attributeTypeName = attributeData.AttributeClass?.Name;

            // Check if PermissionLevel is being set
            if (HasPermissionLevelArgument(attributeData))
            {
                var location = attributeData.GetLocation() ?? propertySymbol.Locations[0];
                var diagnostic = Diagnostic.Create(_Rule, location, attributeTypeName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool HasPermissionLevelArgument(AttributeData attributeData)
    {
        // Check named arguments for PermissionLevel
        foreach (var namedArg in attributeData.NamedArguments)
        {
            if (namedArg.Key == "PermissionLevel")
                return true;
        }

        // Check constructor arguments - if any argument is SecurityPermissionLevels enum
        foreach (var constructorArg in attributeData.ConstructorArguments)
        {
            if (constructorArg.Type?.Name == "SecurityPermissionLevels")
                return true;
        }

        return false;
    }
}