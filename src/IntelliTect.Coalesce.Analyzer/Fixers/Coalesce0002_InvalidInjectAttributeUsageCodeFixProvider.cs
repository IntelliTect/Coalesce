using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Composition;

namespace IntelliTect.Coalesce.Analyzer.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Coalesce0002_InvalidInjectAttributeUsageCodeFixProvider)), Shared]
public class Coalesce0002_InvalidInjectAttributeUsageCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("COALESCE0002");

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var diagnostic = context.Diagnostics.FirstOrDefault();
        if (diagnostic == null) return;

        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var attributeNode = root.FindNode(diagnosticSpan);

        if (attributeNode is not AttributeSyntax injectAttribute) return;

        var action = CodeAction.Create(
            title: "Remove [Inject] attribute",
            createChangedDocument: c => RemoveInjectAttributeAsync(context.Document, injectAttribute, c),
            equivalenceKey: "RemoveInjectAttribute");

        context.RegisterCodeFix(action, diagnostic);
    }

    private static async Task<Document> RemoveInjectAttributeAsync(
        Document document,
        AttributeSyntax injectAttribute,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null) return document;

        if (injectAttribute.Parent is not AttributeListSyntax attributeList) return document;

        SyntaxNode? newRoot;

        if (attributeList.Attributes.Count == 1)
        {
            // Remove the entire attribute list if it only contains the Inject attribute
            newRoot = root.RemoveNode(attributeList, SyntaxRemoveOptions.KeepLeadingTrivia);
        }
        else
        {
            // Remove only the Inject attribute from the list, handling commas properly
            var newAttributes = attributeList.Attributes.Remove(injectAttribute);
            var newAttributeList = attributeList.WithAttributes(newAttributes);
            newRoot = root.ReplaceNode(attributeList, newAttributeList);
        }

        return document.WithSyntaxRoot(newRoot ?? root);
    }
}
