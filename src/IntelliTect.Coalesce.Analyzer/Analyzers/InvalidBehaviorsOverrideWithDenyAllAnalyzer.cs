using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using IntelliTect.Coalesce.Core.Extensions;
using System.Linq;

namespace IntelliTect.Coalesce.Analyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class InvalidBehaviorsOverrideWithDenyAllAnalyzer : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _NestedBehaviorsWithAllDenyAllRule = new(
        id: "COA0009",
        title: "Nested behaviors class defined for model with all CRUD operations denied",
        messageFormat: "Behaviors class '{0}' is defined for model '{1}' which has Create, Edit, and Delete all set to DenyAll",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "A nested behaviors class should not be defined when the containing model has Create, Edit, and Delete attributes all set to DenyAll, as the behaviors will never be used.");

    private static readonly DiagnosticDescriptor _SaveMethodsWithCreateEditDenyAllRule = new(
        id: "COA0010",
        title: "Save-related methods overridden in behaviors for model with Create and Edit denied",
        messageFormat: "Method '{0}' is overridden in behaviors for model '{1}' which has Create and Edit both set to DenyAll",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Save-related methods should not be overridden when the containing model has Create and Edit attributes both set to DenyAll, as these methods will never be called.");

    private static readonly DiagnosticDescriptor _DeleteMethodsWithDeleteDenyAllRule = new(
        id: "COA0011",
        title: "Delete-related methods overridden in behaviors for model with Delete denied",
        messageFormat: "Method '{0}' is overridden in behaviors for model '{1}' which has Delete set to DenyAll",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true,
        description: "Delete-related methods should not be overridden when the containing model has Delete attribute set to DenyAll, as these methods will never be called.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(_NestedBehaviorsWithAllDenyAllRule, _SaveMethodsWithCreateEditDenyAllRule, _DeleteMethodsWithDeleteDenyAllRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeNamedType, SymbolKind.NamedType);
        context.RegisterSymbolAction(AnalyzeMethod, SymbolKind.Method);
    }

    private static readonly string[] SaveRelatedMethods =
    [
        "ExecuteSaveAsync",
        "DetermineSaveKindAsync",
        "FetchObjectAfterSaveAsync",
        "BeforeSave",
        "BeforeSaveAsync",
        "SaveAsync",
        "ExecuteSafeAsync",
        "AfterSaveAsync"
    ];

    private static readonly string[] DeleteRelatedMethods =
    [
        "DeleteAsync",
        "BeforeDelete",
        "BeforeDeleteAsync",
        "AfterDeleteAsync",
        "ExecuteDeleteAsync"
    ];

    private static void AnalyzeNamedType(SymbolAnalysisContext context)
    {
        var typeSymbol = (INamedTypeSymbol)context.Symbol;

        // Only analyze nested types that implement IBehaviors<T>
        if (typeSymbol.ContainingType is null ||
            !typeSymbol.InheritsFromOrImplements("IntelliTect.Coalesce.IBehaviors`1"))
            return;

        var containingType = typeSymbol.ContainingType;

        // Check if the containing type has Create, Edit, and Delete all set to DenyAll
        if (HasAllCrudOperationsDenied(containingType))
        {
            var location = typeSymbol.Locations.FirstOrDefault() ?? typeSymbol.ContainingType.Locations.FirstOrDefault();
            if (location != null)
            {
                var diagnostic = Diagnostic.Create(_NestedBehaviorsWithAllDenyAllRule, location, typeSymbol.Name, containingType.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static void AnalyzeMethod(SymbolAnalysisContext context)
    {
        var methodSymbol = (IMethodSymbol)context.Symbol;

        // Only analyze methods in classes that implement IBehaviors<T>
        if (!methodSymbol.ContainingType.InheritsFromOrImplements("IntelliTect.Coalesce.IBehaviors`1"))
            return;

        // Only analyze overridden or implemented methods (not regular methods)
        if (methodSymbol.MethodKind != MethodKind.Ordinary ||
            (methodSymbol.OverriddenMethod == null && !methodSymbol.ExplicitInterfaceImplementations.Any()))
            return;

        var behaviorType = methodSymbol.ContainingType;
        var modelType = GetModelTypeFromBehaviors(behaviorType);
        if (modelType == null)
            return;

        var methodName = methodSymbol.Name;

        // Check save-related methods
        if (SaveRelatedMethods.Contains(methodName))
        {
            if (HasCreateAndEditDenied(modelType))
            {
                var location = methodSymbol.Locations.FirstOrDefault();
                if (location != null)
                {
                    var diagnostic = Diagnostic.Create(_SaveMethodsWithCreateEditDenyAllRule, location, methodName, modelType.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
        // Check delete-related methods
        else if (DeleteRelatedMethods.Contains(methodName))
        {
            if (HasDeleteDenied(modelType))
            {
                var location = methodSymbol.Locations.FirstOrDefault();
                if (location != null)
                {
                    var diagnostic = Diagnostic.Create(_DeleteMethodsWithDeleteDenyAllRule, location, methodName, modelType.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private static INamedTypeSymbol? GetModelTypeFromBehaviors(INamedTypeSymbol behaviorsType)
    {
        // For nested behaviors classes, the model type is the containing type
        if (behaviorsType.ContainingType != null)
        {
            return behaviorsType.ContainingType;
        }

        // For non-nested behaviors classes, we need to extract T from IBehaviors<T>
        var behaviorInterface = behaviorsType.AllInterfaces
            .FirstOrDefault(i => i.Name == "IBehaviors" && i.TypeArguments.Length == 1);

        return behaviorInterface?.TypeArguments[0] as INamedTypeSymbol;
    }

    private static bool HasAllCrudOperationsDenied(INamedTypeSymbol typeSymbol)
    {
        return HasCreateDenied(typeSymbol) && HasEditDenied(typeSymbol) && HasDeleteDenied(typeSymbol);
    }

    private static bool HasCreateAndEditDenied(INamedTypeSymbol typeSymbol)
    {
        return HasCreateDenied(typeSymbol) && HasEditDenied(typeSymbol);
    }

    private static bool HasCreateDenied(INamedTypeSymbol typeSymbol)
    {
        var createAttr = typeSymbol.GetAttributeByName("IntelliTect.Coalesce.DataAnnotations.CreateAttribute");
        return IsAttributeDenyAll(createAttr);
    }

    private static bool HasEditDenied(INamedTypeSymbol typeSymbol)
    {
        var editAttr = typeSymbol.GetAttributeByName("IntelliTect.Coalesce.DataAnnotations.EditAttribute");
        return IsAttributeDenyAll(editAttr);
    }

    private static bool HasDeleteDenied(INamedTypeSymbol typeSymbol)
    {
        var deleteAttr = typeSymbol.GetAttributeByName("IntelliTect.Coalesce.DataAnnotations.DeleteAttribute");
        return IsAttributeDenyAll(deleteAttr);
    }

    private static bool IsAttributeDenyAll(AttributeData? attributeData)
    {
        if (attributeData == null)
            return false;

        // Check named arguments for PermissionLevel = DenyAll
        foreach (var namedArg in attributeData.NamedArguments)
        {
            if (namedArg.Key == "PermissionLevel" &&
                namedArg.Value.Value is int permissionLevel &&
                permissionLevel == 3) // DenyAll = 3
            {
                return true;
            }
        }

        // Check constructor arguments - if any argument is SecurityPermissionLevels.DenyAll
        foreach (var constructorArg in attributeData.ConstructorArguments)
        {
            if (constructorArg.Type?.Name == "SecurityPermissionLevels" &&
                constructorArg.Value is int value &&
                value == 3) // DenyAll = 3
            {
                return true;
            }
        }

        return false;
    }
}
