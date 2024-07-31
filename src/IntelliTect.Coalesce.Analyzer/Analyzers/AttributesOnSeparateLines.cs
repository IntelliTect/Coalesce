using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace IntelliTect.Analyzer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AttributesOnSeparateLines : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "INTL0101";
        private const string Title = "Attributes separate lines";
        private const string MessageFormat = "Attributes should be on separate lines";
        private const string Description = "All attributes should be on separate lines and be wrapped in their own braces.";
        private const string Category = "Formatting";
        private static readonly string _HelpLinkUri = DiagnosticUrlBuilder.GetUrl(Title,
            DiagnosticId);

        private static readonly DiagnosticDescriptor _Rule = new(DiagnosticId, Title, MessageFormat,
            Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description, _HelpLinkUri);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_Rule);

        public override void Initialize(AnalysisContext context)
        {
            if (context is null)
            {
                throw new System.ArgumentNullException(nameof(context));
            }

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Property, SymbolKind.NamedType, SymbolKind.Method, SymbolKind.Field);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            ISymbol namedTypeSymbol = context.Symbol;

            if (namedTypeSymbol.GetAttributes().Any())
            {
                Dictionary<int, AttributeData> lineDictionary = [];
                foreach (AttributeData attribute in namedTypeSymbol.GetAttributes())
                {
                    SyntaxReference applicationSyntaxReference = attribute.ApplicationSyntaxReference;
                    Microsoft.CodeAnalysis.Text.TextSpan textSpan = applicationSyntaxReference.Span;
                    SyntaxTree syntaxTree = applicationSyntaxReference.SyntaxTree;
                    FileLinePositionSpan lineSpan = syntaxTree.GetLineSpan(textSpan);

                    IEnumerable<int> symbolLineNumbers = namedTypeSymbol.Locations
                        .Where(x => x.GetLineSpan().Path == lineSpan.Path)
                        .Select(x => x.GetLineSpan().StartLinePosition.Line);

                    int attributeLineNo = lineSpan.StartLinePosition.Line;
                    if (lineDictionary.ContainsKey(attributeLineNo) || symbolLineNumbers.Contains(attributeLineNo))
                    {
                        Location location = syntaxTree.GetLocation(textSpan);
                        Diagnostic diagnostic = Diagnostic.Create(_Rule, location, attribute.AttributeClass.Name);

                        context.ReportDiagnostic(diagnostic);
                    }
                    else
                    {
                        lineDictionary.Add(attributeLineNo, attribute);
                    }
                }
            }
        }
    }
}
