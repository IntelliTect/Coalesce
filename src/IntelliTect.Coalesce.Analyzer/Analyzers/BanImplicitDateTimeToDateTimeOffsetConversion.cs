using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Operations;

namespace IntelliTect.Analyzer.Analyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class BanImplicitDateTimeToDateTimeOffsetConversion : DiagnosticAnalyzer
    {
        private const string Category = "Reliability";

        private static readonly DiagnosticDescriptor _Rule202 = new(Rule202.DiagnosticId,
            Rule202.Title,
            Rule202.MessageFormat,
            Category, DiagnosticSeverity.Warning, true, Rule202.Description,
            Rule202.HelpMessageUri);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(_Rule202);

        public override void Initialize(AnalysisContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
            context.EnableConcurrentExecution();
            context.RegisterOperationAction(AnalyzeInvocation, OperationKind.Conversion);
        }

        private void AnalyzeInvocation(OperationAnalysisContext context)
        {
            if (context.Operation is not IConversionOperation conversionOperation)
            {
                return;
            }

            if (conversionOperation.Conversion.IsImplicit && conversionOperation.Conversion.MethodSymbol is object && conversionOperation.Conversion.MethodSymbol.ContainingType is object)
            {
                INamedTypeSymbol containingType = conversionOperation.Conversion.MethodSymbol.ContainingType;
                INamedTypeSymbol dateTimeOffsetType = context.Compilation.GetTypeByMetadataName("System.DateTimeOffset");
                if (SymbolEqualityComparer.Default.Equals(containingType, dateTimeOffsetType))
                {
                    context.ReportDiagnostic(Diagnostic.Create(_Rule202, conversionOperation.Syntax.GetLocation()));
                }
            }


        }

        private static class Rule202
        {
            internal const string DiagnosticId = "INTL0202";
            internal const string Title = "Do not use implicit conversion from `DateTime` to `DateTimeOffset`";
            internal const string MessageFormat = "Using the symbol 'DateTimeOffset.implicit operator DateTimeOffset(DateTime)' can result in unpredictable behavior";
#pragma warning disable INTL0001 // Allow field to not be prefixed with an underscore to match the style
            internal static readonly string HelpMessageUri = DiagnosticUrlBuilder.GetUrl(Title,
                DiagnosticId);
#pragma warning restore INTL0001 

            internal const string Description =
                "Implicit conversion of `DateTime` to `DateTimeOffset` determines timezone offset based on the `DateTime.Kind` value, and for `DateTimeKind.Unspecified` it assumes `DateTimeKind.Local`, which may lead to differing behavior between running locally and on a server.";
        }
    }
}
