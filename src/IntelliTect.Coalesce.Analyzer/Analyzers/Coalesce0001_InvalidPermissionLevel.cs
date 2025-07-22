
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class Coalesce0001_InvalidPermissionLevel : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _Rule = new(
        id: "COALESCE0001",
        title: "Invalid PermissionLevel usage on property security attributes",
        messageFormat: "Property-level {0} attributes should not set PermissionLevel",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "PermissionLevel is only valid for class-level security attributes. For property-level security, use the Roles property to specify role-based access control.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_Rule);

    public override void Initialize(AnalysisContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeProperty, SymbolKind.Property);
    }

    private static void AnalyzeProperty(SymbolAnalysisContext context)
    {
        var propertySymbol = (IPropertySymbol)context.Symbol;

        foreach (var attributeData in propertySymbol.GetAttributes())
        {
            var attributeType = attributeData.AttributeClass;
            var attributeTypeName = attributeType?.Name;
            var attributeNamespace = attributeType?.ContainingNamespace?.ToDisplayString();

            if (attributeNamespace != "IntelliTect.Coalesce.DataAnnotations")
                continue;

            if (attributeTypeName != "ReadAttribute" && attributeTypeName != "EditAttribute")
                continue;

            // Check if PermissionLevel is being set
            if (HasPermissionLevelArgument(attributeData))
            {
                var attributeName = attributeTypeName.Replace("Attribute", "");
                var diagnostic = Diagnostic.Create(_Rule, propertySymbol.Locations[0], attributeName);
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