using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Analyzer.Analyzers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveCoalesceAttributeCodeFixProvider)), Shared]
public class RemoveCoalesceAttributeCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("COALESCE0003", "COALESCE0004");

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var diagnostic = context.Diagnostics.FirstOrDefault(d => FixableDiagnosticIds.Contains(d.Id));
        if (diagnostic == null) return;

        var attributeNode = root.FindNode(diagnostic.Location.SourceSpan);
        if (attributeNode is not AttributeSyntax coalesceAttribute) return;

        var action = CodeAction.Create(
            title: "Remove [Coalesce] attribute",
            createChangedDocument: c => RemoveCoalesceAttributeAsync(context.Document, coalesceAttribute, c),
            equivalenceKey: "RemoveCoalesceAttribute");

        context.RegisterCodeFix(action, diagnostic);
    }

    private static async Task<Document> RemoveCoalesceAttributeAsync(
        Document document,
        AttributeSyntax coalesceAttribute,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null) return document;

        if (coalesceAttribute.Parent is not AttributeListSyntax attributeList) return document;

        SyntaxNode? newRoot;

        if (attributeList.Attributes.Count == 1)
        {
            // Remove the entire attribute list if it only contains the Coalesce attribute
            newRoot = root.RemoveNode(attributeList, SyntaxRemoveOptions.KeepNoTrivia);
        }
        else
        {
            // Remove only the Coalesce attribute from the list
            var newAttributeList = attributeList.RemoveNode(coalesceAttribute, SyntaxRemoveOptions.KeepNoTrivia);
            if (newAttributeList != null)
            {
                newRoot = root.ReplaceNode(attributeList, newAttributeList);
            }
            else
            {
                newRoot = root.RemoveNode(attributeList, SyntaxRemoveOptions.KeepNoTrivia);
            }
        }

        return document.WithSyntaxRoot(newRoot ?? root);
    }
}
