using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace IntelliTect.Coalesce.Analyzers
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ReadAttributePermissionLevelCodeFixProvider)), Shared]
    public class ReadAttributePermissionLevelCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds =>
            ImmutableArray.Create(ReadAttributePermissionLevelAnalyzer.Rule.Id);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.FirstOrDefault(d => FixableDiagnosticIds.Contains(d.Id));
            if (diagnostic == null)
                return;

            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var attributeNode = root.FindNode(diagnosticSpan) as AttributeSyntax;
            if (attributeNode == null)
                return;

            // Register a code action that will invoke the fix.
            var action = CodeAction.Create(
                title: "Remove PermissionLevel from ReadAttribute",
                createChangedDocument: c => RemovePermissionLevelArgument(context.Document, attributeNode, c),
                equivalenceKey: "RemovePermissionLevelFromReadAttribute");

            context.RegisterCodeFix(action, diagnostic);
        }

        private static async Task<Document> RemovePermissionLevelArgument(
            Document document,
            AttributeSyntax attributeNode,
            CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            if (attributeNode.ArgumentList == null)
                return document;

            var newArguments = new List<AttributeArgumentSyntax>();

            // Keep only arguments that are not PermissionLevel arguments
            foreach (var argument in attributeNode.ArgumentList.Arguments)
            {
                if (!IsPermissionLevelArgument(argument))
                {
                    newArguments.Add(argument);
                }
            }

            AttributeSyntax newAttributeNode;
            
            // If no arguments remain, remove the entire argument list
            if (newArguments.Count == 0)
            {
                newAttributeNode = attributeNode.WithArgumentList(null);
            }
            else
            {
                var newArgumentList = SyntaxFactory.AttributeArgumentList(
                    SyntaxFactory.SeparatedList(newArguments));
                newAttributeNode = attributeNode.WithArgumentList(newArgumentList);
            }

            var newRoot = root.ReplaceNode(attributeNode, newAttributeNode);
            return document.WithSyntaxRoot(newRoot);
        }

        private static bool IsPermissionLevelArgument(AttributeArgumentSyntax argument)
        {
            // Check for SecurityPermissionLevels enum argument
            if (argument.Expression is MemberAccessExpressionSyntax memberAccess)
            {
                if (memberAccess.Expression is IdentifierNameSyntax identifier &&
                    identifier.Identifier.ValueText == "SecurityPermissionLevels")
                {
                    return true;
                }
            }
            
            // Check for direct enum value
            if (argument.Expression is IdentifierNameSyntax enumValue &&
                (enumValue.Identifier.ValueText == "AllowAll" ||
                 enumValue.Identifier.ValueText == "AllowAuthenticated" ||
                 enumValue.Identifier.ValueText == "DenyAll"))
            {
                return true;
            }

            return false;
        }
    }
}