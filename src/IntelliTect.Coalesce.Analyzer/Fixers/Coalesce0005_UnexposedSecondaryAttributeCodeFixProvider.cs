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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Coalesce0005_UnexposedSecondaryAttributeCodeFixProvider)), Shared]
public class Coalesce0005_UnexposedSecondaryAttributeCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("COA0005", "COA0006");

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var diagnostic = context.Diagnostics.FirstOrDefault();
        if (diagnostic == null) return;

        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var attributeNode = root.FindNode(diagnosticSpan);

        if (attributeNode is not AttributeSyntax targetAttribute) return;

        // Find the containing type or method declaration
        var memberDeclaration = attributeNode.FirstAncestorOrSelf<MemberDeclarationSyntax>();
        if (memberDeclaration == null) return;

        var addCoalesceAction = CodeAction.Create(
            title: "Add [Coalesce] attribute",
            createChangedDocument: c => AddCoalesceAttributeAsync(context.Document, memberDeclaration, c),
            equivalenceKey: "AddCoalesceAttribute");

        context.RegisterCodeFix(addCoalesceAction, diagnostic);

        // Only offer SemanticKernel fix for methods (Execute attribute)
        if (memberDeclaration is MethodDeclarationSyntax)
        {
            var addSemanticKernelAction = CodeAction.Create(
                title: "Add [SemanticKernel] attribute",
                createChangedDocument: c => AddSemanticKernelAttributeAsync(context.Document, memberDeclaration, c),
                equivalenceKey: "AddSemanticKernelAttribute");

            context.RegisterCodeFix(addSemanticKernelAction, diagnostic);
        }
    }

    private static async Task<Document> AddCoalesceAttributeAsync(
        Document document,
        MemberDeclarationSyntax memberDeclaration,
        CancellationToken cancellationToken)
    {
        return await AddAttributeAsync(document, memberDeclaration, "Coalesce", cancellationToken);
    }

    private static async Task<Document> AddSemanticKernelAttributeAsync(
        Document document,
        MemberDeclarationSyntax memberDeclaration,
        CancellationToken cancellationToken)
    {
        return await AddAttributeAsync(document, memberDeclaration, "SemanticKernel", cancellationToken);
    }

    private static async Task<Document> AddAttributeAsync(
        Document document,
        MemberDeclarationSyntax memberDeclaration,
        string attributeName,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null) return document;

        MemberDeclarationSyntax newMemberDeclaration;

        // Find the first attribute list that contains the target attribute
        var targetAttributeList = memberDeclaration.AttributeLists.FirstOrDefault(list =>
            list.Attributes.Any(attr =>
                attr.Name.ToString() is "Execute" or "Service" or "StandaloneEntity"));

        if (targetAttributeList != null)
        {
            // Add the new attribute to the same list as the target attribute
            var newAttribute = SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeName));
            var newAttributeList = targetAttributeList.WithAttributes(
                targetAttributeList.Attributes.Insert(0, newAttribute.WithTrailingTrivia(SyntaxFactory.Space)));

            newMemberDeclaration = memberDeclaration.ReplaceNode(targetAttributeList, newAttributeList);
        }
        else
        {
            // Create a new attribute list and add it first
            var attributeList = SyntaxFactory.AttributeList(
                SyntaxFactory.SingletonSeparatedList(
                    SyntaxFactory.Attribute(SyntaxFactory.IdentifierName(attributeName))
                )
            ).WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);

            newMemberDeclaration = memberDeclaration.WithAttributeLists(
                memberDeclaration.AttributeLists.Insert(0, attributeList));
        }

        var newRoot = root.ReplaceNode(memberDeclaration, newMemberDeclaration);
        return document.WithSyntaxRoot(newRoot);
    }
}
