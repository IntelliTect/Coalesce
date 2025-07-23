using IntelliTect.Coalesce.Core.Extensions;

namespace IntelliTect.Coalesce.Analyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class Coalesce0002_InvalidInjectAttributeUsage : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _Rule = new(
        id: "COALESCE0002",
        title: "Invalid InjectAttribute usage",
        messageFormat: "InjectAttribute is only valid on parameters of Coalesce client methods - either methods marked with [Coalesce] or [SemanticKernel] attributes, or methods on interfaces marked with [Service] attribute",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeParameter, SymbolKind.Parameter);
    }

    private static void AnalyzeParameter(SymbolAnalysisContext context)
    {
        var parameterSymbol = (IParameterSymbol)context.Symbol;

        var injectAttr = parameterSymbol.GetAttributeByName("IntelliTect.Coalesce.DataAnnotations.InjectAttribute");
        if (injectAttr is null) return;

        if (parameterSymbol.ContainingSymbol is not IMethodSymbol containingMethod)
            return;

        // Check if the method is a valid Coalesce client method
        if (!IsValidCoalesceMethod(containingMethod))
        {
            context.ReportDiagnostic(Diagnostic.Create(_Rule, injectAttr.GetLocation()));
        }
    }

    private static bool IsValidCoalesceMethod(IMethodSymbol methodSymbol)
    {
        // Check if method has [Coalesce] or [SemanticKernel] attribute
        if (HasExposedAttribute(methodSymbol))
            return true;

        // Check if method is on an interface marked with [Service]
        var containingType = methodSymbol.ContainingType;
        if (containingType != null && containingType.TypeKind == TypeKind.Interface && HasServiceAttribute(containingType))
            return true;

        // Check if method implements an interface method that would be valid
        if (containingType != null && containingType.TypeKind == TypeKind.Class)
        {
            foreach (var interfaceType in containingType.AllInterfaces)
            {
                var interfaceMethod = interfaceType.GetMembers(methodSymbol.Name)
                    .OfType<IMethodSymbol>()
                    .FirstOrDefault(m => methodSymbol.ContainingType.FindImplementationForInterfaceMember(m)?.Equals(methodSymbol, SymbolEqualityComparer.Default) == true);

                if (interfaceMethod != null && IsValidCoalesceMethod(interfaceMethod))
                    return true;
            }
        }

        return false;
    }

    private static bool HasExposedAttribute(IMethodSymbol methodSymbol)
    {
        return methodSymbol.GetAttributesByName(
            "IntelliTect.Coalesce.CoalesceAttribute",
            "IntelliTect.Coalesce.SemanticKernelAttribute").Any();
    }

    private static bool HasServiceAttribute(ITypeSymbol typeSymbol)
    {
        return typeSymbol.GetAttributeByName("IntelliTect.Coalesce.ServiceAttribute") != null;
    }
}
