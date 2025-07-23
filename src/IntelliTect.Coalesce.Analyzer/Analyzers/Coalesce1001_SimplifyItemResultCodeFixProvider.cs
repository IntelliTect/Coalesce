using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Composition;

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
        var objectCreation = root.FindNode(diagnosticSpan) as BaseObjectCreationExpressionSyntax;
        if (objectCreation == null) return;

        // Extract the simplified expression from the diagnostic properties
        var simplifiedExpression = diagnostic.Properties.GetValueOrDefault("SimplifiedExpression");

        if (string.IsNullOrEmpty(simplifiedExpression)) return;

        var action = CodeAction.Create(
            title: $"Replace with '{simplifiedExpression}'",
            createChangedDocument: c => SimplifyItemResultAsync(context.Document, objectCreation, simplifiedExpression!, c),
            equivalenceKey: "SimplifyItemResult");

        context.RegisterCodeFix(action, diagnostic);
    }

    private static async Task<Document> SimplifyItemResultAsync(
        Document document,
        BaseObjectCreationExpressionSyntax objectCreation,
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
