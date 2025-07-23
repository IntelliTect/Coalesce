namespace IntelliTect.Coalesce.Analyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class Coalesce0003_InvalidCoalesceAttributeOnNestedTypes : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _Rule = new(
        id: "COALESCE0003",
        title: "Invalid CoalesceAttribute usage on nested data source or behavior",
        messageFormat: "Parameterless CoalesceAttribute has no effect on nested data sources or behaviors - they are automatically discovered by their containing type",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Nested data sources and behaviors are automatically associated with their containing type and do not need the [Coalesce] attribute.");

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

        // Check if this is a nested type
        if (typeSymbol.ContainingType == null)
            return;

        var coalesceAttr = typeSymbol.GetAttributes().FirstOrDefault(attr =>
            attr.AttributeClass?.Name == "CoalesceAttribute" &&
            attr.AttributeClass?.ContainingNamespace?.ToDisplayString() == "IntelliTect.Coalesce");

        if (coalesceAttr is null) return;

        // Check if this is a data source or behavior
        if (!typeSymbol.AllInterfaces.Any(i =>
            i.Name is "IBehaviors" or "IDataSource" &&
            i.ContainingNamespace?.ToDisplayString() == "IntelliTect.Coalesce"
        )) return;

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
}
