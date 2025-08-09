using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace IntelliTect.Coalesce.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FileTypeAttributeAnalyzer : DiagnosticAnalyzer
    {
        public static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
            "COA001",
            "Consider adding FileType attribute to file parameters",
            "Consider adding [FileType] attribute to parameter '{0}' to provide file type suggestions",
            "Usage", 
            DiagnosticSeverity.Info,
            isEnabledByDefault: true,
            description: "The FileType attribute provides suggestions for acceptable file types to help with validation and UI hints. It does not enforce strict restrictions.");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
            ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeParameter, SyntaxKind.Parameter);
        }

        private static void AnalyzeParameter(SyntaxNodeAnalysisContext context)
        {
            var parameter = (ParameterSyntax)context.Node;
            var semanticModel = context.SemanticModel;

            // Get the parameter symbol
            if (semanticModel.GetDeclaredSymbol(parameter) is not IParameterSymbol parameterSymbol)
                return;

            // Check if parameter is a file type that might benefit from FileType attribute
            if (!IsFileParameter(parameterSymbol))
                return;

            // Check if FileType attribute is already present
            if (HasFileTypeAttribute(parameterSymbol))
                return;

            // Check if this is a service method exposed by interface (skip suggestion)
            if (IsServiceMethodExposedByInterface(parameterSymbol.ContainingSymbol))
                return;

            // Report diagnostic suggesting FileType attribute
            var diagnostic = Diagnostic.Create(
                Rule,
                parameter.Identifier.GetLocation(),
                parameterSymbol.Name);

            context.ReportDiagnostic(diagnostic);
        }

        private static bool IsFileParameter(IParameterSymbol parameter)
        {
            var typeName = parameter.Type.ToDisplayString();
            
            // Check for common file-related types
            return typeName.Contains("IFormFile") ||
                   typeName.Contains("Stream") ||
                   typeName.Contains("byte[]") ||
                   (parameter.Name.Contains("file", StringComparison.OrdinalIgnoreCase) && 
                    (typeName.Contains("string") || typeName.Contains("byte")));
        }

        private static bool HasFileTypeAttribute(IParameterSymbol parameter)
        {
            return parameter.GetAttributes()
                .Any(attr => attr.AttributeClass?.Name == "FileTypeAttribute");
        }

        private static bool IsServiceMethodExposedByInterface(ISymbol? containingMethod)
        {
            if (containingMethod is not IMethodSymbol method)
                return false;

            var containingType = method.ContainingType;
            
            // Check if the containing type implements interfaces that expose this method
            var interfaces = containingType.AllInterfaces;
            
            foreach (var @interface in interfaces)
            {
                var interfaceMethod = @interface.GetMembers()
                    .OfType<IMethodSymbol>()
                    .FirstOrDefault(m => SymbolEqualityComparer.Default.Equals(
                        containingType.FindImplementationForInterfaceMember(m), method));
                
                if (interfaceMethod != null)
                {
                    // This method is exposed by an interface, skip the suggestion
                    return true;
                }
            }

            return false;
        }
    }
}