using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Linq;

namespace IntelliTect.Coalesce.Analyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class SecurityBypassAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor SecurityBypassRule = new(
        id: "COA2001",
        title: "Authorization logic in data source without default data source",
        messageFormat: "Data source appears to contains authorization logic but '{0}' has no explicit default data source. Security checks may be bypassed by a client who uses the type's default data source.",
        category: "Security",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Data sources that perform authorization checks should ensure their served type has a default data source to prevent security bypasses. Without a default data source, clients can directly access the served type without the authorization logic.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(SecurityBypassRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeOperation, OperationKind.MethodBody);
    }

    private static void AnalyzeOperation(OperationAnalysisContext context)
    {
        var methodBody = (IMethodBodyOperation)context.Operation;
        var method = methodBody.SemanticModel?.GetDeclaredSymbol(methodBody.Syntax) as IMethodSymbol;
        if (method?.ContainingType is not INamedTypeSymbol dataSourceType)
            return;

        // Check if this is a data source type
        if (!IsDataSourceType(dataSourceType, out var servedType) || servedType is null)
            return;

        // Skip if the served type is a standalone entity (no implicit fallback risk)
        if (servedType.GetAttributeByName("IntelliTect.Coalesce.StandaloneEntityAttribute") is not null)
            return;

        // Skip if this is already a default data source
        if (dataSourceType.GetAttributeByName("IntelliTect.Coalesce.DefaultDataSourceAttribute") is not null)
            return;

        // Get ClaimsPrincipal type from the compilation
        var claimsPrincipalType = context.Compilation.GetTypeByMetadataName("System.Security.Claims.ClaimsPrincipal");
        if (claimsPrincipalType is null)
            return;

        // Check if the method contains authorization logic
        if (!ContainsClaimsPrincipalUsage(methodBody, claimsPrincipalType))
            return;

        // Check if the served type has a default data source
        if (HasDefaultDataSource(servedType))
            return;

        // Report the diagnostic at the class location
        var classLocation = dataSourceType.Locations.FirstOrDefault();
        var diagnostic = Diagnostic.Create(SecurityBypassRule, classLocation, servedType.Name);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool ContainsClaimsPrincipalUsage(IMethodBodyOperation methodBody, INamedTypeSymbol claimsPrincipalType)
    {
        foreach (var descendant in methodBody.Descendants())
        {
            // Check for member reference operations on ClaimsPrincipal
            if (descendant is IMemberReferenceOperation memberRef &&
                SymbolEqualityComparer.Default.Equals(memberRef.Instance?.Type, claimsPrincipalType))
            {
                return true;
            }

            // Check for invocation operations on ClaimsPrincipal
            if (descendant is IInvocationOperation invocation)
            {
                // Direct method calls on ClaimsPrincipal
                if (invocation.Instance is not null &&
                    SymbolEqualityComparer.Default.Equals(invocation.Instance.Type, claimsPrincipalType))
                {
                    return true;
                }

                // Extension method calls on ClaimsPrincipal
                if (invocation.TargetMethod.IsExtensionMethod &&
                    invocation.TargetMethod.Parameters.Length > 0 &&
                    SymbolEqualityComparer.Default.Equals(invocation.TargetMethod.Parameters[0].Type, claimsPrincipalType))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool IsDataSourceType(INamedTypeSymbol typeSymbol, out INamedTypeSymbol? servedType)
    {
        servedType = null;

        // Check if the type implements IDataSource<T>
        foreach (var interfaceType in typeSymbol.AllInterfaces)
        {
            if (interfaceType.IsGenericType &&
                interfaceType.ConstructedFrom.ToDisplayString() == "IntelliTect.Coalesce.IDataSource<T>")
            {
                servedType = interfaceType.TypeArguments[0] as INamedTypeSymbol;
                return servedType is not null;
            }
        }

        return false;
    }

    private static bool HasDefaultDataSource(INamedTypeSymbol servedType)
    {
        // Look for nested types marked with [DefaultDataSource]
        foreach (var nestedType in servedType.GetTypeMembers())
        {
            if (nestedType.GetAttributeByName("IntelliTect.Coalesce.DefaultDataSourceAttribute") is not null)
                return true;
        }

        // TODO: In a more comprehensive analysis, we could also check for 
        // external data sources that implement IDataSource<TServedType> and are marked with [DefaultDataSource],
        // but that would require analyzing the entire compilation which could be expensive.
        // For now, we focus on the common pattern of nested default data sources.

        return false;
    }
}
