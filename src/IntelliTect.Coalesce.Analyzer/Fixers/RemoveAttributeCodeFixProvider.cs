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

    private static bool IsAttributeListOnOwnLine(AttributeListSyntax attributeList)
    {
        var attributeLineSpan = attributeList.GetLocation().GetLineSpan();
        var attributeStartLine = attributeLineSpan.StartLinePosition.Line;
        var attributeEndLine = attributeLineSpan.EndLinePosition.Line;

        // Check if there's a preceding token on the same line
        var precedingToken = attributeList.GetFirstToken().GetPreviousToken();
        if (precedingToken != default)
        {
            var precedingLineSpan = precedingToken.GetLocation().GetLineSpan();
            if (precedingLineSpan.EndLinePosition.Line == attributeStartLine)
            {
                return false; // Something else is on the same line before the attribute
            }
        }

        // Check if there's a following token on the same line
        var followingToken = attributeList.GetLastToken().GetNextToken();
        if (followingToken != default)
        {
            var followingLineSpan = followingToken.GetLocation().GetLineSpan();
            if (followingLineSpan.StartLinePosition.Line == attributeEndLine)
            {
                return false; // Something else is on the same line after the attribute
            }
        }

        return true; // Attribute is on its own line
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
            // Remove the entire attribute list.
            if (IsAttributeListOnOwnLine(attributeList))
            {
                newRoot = root.RemoveNode(attributeList, SyntaxRemoveOptions.KeepExteriorTrivia)!;

                // Find the trivia that now represents the blank line that was left by removing the attribute list.
                var trivia = newRoot.FindTrivia(attributeList.SpanStart);

                IEnumerable<SyntaxTrivia> newTrivia = trivia.Token.LeadingTrivia;
                newTrivia = newTrivia
                    // Reverse so we can iterate leftwards in the document
                    .Reverse()
                    // Skip whitespace that trails the newline.
                    .SkipWhile(t => t.IsKind(SyntaxKind.WhitespaceTrivia));

                // If we've reached a newline, remove it. We will then have successfully removed the blank line where the attribute used to be.
                if (newTrivia.FirstOrDefault().IsKind(SyntaxKind.EndOfLineTrivia))
                {
                    newTrivia = newTrivia.Skip(1);
                }

                // Un-reverse back to normal left-to-right order
                newTrivia = newTrivia.Reverse();

                newRoot = newRoot.ReplaceToken(trivia.Token, trivia.Token.WithLeadingTrivia(newTrivia));
            }
            else
            {
                // For attributes not on their own line, keep leading trivia
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
