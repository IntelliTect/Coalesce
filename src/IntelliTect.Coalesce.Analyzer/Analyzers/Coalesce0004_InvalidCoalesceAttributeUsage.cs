using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace IntelliTect.Coalesce.Analyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class Coalesce0004_InvalidCoalesceAttributeUsage : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _Rule = new(
        id: "COALESCE0004",
        title: "Invalid CoalesceAttribute usage on unsupported type",
        messageFormat: "CoalesceAttribute only functions on types that inherit from DbContext, implement IDataSource<T>, IBehaviors<T>, IClassDto<T>, or are marked with [Service] or [StandaloneEntity]",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "The [Coalesce] attribute should only be applied to types that are supported by the Coalesce framework.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
    }

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var typeSymbol = (INamedTypeSymbol)context.Symbol;

        var coalesceAttr = typeSymbol.GetAttributes().FirstOrDefault(attr =>
            attr.AttributeClass?.Name == "CoalesceAttribute" &&
            attr.AttributeClass?.ContainingNamespace?.ToDisplayString() == "IntelliTect.Coalesce");

        if (coalesceAttr is null) return;

        // Check if the type is valid for CoalesceAttribute
        if (IsValidCoalesceType(typeSymbol)) return;

        var location = coalesceAttr.ApplicationSyntaxReference?.GetSyntax().GetLocation();
        if (location is null) return;

        if (coalesceAttr.ConstructorArguments.Any() || coalesceAttr.NamedArguments.Any())
        {
            // If there are parameters, the attribute is being used to customize something (i.e. ClientTypeName)
            return;
        }

        var diagnostic = Diagnostic.Create(_Rule, location);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsValidCoalesceType(INamedTypeSymbol typeSymbol)
    {
        // Check if inherits from DbContext
        var current = typeSymbol.BaseType;
        while (current is not null)
        {
            if (current.Name == "DbContext" &&
                current.ContainingNamespace?.ToDisplayString() == "Microsoft.EntityFrameworkCore")
            {
                return true;
            }
            current = current.BaseType;
        }

        // Check if implements required interfaces
        if (typeSymbol.AllInterfaces.Any(i =>
            i.ContainingNamespace?.ToDisplayString() == "IntelliTect.Coalesce" &&
            i.Name is "IDataSource" or "IBehaviors" or "IClassDto"))
        {
            return true;
        }

        // Check if has required attributes
        return typeSymbol.GetAttributes().Any(attr =>
            attr.AttributeClass?.ContainingNamespace?.ToDisplayString() == "IntelliTect.Coalesce" &&
            attr.AttributeClass?.Name is "ServiceAttribute" or "StandaloneEntityAttribute");
    }
}
