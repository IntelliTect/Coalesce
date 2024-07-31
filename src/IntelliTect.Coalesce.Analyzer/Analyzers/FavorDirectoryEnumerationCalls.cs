using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace IntelliTect.Analyzer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FavorDirectoryEnumerationCalls : DiagnosticAnalyzer
    {
        private const string Category = "Performance";

        private static readonly DiagnosticDescriptor _Rule301 = new(Rule301.DiagnosticId,
            Rule301.Title,
            Rule301.MessageFormat,
            Category, DiagnosticSeverity.Info, true, Rule301.Description,
            Rule301.HelpMessageUri);

        private static readonly DiagnosticDescriptor _Rule302 = new(Rule302.DiagnosticId,
            Rule302.Title,
            Rule302.MessageFormat,
            Category, DiagnosticSeverity.Info, true, Rule302.Description,
            Rule302.HelpMessageUri);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(_Rule301, _Rule302);

        public override void Initialize(AnalysisContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var expression = (InvocationExpressionSyntax)context.Node;

            if (expression.Expression is not MemberAccessExpressionSyntax memberAccess)
            {
                return;
            }

            if (memberAccess.Expression is not IdentifierNameSyntax nameSyntax)
            {
                return;
            }

            if (string.Equals(nameSyntax.Identifier.Text, "Directory", StringComparison.CurrentCultureIgnoreCase))
            {
                if (memberAccess.ChildNodes().Cast<IdentifierNameSyntax>().Any(x =>
                        string.Equals(x.Identifier.Text, "GetFiles", StringComparison.CurrentCultureIgnoreCase)))
                {
                    // Unsure if this is the best way to determine if member was defined in the project.
                    SymbolInfo symbol = context.SemanticModel.GetSymbolInfo(nameSyntax);
                    if (symbol.Symbol == null || symbol.Symbol.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.IO.Directory")
                    {
                        Location loc = memberAccess.GetLocation();
                        context.ReportDiagnostic(Diagnostic.Create(_Rule301, loc, memberAccess.Name));
                    }
                }

                if (memberAccess.ChildNodes().Cast<IdentifierNameSyntax>().Any(x =>
                    string.Equals(x.Identifier.Text, "GetDirectories", StringComparison.CurrentCultureIgnoreCase)))
                {
                    // Unsure if this is the best way to determine if member was defined in the project.
                    SymbolInfo symbol = context.SemanticModel.GetSymbolInfo(nameSyntax);
                    if (symbol.Symbol is null || symbol.Symbol.OriginalDefinition.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "global::System.IO.Directory")
                    {
                        Location loc = memberAccess.GetLocation();
                        context.ReportDiagnostic(Diagnostic.Create(_Rule302, loc, memberAccess.Name));
                    }
                }
            }
        }

        private static class Rule301
        {
            internal const string DiagnosticId = "INTL0301";
            internal const string Title = "Favor using EnumerateFiles";
            internal const string MessageFormat = "Favor using the method `EnumerateFiles` over the `GetFiles` method";
#pragma warning disable INTL0001 // Allow field to not be prefixed with an underscore to match the style
            internal static readonly string HelpMessageUri = DiagnosticUrlBuilder.GetUrl(Title,
                DiagnosticId);
#pragma warning restore INTL0001 

            internal const string Description =
                "When you use EnumerateFiles, you can start enumerating the collection of names before the whole collection is returned; when you use GetFiles, you must wait for the whole array of names to be returned before you can access the array. Therefore, when you are working with many files and directories, EnumerateFiles can be more efficient.";
        }

        private static class Rule302
        {
            internal const string DiagnosticId = "INTL0302";
            internal const string Title = "Favor using EnumerateDirectories";
            internal const string MessageFormat = "Favor using the method `EnumerateDirectories` over the `GetDirectories` method";
#pragma warning disable INTL0001 // Allow field to not be prefixed with an underscore to match the style
            internal static readonly string HelpMessageUri = DiagnosticUrlBuilder.GetUrl(Title,
                DiagnosticId);
#pragma warning restore INTL0001 

            internal const string Description =
                "When you use EnumerateDirectories, you can start enumerating the collection of names before the whole collection is returned; when you use GetDirectories, you must wait for the whole array of names to be returned before you can access the array. Therefore, when you are working with many files and directories, EnumerateDirectories can be more efficient.";
        }
    }
}
