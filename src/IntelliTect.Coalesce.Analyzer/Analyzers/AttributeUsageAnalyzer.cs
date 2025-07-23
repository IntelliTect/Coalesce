using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using IntelliTect.Coalesce.Core.Extensions;

namespace IntelliTect.Coalesce.Analyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class AttributeUsageAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor InvalidCoalesceUsageOnNestedTypesRule = new(
        id: "COALESCE0003",
        title: "Invalid CoalesceAttribute usage on nested data source or behavior",
        messageFormat: "Parameterless CoalesceAttribute has no effect on nested data sources or behaviors - they are automatically discovered by their containing type",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Nested data sources and behaviors are automatically associated with their containing type and do not need the [Coalesce] attribute.");

    public static readonly DiagnosticDescriptor InvalidCoalesceUsageRule = new(
        id: "COALESCE0004",
        title: "Invalid CoalesceAttribute usage on unsupported type",
        messageFormat: "CoalesceAttribute can only expose types that inherit from DbContext, implement IDataSource<T>, IBehaviors<T>, IClassDto<T>, or are marked with [Service] or [StandaloneEntity]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "The [Coalesce] attribute should only be applied to types that are supported by the Coalesce framework.");

    public static readonly DiagnosticDescriptor UnexposedSecondaryAttributeForTypesRule = new(
        id: "COALESCE0005",
        title: "Unexposed secondary attribute",
        messageFormat: "{0} attribute must be accompanied by [Coalesce] to be exposed by the framework",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Types marked with [Service] or [StandaloneEntity] require [Coalesce] attribute to be properly processed by the framework.");

    public static readonly DiagnosticDescriptor UnexposedSecondaryAttributeForMethodsRule = new(
        id: "COALESCE0006",
        title: "Unexposed secondary attribute",
        messageFormat: "{0} attribute must be accompanied by either [Coalesce] or [SemanticKernel] to be exposed by the framework",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Methods marked with [Execute] require either [Coalesce] or [SemanticKernel] attribute to be properly processed by the framework.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(InvalidCoalesceUsageOnNestedTypesRule, InvalidCoalesceUsageRule, UnexposedSecondaryAttributeForTypesRule, UnexposedSecondaryAttributeForMethodsRule);

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

        var coalesceAttr = typeSymbol.GetAttributeByName("IntelliTect.Coalesce.CoalesceAttribute");

        if (coalesceAttr is null)
        {
            // COALESCE0005: Check for StandaloneEntity or Service attributes without Coalesce
            var standaloneEntityAttr = typeSymbol.GetAttributeByName("IntelliTect.Coalesce.StandaloneEntityAttribute");
            var serviceAttr = typeSymbol.GetAttributeByName("IntelliTect.Coalesce.ServiceAttribute");

            var targetAttr = standaloneEntityAttr ?? serviceAttr;
            if (targetAttr?.GetLocation() is Location attrLocation)
            {
                var attributeName = targetAttr.AttributeClass?.Name?.Replace("Attribute", "") ?? "Unknown";
                var diagnostic = Diagnostic.Create(UnexposedSecondaryAttributeForTypesRule, attrLocation, $"[{attributeName}]");
                context.ReportDiagnostic(diagnostic);
            }
        }
        else if (!coalesceAttr.ConstructorArguments.Any() && !coalesceAttr.NamedArguments.Any())
        {
            // If there are no ctor args and no named args, it isn't being used to configure ClientTypeName

            // COALESCE0004: Check for CoalesceAttribute on types that aren't valid targets for CoalesceAttribute.
            if (!IsValidCoalesceType(typeSymbol))
            {
                if (coalesceAttr.GetLocation() is Location location)
                {
                    var diagnostic = Diagnostic.Create(InvalidCoalesceUsageRule, location);
                    context.ReportDiagnostic(diagnostic);
                }
            }

            // COALESCE0003: Check for invalid CoalesceAttribute usage on nested types
            if (typeSymbol.ContainingType is not null &&
                typeSymbol.InheritsFromOrImplements(
                    "IntelliTect.Coalesce.IBehaviors`1",
                    "IntelliTect.Coalesce.IDataSource`1"
            ))
            {
                if (coalesceAttr.GetLocation() is Location location)
                {
                    var diagnostic = Diagnostic.Create(InvalidCoalesceUsageOnNestedTypesRule, location);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static bool IsValidCoalesceType(INamedTypeSymbol typeSymbol)
    {
        // Check if inherits from DbContext or implements required interfaces
        if (typeSymbol.InheritsFromOrImplements(
            "Microsoft.EntityFrameworkCore.DbContext",
            "IntelliTect.Coalesce.IDataSource`1",
            "IntelliTect.Coalesce.IBehaviors`1",
            "IntelliTect.Coalesce.IClassDto`1"))
        {
            return true;
        }

        // Check if has required attributes
        return typeSymbol.GetAttributesByName(
            "IntelliTect.Coalesce.ServiceAttribute",
            "IntelliTect.Coalesce.StandaloneEntityAttribute").Any();
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var methodSymbol = (IMethodSymbol)context.Symbol;

        // Skip special methods like constructors, property getters/setters, etc.
        if (methodSymbol.MethodKind != MethodKind.Ordinary)
            return;

        var executeAttr = methodSymbol.GetAttributeByName("IntelliTect.Coalesce.DataAnnotations.ExecuteAttribute");

        if (executeAttr == null) return;

        // Check if the method has [Coalesce] or [SemanticKernel] attribute
        var hasCoalesceOrSemanticKernel = methodSymbol.GetAttributesByName(
            "IntelliTect.Coalesce.CoalesceAttribute",
            "IntelliTect.Coalesce.SemanticKernelAttribute").Any();

        if (hasCoalesceOrSemanticKernel) return;

        // Check if the containing type has [Service] attribute or [Coalesce] attribute (which makes Execute valid)
        if (methodSymbol.ContainingType != null)
        {
            var typeHasServiceOrCoalesce = methodSymbol.ContainingType.GetAttributesByName(
                "IntelliTect.Coalesce.ServiceAttribute",
                "IntelliTect.Coalesce.CoalesceAttribute").Any();

            if (typeHasServiceOrCoalesce) return;
        }

        var location = executeAttr.GetLocation();
        if (location is null) return;

        var diagnostic = Diagnostic.Create(UnexposedSecondaryAttributeForMethodsRule, location, "[Execute]");
        context.ReportDiagnostic(diagnostic);
    }
}
