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

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(Coalesce0201_MissingFileTypeAttributeCodeFixProvider)), Shared]
public class Coalesce0201_MissingFileTypeAttributeCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("COALESCE0201");

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root is null) return;

        var diagnostic = context.Diagnostics.FirstOrDefault(d => FixableDiagnosticIds.Contains(d.Id));
        if (diagnostic is null) return;

        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var parameterNode = root.FindNode(diagnosticSpan);

        if (parameterNode is not ParameterSyntax parameter) return;

        var action = CodeAction.Create(
            title: "Add [FileType] attribute",
            createChangedDocument: c => AddFileTypeAttributeAsync(context.Document, parameter, c),
            equivalenceKey: "AddFileTypeAttribute");

        context.RegisterCodeFix(action, diagnostic);
    }

    private static async Task<Document> AddFileTypeAttributeAsync(
        Document document,
        ParameterSyntax parameter,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root is null) return document;

        // Create the FileType attribute with a placeholder value
        var fileTypeAttribute = SyntaxFactory.Attribute(
            SyntaxFactory.IdentifierName("FileType"))
            .WithArgumentList(
                SyntaxFactory.AttributeArgumentList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.AttributeArgument(
                            SyntaxFactory.LiteralExpression(
                                SyntaxKind.StringLiteralExpression,
                                SyntaxFactory.Literal("*/*"))))));

        var attributeList = SyntaxFactory.AttributeList(
            SyntaxFactory.SingletonSeparatedList(fileTypeAttribute));

        // Add the attribute list while preserving the parameter's leading trivia
        var newParameter = parameter.WithAttributeLists(
            parameter.AttributeLists.Add(attributeList))
            .WithLeadingTrivia(parameter.GetLeadingTrivia());

        var newRoot = root.ReplaceNode(parameter, newParameter);
        return document.WithSyntaxRoot(newRoot);
    }
}
