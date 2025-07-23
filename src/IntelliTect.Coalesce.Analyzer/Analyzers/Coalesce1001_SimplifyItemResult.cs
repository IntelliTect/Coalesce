using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;
using System.Collections.Immutable;
using System.Linq;

namespace IntelliTect.Coalesce.Analyzer.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class Coalesce1001_SimplifyItemResult : DiagnosticAnalyzer
{
    private static readonly DiagnosticDescriptor _Rule = new(
        id: "COALESCE1001",
        title: "ItemResult instantiation can be simplified using implicit conversion",
        messageFormat: "ItemResult instantiation can be simplified to '{0}'",
        category: "Style",
        defaultSeverity: DiagnosticSeverity.Info,
        isEnabledByDefault: true,
        description: "ItemResult and ItemResult<T> have implicit conversions from boolean, string, and object values. Use the implicit conversion for cleaner, more readable code.");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(_Rule);

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
        var syntaxNode = objectCreation.Syntax as ObjectCreationExpressionSyntax;
        if (syntaxNode == null) return;

        // Create virtual representation of the ItemResult
        var virtualResult = CreateVirtualItemResult(context, objectCreation, type);
        if (virtualResult == null) return;

        // Analyze the virtual result for simplification opportunities
        AnalyzeVirtualItemResult(context, syntaxNode, type, virtualResult);
    }

    private class VirtualItemResult
    {
        public bool? WasSuccessful { get; set; }
        public string? Message { get; set; }
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

        // Process constructor arguments
        if (objectCreation.Arguments.Length > 0)
        {
            if (!ProcessConstructorArguments(context, objectCreation.Arguments, result))
                return null;
        }

        // Process object initializer
        if (objectCreation.Initializer != null)
        {
            if (!ProcessObjectInitializer(context, objectCreation.Initializer, result))
                return null;
        }

        return result;
    }

    private static bool ProcessConstructorArguments(
        OperationAnalysisContext context,
        ImmutableArray<IArgumentOperation> arguments,
        VirtualItemResult result)
    {
        foreach (var arg in arguments)
        {
            switch (arg.Parameter?.Name)
            {
                case "wasSuccessful":
                    result.WasSuccessful = GetBooleanLiteralValue(arg.Value);
                    break;
                case "message":
                case "errorMessage":
                    result.Message = GetStringLiteralValue(arg.Value);
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

        return true;
    }

    private static bool ProcessObjectInitializer(
        OperationAnalysisContext context,
        IObjectOrCollectionInitializerOperation initializer,
        VirtualItemResult result)
    {
        var assignments = initializer.Initializers.OfType<IAssignmentOperation>().ToList();
        if (assignments.Count == 0) return false;

        foreach (var assignment in assignments)
        {
            if (assignment.Target is not IPropertyReferenceOperation propRef) continue;

            var propName = propRef.Property.Name;
            switch (propName)
            {
                case "WasSuccessful":
                    result.WasSuccessful = GetBooleanLiteralValue(assignment.Value);
                    break;

                case "Message":
                    result.Message = GetStringLiteralValue(assignment.Value);
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

        return true;
    }

    private static void AnalyzeVirtualItemResult(
        OperationAnalysisContext context,
        ObjectCreationExpressionSyntax objectCreation,
        ITypeSymbol type,
        VirtualItemResult virtualResult)
    {
        // Only analyze if we have relevant properties only
        if (!virtualResult.HasOnlyRelevantProperties) return;

        // Case 1: Can simplify to boolean (true or false)
        if (virtualResult.ObjectExpression == null && virtualResult.Message == null)
        {
            var boolValue = virtualResult.WasSuccessful == true ? "true" : "false";
            if (virtualResult.WasSuccessful.HasValue)
            {
                var diagnostic = CreateDiagnosticWithSimplifiedExpression(objectCreation, boolValue);
                context.ReportDiagnostic(diagnostic);
                return;
            }
        }

        // Case 2: Can simplify to string error message
        if (virtualResult.WasSuccessful is null or false &&
            virtualResult.Message != null &&
            virtualResult.ObjectExpression == null &&
            !IsItemResultOfString(type))
        {
            var simplifiedExpression = $"\"{virtualResult.Message}\"";
            var diagnostic = CreateDiagnosticWithSimplifiedExpression(objectCreation, simplifiedExpression);
            context.ReportDiagnostic(diagnostic);
            return;
        }

        // Case 3: Can simplify to object value
        if (virtualResult.ObjectExpression != null &&
            IsGenericItemResult(type) &&
            (virtualResult.WasSuccessful != false) &&
            (virtualResult.Message == null))
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
                var objText = virtualResult.ObjectExpression.ToString();
                var diagnostic = CreateDiagnosticWithSimplifiedExpression(objectCreation, objText);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    private static bool IsItemResultType(ITypeSymbol type)
    {
        return type.ContainingNamespace?.ToDisplayString() == "IntelliTect.Coalesce.Models" &&
               (type.Name == "ItemResult" ||
                (type is INamedTypeSymbol { IsGenericType: true } namedType &&
                 namedType.ConstructedFrom.Name == "ItemResult"));
    }

    private static Diagnostic CreateDiagnosticWithSimplifiedExpression(
        ObjectCreationExpressionSyntax objectCreation,
        string simplifiedExpression)
    {
        var properties = ImmutableDictionary.CreateBuilder<string, string?>();
        properties.Add("SimplifiedExpression", simplifiedExpression);

        return Diagnostic.Create(
            _Rule,
            objectCreation.GetLocation(),
            properties.ToImmutable(),
            simplifiedExpression);
    }

    private static bool IsItemResultOfString(ITypeSymbol type)
    {
        return type is INamedTypeSymbol { IsGenericType: true, TypeArguments.Length: 1 } namedType &&
               namedType.TypeArguments[0].SpecialType == SpecialType.System_String;
    }

    private static bool IsGenericItemResult(ITypeSymbol type)
    {
        return type is INamedTypeSymbol { IsGenericType: true } namedType &&
               namedType.ConstructedFrom.Name == "ItemResult" &&
               namedType.ContainingNamespace?.ToDisplayString() == "IntelliTect.Coalesce.Models";
    }

    private static bool? GetBooleanLiteralValue(IOperation operation)
    {
        return operation switch
        {
            ILiteralOperation literal when literal.ConstantValue.HasValue && literal.ConstantValue.Value is bool boolValue => boolValue,
            _ => null
        };
    }

    private static string? GetStringLiteralValue(IOperation operation)
    {
        return operation switch
        {
            ILiteralOperation literal when literal.ConstantValue.HasValue && literal.ConstantValue.Value is string stringValue => stringValue,
            _ => null
        };
    }

    private static bool IsNullOrDefault(IOperation operation)
    {
        return operation switch
        {
            ILiteralOperation literal when literal.ConstantValue.HasValue && literal.ConstantValue.Value == null => true,
            IDefaultValueOperation => true,
            _ => false
        };
    }

}
