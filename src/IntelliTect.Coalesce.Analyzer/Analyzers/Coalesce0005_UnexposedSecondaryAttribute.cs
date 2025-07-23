using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace IntelliTect.Coalesce.Analyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class Coalesce0005_UnexposedSecondaryAttribute : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor RuleForTypes = new(
        id: "COALESCE0005",
        title: "Unexposed secondary attribute",
        messageFormat: "{0} attribute requires [Coalesce] attribute to be exposed by the framework",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Types marked with [Service] or [StandaloneEntity] require [Coalesce] attribute to be properly processed by the framework.");

    public static readonly DiagnosticDescriptor RuleForMethods = new(
        id: "COALESCE0006",
        title: "Unexposed secondary attribute",
        messageFormat: "{0} attribute requires either [Coalesce] or [SemanticKernel] attribute to be exposed by the framework",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Methods marked with [Execute] require either [Coalesce] or [SemanticKernel] attribute to be properly processed by the framework.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleForTypes, RuleForMethods);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var typeSymbol = (INamedTypeSymbol)context.Symbol;

        // Check for StandaloneEntity or Service attributes
        var standaloneEntityAttr = typeSymbol.GetAttributes().FirstOrDefault(attr =>
            attr.AttributeClass?.ContainingNamespace?.ToDisplayString() == "IntelliTect.Coalesce" &&
            attr.AttributeClass?.Name == "StandaloneEntityAttribute");

        var serviceAttr = typeSymbol.GetAttributes().FirstOrDefault(attr =>
            attr.AttributeClass?.ContainingNamespace?.ToDisplayString() == "IntelliTect.Coalesce" &&
            attr.AttributeClass?.Name == "ServiceAttribute");

        var targetAttr = standaloneEntityAttr ?? serviceAttr;

        if (targetAttr != null)
        {
            var hasCoalesce = typeSymbol.GetAttributes().Any(attr =>
                attr.AttributeClass?.ContainingNamespace?.ToDisplayString() == "IntelliTect.Coalesce" &&
                attr.AttributeClass?.Name == "CoalesceAttribute");

            if (!hasCoalesce)
            {
                var location = targetAttr.ApplicationSyntaxReference?.GetSyntax().GetLocation();
                if (location is not null)
                {
                    var attributeName = targetAttr.AttributeClass?.Name?.Replace("Attribute", "") ?? "Unknown";
                    var diagnostic = Diagnostic.Create(RuleForTypes, location, $"[{attributeName}]");
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var methodSymbol = (IMethodSymbol)context.Symbol;

        // Skip special methods like constructors, property getters/setters, etc.
        if (methodSymbol.MethodKind != MethodKind.Ordinary)
            return;

        var executeAttr = methodSymbol.GetAttributes().FirstOrDefault(attr =>
            attr.AttributeClass?.ContainingNamespace?.ToDisplayString() == "IntelliTect.Coalesce.DataAnnotations" &&
            attr.AttributeClass?.Name == "ExecuteAttribute");

        if (executeAttr == null) return;

        // Check if the method has [Coalesce] or [SemanticKernel] attribute
        var hasCoalesceOrSemanticKernel = methodSymbol.GetAttributes().Any(attr =>
            attr.AttributeClass?.ContainingNamespace?.ToDisplayString() == "IntelliTect.Coalesce" &&
            attr.AttributeClass?.Name is "CoalesceAttribute" or "SemanticKernelAttribute");

        if (hasCoalesceOrSemanticKernel) return;

        // Check if the containing type has [Service] attribute or [Coalesce] attribute (which makes Execute valid)
        if (methodSymbol.ContainingType != null)
        {
            var typeHasServiceOrCoalesce = methodSymbol.ContainingType.GetAttributes().Any(attr =>
                attr.AttributeClass?.ContainingNamespace?.ToDisplayString() == "IntelliTect.Coalesce" &&
                attr.AttributeClass?.Name is "ServiceAttribute" or "CoalesceAttribute");

            if (typeHasServiceOrCoalesce) return;
        }

        var location = executeAttr.ApplicationSyntaxReference?.GetSyntax().GetLocation();
        if (location is null) return;

        var diagnostic = Diagnostic.Create(RuleForMethods, location, "[Execute]");
        context.ReportDiagnostic(diagnostic);
    }
}
