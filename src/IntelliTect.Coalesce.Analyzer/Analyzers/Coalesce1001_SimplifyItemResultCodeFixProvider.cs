using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Analyzer.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Coalesce1001_SimplifyItemResultCodeFixProvider)), Shared]
public class Coalesce1001_SimplifyItemResultCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("COALESCE1001");

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var diagnostic = context.Diagnostics.FirstOrDefault();
        if (diagnostic == null) return;

        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var objectCreation = root.FindNode(diagnosticSpan) as ObjectCreationExpressionSyntax;
        if (objectCreation == null) return;

        // Extract the simplified expression from the diagnostic properties
        var simplifiedExpression = diagnostic.Properties.GetValueOrDefault("SimplifiedExpression");

        if (string.IsNullOrEmpty(simplifiedExpression))
        {
            // Fallback: try to extract from the first argument if available
            if (objectCreation.ArgumentList?.Arguments.Count > 0)
            {
                var firstArg = objectCreation.ArgumentList.Arguments[0];
                simplifiedExpression = firstArg.Expression.ToString();
            }
        }

        if (string.IsNullOrEmpty(simplifiedExpression)) return;

        var action = CodeAction.Create(
            title: $"Replace with '{simplifiedExpression}'",
            createChangedDocument: c => SimplifyItemResultAsync(context.Document, objectCreation, simplifiedExpression!, c),
            equivalenceKey: "SimplifyItemResult");

        context.RegisterCodeFix(action, diagnostic);
    }

    private static async Task<Document> SimplifyItemResultAsync(
        Document document,
        ObjectCreationExpressionSyntax objectCreation,
        string simplifiedExpression,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null) return document;

        // Parse the simplified expression
        var newExpression = SyntaxFactory.ParseExpression(simplifiedExpression)
            .WithTriviaFrom(objectCreation);

        var newRoot = root.ReplaceNode(objectCreation, newExpression);
        return document.WithSyntaxRoot(newRoot);
    }
}
