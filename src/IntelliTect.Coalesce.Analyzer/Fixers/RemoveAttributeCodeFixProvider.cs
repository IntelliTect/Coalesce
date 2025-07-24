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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveAttributeCodeFixProvider)), Shared]
public class RemoveAttributeCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("COA0002", "COA0003", "COA0004");

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var diagnostic = context.Diagnostics.FirstOrDefault(d => FixableDiagnosticIds.Contains(d.Id));
        if (diagnostic == null) return;

        var attributeNode = root.FindNode(diagnostic.Location.SourceSpan);
        if (attributeNode is not AttributeSyntax attribute) return;

        var attributeName = GetAttributeName(attribute);
        var actionTitle = $"Remove [{attributeName}] attribute";
        var equivalenceKey = $"Remove{attributeName}Attribute";

        var action = CodeAction.Create(
            title: actionTitle,
            createChangedDocument: c => RemoveAttributeAsync(context.Document, attribute, c),
            equivalenceKey: equivalenceKey);

        context.RegisterCodeFix(action, diagnostic);
    }

    private static string GetAttributeName(AttributeSyntax attribute)
    {
        var name = attribute.Name.ToString();

        // Remove "Attribute" suffix if present
        if (name.EndsWith("Attribute"))
            name = name.Substring(0, name.Length - "Attribute".Length);

        return name;
    }

    private static async Task<Document> RemoveAttributeAsync(
        Document document,
        AttributeSyntax attribute,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null) return document;

        if (attribute.Parent is not AttributeListSyntax attributeList) return document;

        SyntaxNode? newRoot;

        if (attributeList.Attributes.Count == 1)
        {
            // Remove the entire attribute list - always use advanced trivia handling
            var nextToken = attributeList.GetLastToken().GetNextToken();
            if (!nextToken.IsKind(SyntaxKind.None))
            {
                // Extract any blank lines (EndOfLineTrivia) from the attribute's leading trivia
                var attributeLeadingTrivia = attributeList.GetLeadingTrivia();
                var blankLineTrivia = attributeLeadingTrivia.Where(t => t.IsKind(SyntaxKind.EndOfLineTrivia));

                // Combine blank lines with the next token's existing leading trivia
                var newLeadingTrivia = nextToken.LeadingTrivia.InsertRange(0, blankLineTrivia);
                var newNextToken = nextToken.WithLeadingTrivia(newLeadingTrivia);

                // Replace the next token and then remove the attribute with no trivia
                var tempRoot = root.ReplaceToken(nextToken, newNextToken);
                newRoot = tempRoot.RemoveNode(tempRoot.FindNode(attributeList.Span), SyntaxRemoveOptions.KeepNoTrivia);
            }
            else
            {
                // Fallback if no next token found
                newRoot = root.RemoveNode(attributeList, SyntaxRemoveOptions.KeepLeadingTrivia);
            }
        }
        else
        {
            // Remove only the target attribute from the list, handling commas properly
            var newAttributes = attributeList.Attributes.Remove(attribute);
            var newAttributeList = attributeList.WithAttributes(newAttributes);
            newRoot = root.ReplaceNode(attributeList, newAttributeList);
        }

        return document.WithSyntaxRoot(newRoot ?? root);
    }
}
