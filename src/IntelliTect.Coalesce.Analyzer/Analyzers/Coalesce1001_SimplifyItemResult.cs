using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace IntelliTect.Coalesce.Analyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class Coalesce1001_SimplifyItemResult : DiagnosticAnalyzer
{
    public static readonly DiagnosticDescriptor _Rule = new(
        id: "COA1001",
        title: "ItemResult instantiation can be simplified using implicit conversion",
        messageFormat: "ItemResult instantiation can be simplified to '{0}'",
        category: "Style",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "ItemResult and ItemResult<T> have implicit conversions from boolean, string, and object values. Use the implicit conversion for cleaner, more readable code.");

    public static readonly DiagnosticDescriptor _UnnecessaryRule = new(
        id: "COA1002",
        title: "ItemResult instantiation can be simplified using implicit conversion",
        messageFormat: "ItemResult instantiation can be simplified to '{0}'",
        category: "Style",
        defaultSeverity: DiagnosticSeverity.Hidden,
        isEnabledByDefault: true,
        customTags: [WellKnownDiagnosticTags.Unnecessary],
        description: "ItemResult and ItemResult<T> have implicit conversions from boolean, string, and object values. Use the implicit conversion for cleaner, more readable code.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(_Rule, _UnnecessaryRule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterOperationAction(AnalyzeObjectCreation, OperationKind.ObjectCreation);
    }

    private static void AnalyzeObjectCreation(OperationAnalysisContext context)
    {
        var objectCreation = (IObjectCreationOperation)context.Operation;

        // Get type information
        var type = objectCreation.Type;
        if (type == null) return;

        // Check if this is ItemResult or ItemResult<T>
        if (!IsItemResultType(type)) return;

        // Get the syntax node for location reporting
        var syntaxNode = objectCreation.Syntax as BaseObjectCreationExpressionSyntax;
        if (syntaxNode == null) return;

        // Don't suggest simplification in non-target-typed contexts where it wouldn't be beneficial
        if (!ShouldSuggestSimplification(objectCreation)) return;

        // Create virtual representation of the ItemResult
        var virtualResult = CreateVirtualItemResult(context, objectCreation, type);
        if (virtualResult == null) return;

        // Analyze the virtual result for simplification opportunities
        AnalyzeVirtualItemResult(context, syntaxNode, type, virtualResult);
    }

    private class VirtualItemResult
    {
        public ExpressionSyntax? WasSuccessfulExpression { get; set; }
        public ExpressionSyntax? MessageExpression { get; set; }
        public ExpressionSyntax? ObjectExpression { get; set; }
        public ITypeSymbol? ObjectType { get; set; }
        public bool HasOnlyRelevantProperties { get; set; }
    }

    private static VirtualItemResult? CreateVirtualItemResult(
        OperationAnalysisContext context,
        IObjectCreationOperation objectCreation,
        ITypeSymbol type)
    {
        var result = new VirtualItemResult { HasOnlyRelevantProperties = true };


        foreach (var arg in objectCreation.Arguments)
        {
            switch (arg.Parameter?.Name)
            {
                case "wasSuccessful":
                    result.WasSuccessfulExpression = (arg.Syntax as ArgumentSyntax)?.Expression;
                    break;
                case "message":
                case "errorMessage":
                    result.MessageExpression = (arg.Syntax as ArgumentSyntax)?.Expression;
                    break;
                case "obj":
                    result.ObjectExpression = (arg.Syntax as ArgumentSyntax)?.Expression;
                    result.ObjectType = arg.Value.Type;
                    break;
                default:
                    if (!arg.IsImplicit) result.HasOnlyRelevantProperties = false;
                    break;
            }
        }


        foreach (var assignment in objectCreation.Initializer?.Initializers.OfType<IAssignmentOperation>() ?? [])
        {
            if (assignment.Target is not IPropertyReferenceOperation propRef) continue;

            switch (propRef.Property.Name)
            {
                case "WasSuccessful":
                    result.WasSuccessfulExpression = assignment.Value.Syntax as ExpressionSyntax;
                    break;

                case "Message":
                    result.MessageExpression = assignment.Value.Syntax as ExpressionSyntax;
                    break;

                case "Object":
                    result.ObjectExpression = assignment.Value.Syntax as ExpressionSyntax;
                    result.ObjectType = assignment.Value.Type;
                    break;

                default:
                    // Unknown property, can't simplify
                    result.HasOnlyRelevantProperties = false;
                    break;
            }
        }

        return result;
    }

    private static void AnalyzeVirtualItemResult(
        OperationAnalysisContext context,
        BaseObjectCreationExpressionSyntax objectCreation,
        ITypeSymbol type,
        VirtualItemResult virtualResult)
    {
        // Only analyze if we have relevant properties only
        if (!virtualResult.HasOnlyRelevantProperties) return;

        // Case 1: Can simplify to boolean (true or false)
        if ((virtualResult.ObjectExpression == null || IsNullLiteral(virtualResult.ObjectExpression)) &&
            virtualResult.WasSuccessfulExpression is not null &&
            (virtualResult.MessageExpression == null || IsNullLiteral(virtualResult.MessageExpression)))
        {
            ReportSimplificationDiagnostics(context, objectCreation, virtualResult.WasSuccessfulExpression);
            return;
        }

        // Case 2: Can simplify to string error message
        var wasSuccessfulValue = GetBooleanValueFromExpression(virtualResult.WasSuccessfulExpression);
        if (
            (virtualResult.WasSuccessfulExpression == null || (wasSuccessfulValue is false)) &&
            (virtualResult.ObjectExpression == null || IsNullLiteral(virtualResult.ObjectExpression)) &&
            !IsItemResultOfString(type))
        {
            if (virtualResult.MessageExpression != null && !IsNullLiteral(virtualResult.MessageExpression))
            {
                // Expression message (e.g., literal string, interpolated string, variable)
                ReportSimplificationDiagnostics(context, objectCreation, virtualResult.MessageExpression);
                return;
            }
        }

        // Case 3: Can simplify to object value
        if (virtualResult.ObjectExpression != null &&
            (virtualResult.WasSuccessfulExpression == null || (wasSuccessfulValue is true)) &&
            (virtualResult.MessageExpression == null || IsNullLiteral(virtualResult.MessageExpression)))
        {
            var genericType = (INamedTypeSymbol)type;
            var typeArgument = genericType.TypeArguments[0];

            // Skip ItemResult<string> with string value as it's ambiguous
            if (typeArgument.SpecialType == SpecialType.System_String &&
                virtualResult.ObjectType?.SpecialType == SpecialType.System_String)
            {
                return;
            }

            // Check type compatibility
            if (context.Compilation.HasImplicitConversion(virtualResult.ObjectType, typeArgument))
            {
                ReportSimplificationDiagnostics(context, objectCreation, virtualResult.ObjectExpression);
            }
        }
    }

    private static bool IsItemResultType(ITypeSymbol type)
    {
        return type.Name == "ItemResult" &&
            type.ContainingNamespace?.ToDisplayString() == "IntelliTect.Coalesce.Models";
    }

    private static void ReportSimplificationDiagnostics(
        OperationAnalysisContext context,
        BaseObjectCreationExpressionSyntax objectCreation,
        ExpressionSyntax simplifiedExpression)
    {
        var simplifiedText = simplifiedExpression.ToString();
        var properties = ImmutableDictionary.CreateBuilder<string, string?>();
        properties.Add("SimplifiedExpression", simplifiedText);
        var immutableProperties = properties.ToImmutable();

        // Report the main diagnostic with information
        var mainDiagnostic = Diagnostic.Create(
            _Rule,
            objectCreation.GetLocation(),
            immutableProperties,
            simplifiedText);
        context.ReportDiagnostic(mainDiagnostic);

        // Report unnecessary diagnostic for parts that will be removed
        var syntaxTree = objectCreation.SyntaxTree;
        var keepStart = simplifiedExpression.SpanStart;
        var keepEnd = simplifiedExpression.Span.End;

        // Report unnecessary parts before the kept expression
        if (keepStart > objectCreation.SpanStart)
        {
            var beforeSpan = Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(objectCreation.SpanStart, keepStart);
            var beforeLocation = Location.Create(syntaxTree, beforeSpan);
            var beforeDiagnostic = Diagnostic.Create(_UnnecessaryRule, beforeLocation, immutableProperties, simplifiedText);
            context.ReportDiagnostic(beforeDiagnostic);
        }

        // Report unnecessary parts after the kept expression
        if (keepEnd < objectCreation.Span.End)
        {
            var afterSpan = Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(keepEnd, objectCreation.Span.End);
            var afterLocation = Location.Create(syntaxTree, afterSpan);
            var afterDiagnostic = Diagnostic.Create(_UnnecessaryRule, afterLocation, immutableProperties, simplifiedText);
            context.ReportDiagnostic(afterDiagnostic);
        }
    }

    private static Diagnostic CreateDiagnosticWithSimplifiedExpression(
        BaseObjectCreationExpressionSyntax objectCreation,
        ExpressionSyntax simplifiedExpression)
    {
        var simplifiedText = simplifiedExpression.ToString();
        var properties = ImmutableDictionary.CreateBuilder<string, string?>();
        properties.Add("SimplifiedExpression", simplifiedText);

        return Diagnostic.Create(
            _Rule,
            objectCreation.GetLocation(),
            properties.ToImmutable(),
            simplifiedText);
    }

    private static bool IsItemResultOfString(ITypeSymbol type)
    {
        return type is INamedTypeSymbol { IsGenericType: true, TypeArguments.Length: 1 } namedType &&
               namedType.TypeArguments[0].SpecialType == SpecialType.System_String;
    }

    private static bool? GetBooleanValueFromExpression(ExpressionSyntax? expression)
    {
        return expression switch
        {
            LiteralExpressionSyntax literal when literal.Token.IsKind(SyntaxKind.TrueKeyword) => true,
            LiteralExpressionSyntax literal when literal.Token.IsKind(SyntaxKind.FalseKeyword) => false,
            _ => null
        };
    }

    private static bool IsNullLiteral(ExpressionSyntax? expression)
    {
        return expression is LiteralExpressionSyntax literal &&
               literal.Token.IsKind(SyntaxKind.NullKeyword);
    }

    private static bool ShouldSuggestSimplification(IObjectCreationOperation objectCreation)
    {
        // Check the parent operation to determine if we should suggest simplification
        var parent = objectCreation.Parent;

        // Check if this is within a variable initializer that uses var
        if (parent is IVariableInitializerOperation initializer &&
            initializer.Parent is IVariableDeclaratorOperation varOp &&
            varOp.Parent?.Syntax is VariableDeclarationSyntax varDecl &&
            varDecl.Type.IsVar)
        {
            return false;
        }

        return parent switch
        {
            // Don't suggest when the ItemResult is used as an argument to another method
            // where implicit conversion might not work if the target method is generic
            IArgumentOperation => false,

            // For all other cases, suggest simplification
            _ => true
        };
    }
}
