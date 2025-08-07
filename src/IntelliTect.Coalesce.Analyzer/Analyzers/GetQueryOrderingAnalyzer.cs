using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Linq;
using IntelliTect.Coalesce.Core.Extensions;

namespace IntelliTect.Coalesce.Analyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class GetQueryOrderingAnalyzer : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor GetQueryOrderingRule = new(
        id: "COA0012",
        title: "Ordering operations on GetQuery result will be overridden",
        messageFormat: "'{0}' applied to a query returned from GetQuery will be overridden by client-specified or default sorting for the model. Apply sorting with [DefaultOrderBy] attributes on model properties, or by overriding ApplyListDefaultSorting or ApplyListSorting.",
        category: "Usage",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: "Ordering operations (OrderBy, OrderByDescending, ThenBy, ThenByDescending) applied to queries returned from GetQuery methods may be overridden by client-specified sorting. Consider moving the ordering logic to ApplyListDefaultSorting or using [DefaultOrderBy] attributes on model properties.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(GetQueryOrderingRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Invocation);
    }

    private static void AnalyzeInvocation(OperationAnalysisContext context)
    {
        var invocationOperation = (IInvocationOperation)context.Operation;
        var method = invocationOperation.TargetMethod;

        // Check if this is an ordering method call
        if (!IsOrderingMethod(method))
            return;

        // Check if we're in a GetQuery method
        if (!IsInGetQueryMethod(invocationOperation, context.Compilation))
            return;

        // Check if the ordering is being applied to a result of GetQuery call or a subquery
        if (IsInSubquery(invocationOperation))
            return;

        var methodName = method.Name;
        var diagnostic = Diagnostic.Create(GetQueryOrderingRule, invocationOperation.Syntax.GetLocation(), methodName);
        context.ReportDiagnostic(diagnostic);
    }

    private static bool IsOrderingMethod(IMethodSymbol method)
    {
        if (method.ContainingType?.Name != "Queryable" && method.ContainingType?.Name != "Enumerable")
            return false;

        return method.Name is "OrderBy" or "OrderByDescending" or "ThenBy" or "ThenByDescending";
    }

    private static bool IsInGetQueryMethod(IInvocationOperation invocation, Compilation compilation)
    {
        // Walk up the syntax tree to find the containing method
        var syntax = invocation.Syntax;
        MethodDeclarationSyntax? methodDecl = null;

        while (syntax != null)
        {
            if (syntax is MethodDeclarationSyntax method)
            {
                methodDecl = method;
                break;
            }
            syntax = syntax.Parent;
        }

        // Check if we're in a GetQuery or GetQueryAsync method
        if (methodDecl?.Identifier.ValueText is not ("GetQuery" or "GetQueryAsync"))
            return false;

        // Check if we're inside a class that inherits from StandardDataSource<T>
        // Walk up the syntax tree to find the containing class
        var classDecl = methodDecl.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (classDecl != null)
        {
            // Get the semantic model and symbol for the class
            var semanticModel = invocation.SemanticModel;
            if (semanticModel != null)
            {
                var classSymbol = semanticModel.GetDeclaredSymbol(classDecl);
                if (classSymbol != null)
                {
                    return classSymbol.InheritsFromOrImplements("IntelliTect.Coalesce.StandardDataSource`1");
                }
            }
        }

        return false;
    }

    private static bool IsInSubquery(IInvocationOperation invocation)
    {
        // Walk up the operation tree to see if we're inside another LINQ method call that would indicate a subquery
        IOperation? current = invocation.Parent;
        int depth = 0;

        while (current != null)
        {
            if (current is IInvocationOperation parentInvocation)
            {
                var method = parentInvocation.TargetMethod;

                // If we encounter another LINQ method that typically operates on collections,
                // we might be in a subquery context
                if (IsLinqMethod(method))
                {
                    depth++;

                    // If we find methods like Where, Select, etc. that indicate we're processing 
                    // elements of a collection (suggesting a subquery), allow the ordering
                    if (IsSubqueryIndicatingMethod(method) && depth > 1)
                    {
                        return true;
                    }
                }
            }
            current = current.Parent;
        }

        return false;
    }

    private static bool IsLinqMethod(IMethodSymbol method)
    {
        return method.ContainingType?.Name is "Queryable" or "Enumerable";
    }

    private static bool IsSubqueryIndicatingMethod(IMethodSymbol method)
    {
        // Methods that typically indicate we're working with elements inside a collection
        return method.Name is "Where" or "Select" or "SelectMany" or "Any" or "All" or "Count" or "Sum" or "Average" or "Min" or "Max";
    }
}
