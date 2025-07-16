using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace IntelliTect.Coalesce.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ReadAttributePermissionLevelAnalyzer : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            "COA001",
            "ReadAttribute on a property cannot declare a PermissionLevel",
            "ReadAttribute on property '{0}' cannot declare a PermissionLevel. PermissionLevel is only allowed on class-level ReadAttribute.",
            "Coalesce",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: "ReadAttribute on properties should not specify a PermissionLevel parameter. Use roles instead or apply the ReadAttribute at the class level.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeAttributeList, SyntaxKind.AttributeList);
        }

        private static void AnalyzeAttributeList(SyntaxNodeAnalysisContext context)
        {
            var attributeList = (AttributeListSyntax)context.Node;

            // Check if this attribute list is on a property
            if (attributeList.Parent is not PropertyDeclarationSyntax property)
                return;

            foreach (var attribute in attributeList.Attributes)
            {
                var symbolInfo = context.SemanticModel.GetSymbolInfo(attribute);
                if (symbolInfo.Symbol is not IMethodSymbol attributeConstructor)
                    continue;

                var attributeType = attributeConstructor.ContainingType;
                
                // Check if this is ReadAttribute
                if (!IsReadAttribute(attributeType))
                    continue;

                // Check if the attribute has a PermissionLevel argument
                if (HasPermissionLevelArgument(attribute))
                {
                    var diagnostic = Diagnostic.Create(
                        Rule,
                        attribute.GetLocation(),
                        property.Identifier.ValueText);
                    
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private static bool IsReadAttribute(INamedTypeSymbol attributeType)
        {
            return attributeType.Name == "ReadAttribute" && 
                   attributeType.ContainingNamespace.ToDisplayString() == "IntelliTect.Coalesce.DataAnnotations";
        }

        private static bool HasPermissionLevelArgument(AttributeSyntax attribute)
        {
            if (attribute.ArgumentList == null)
                return false;

            foreach (var argument in attribute.ArgumentList.Arguments)
            {
                // Check for SecurityPermissionLevels enum argument
                if (argument.Expression is MemberAccessExpressionSyntax memberAccess)
                {
                    if (memberAccess.Expression is IdentifierNameSyntax identifier &&
                        identifier.Identifier.ValueText == "SecurityPermissionLevels")
                    {
                        return true;
                    }
                }
                
                // Check for direct enum value (if used as a literal)
                if (argument.Expression is IdentifierNameSyntax enumValue &&
                    (enumValue.Identifier.ValueText == "AllowAll" ||
                     enumValue.Identifier.ValueText == "AllowAuthenticated" ||
                     enumValue.Identifier.ValueText == "DenyAll"))
                {
                    return true;
                }
            }

            return false;
        }
    }
}