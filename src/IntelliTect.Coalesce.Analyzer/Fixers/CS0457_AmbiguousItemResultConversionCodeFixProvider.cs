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

namespace IntelliTect.Coalesce.Analyzer.Fixers;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(CS0457_AmbiguousItemResultConversionCodeFixProvider)), Shared]
public class CS0457_AmbiguousItemResultConversionCodeFixProvider : CodeFixProvider
{
    public sealed override ImmutableArray<string> FixableDiagnosticIds =>
        ImmutableArray.Create("CS0457");

    public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

    public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
        if (root == null) return;

        var diagnostic = context.Diagnostics.FirstOrDefault(d => d.Id == "CS0457");
        if (diagnostic == null) return;

        // Check if this is specifically an ItemResult<string> ambiguous conversion error
        var message = diagnostic.GetMessage();
        if (!IsItemResultAmbiguousConversion(message)) return;

        var diagnosticSpan = diagnostic.Location.SourceSpan;
        var node = root.FindNode(diagnosticSpan);

        // Find the string expression that's causing the ambiguity
        var stringExpression = FindStringExpression(node);
        if (stringExpression == null) return;

        // Create both code fix actions
        var successAction = CodeAction.Create(
            title: "Construct as success with result",
            createChangedDocument: c => CreateSuccessfulResultAsync(context.Document, stringExpression, c),
            equivalenceKey: "CreateSuccessfulResult");

        var errorAction = CodeAction.Create(
            title: "Construct as error with message",
            createChangedDocument: c => CreateErrorResultAsync(context.Document, stringExpression, c),
            equivalenceKey: "CreateErrorResult");

        context.RegisterCodeFix(successAction, diagnostic);
        context.RegisterCodeFix(errorAction, diagnostic);
    }

    private static bool IsItemResultAmbiguousConversion(string message)
    {
        return message.Contains("ItemResult") &&
               message.Contains("implicit operator") &&
               message.Contains("string") &&
               (message.Contains("ItemResult<string>") || message.Contains("ItemResult<System.String>"));
    }

    private static ExpressionSyntax? FindStringExpression(SyntaxNode node)
    {
        // Look for various patterns where a string might be used in an ItemResult context
        return node switch
        {
            // Direct assignment: ItemResult<string> result = stringVar;
            AssignmentExpressionSyntax assignment => assignment.Right,

            // Variable declaration: ItemResult<string> result = stringVar;
            VariableDeclaratorSyntax declarator when declarator.Initializer?.Value != null => declarator.Initializer.Value,

            // Return statement: return stringVar;
            ReturnStatementSyntax returnStmt when returnStmt.Expression != null => returnStmt.Expression,

            // Method argument: Method(stringVar)
            ArgumentSyntax argument => argument.Expression,

            // The expression itself might be the problematic node
            _ when node is ExpressionSyntax expr => expr,

            // Walk up to find the parent expression that might contain the string
            _ => node.Parent switch
            {
                AssignmentExpressionSyntax assignment => assignment.Right,
                VariableDeclaratorSyntax declarator when declarator.Initializer?.Value != null => declarator.Initializer.Value,
                ReturnStatementSyntax returnStmt when returnStmt.Expression != null => returnStmt.Expression,
                ArgumentSyntax argument => argument.Expression,
                _ => null
            }
        };
    }

    private static async Task<Document> CreateSuccessfulResultAsync(
        Document document,
        ExpressionSyntax stringExpression,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null) return document;

        // Create: new(true, obj: stringExpression)
        var newExpression = SyntaxFactory.ImplicitObjectCreationExpression()
            .WithArgumentList(SyntaxFactory.ArgumentList(
                SyntaxFactory.SeparatedList<ArgumentSyntax>(new[]
                {
                    SyntaxFactory.Argument(
                        SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)),
                    SyntaxFactory.Argument(
                        SyntaxFactory.NameColon("obj"),
                        SyntaxFactory.Token(SyntaxKind.None),
                        stringExpression)
                })))
            .WithTriviaFrom(stringExpression);

        var newRoot = root.ReplaceNode(stringExpression, newExpression);
        return document.WithSyntaxRoot(newRoot);
    }

    private static async Task<Document> CreateErrorResultAsync(
        Document document,
        ExpressionSyntax stringExpression,
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        if (root == null) return document;

        // Create: new(false, message: stringExpression)
        var newExpression = SyntaxFactory.ImplicitObjectCreationExpression()
            .WithArgumentList(SyntaxFactory.ArgumentList(
                SyntaxFactory.SeparatedList<ArgumentSyntax>(new[]
                {
                    SyntaxFactory.Argument(
                        SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression)),
                    SyntaxFactory.Argument(
                        SyntaxFactory.NameColon("message"),
                        SyntaxFactory.Token(SyntaxKind.None),
                        stringExpression)
                })))
            .WithTriviaFrom(stringExpression);

        var newRoot = root.ReplaceNode(stringExpression, newExpression);
        return document.WithSyntaxRoot(newRoot);
    }
}
