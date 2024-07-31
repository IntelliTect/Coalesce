using System;
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
using Microsoft.CodeAnalysis.Formatting;

namespace IntelliTect.Analyzer.CodeFixes
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AttributesOnSeparateLines))]
    [Shared]
    public class AttributesOnSeparateLines : CodeFixProvider
    {
        private const string Title = "Fix Format Violation: Put Attributes on separate Lines";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(Analyzers.AttributesOnSeparateLines.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            Diagnostic diagnostic = context.Diagnostics.First();
            Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            SyntaxToken declaration = root.FindToken(diagnosticSpan.Start);

            // Find the enclosing AttributeList
            SyntaxNode attributeList = declaration.Parent;
            while (!attributeList.IsKind(SyntaxKind.AttributeList))
            {
                attributeList = attributeList.Parent;
            }

            // Get the class, method or property adjacent to the AttributeList
            SyntaxNode parentDeclaration = attributeList.Parent;

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(
                CodeAction.Create(
                    title: Title,
                    createChangedDocument: c => PutOnSeparateLine(context.Document, parentDeclaration, c),
                    equivalenceKey: Title),
                diagnostic);
        }

        private static async Task<Document> PutOnSeparateLine(Document document, SyntaxNode parentDeclaration, CancellationToken cancellationToken)
        {
            var attributeLists = new SyntaxList<AttributeListSyntax>();

            // put every attribute into it's own attributelist eg.: [A,B,C] => [A][B][C]
            foreach (AttributeSyntax attribute in GetAttributeListSyntaxes(parentDeclaration).SelectMany(l => l.Attributes))
            {
                attributeLists = attributeLists.Add(
                    SyntaxFactory.AttributeList(
                        SyntaxFactory.SeparatedList(
                            new[] {
                                    SyntaxFactory.Attribute(
                                        attribute.Name,
                                        attribute.ArgumentList)
                            })));
            }

            // the formatter-annotation will wrap every attribute on a separate line
            SyntaxNode newNode = BuildNodeWithAttributeLists(parentDeclaration, attributeLists)
                .WithAdditionalAnnotations(Formatter.Annotation);

            // Replace the old local declaration with the new local declaration.
            SyntaxNode oldRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            SyntaxNode newRoot = oldRoot.ReplaceNode(parentDeclaration, newNode);

            return document.WithSyntaxRoot(newRoot);
        }

        private static IEnumerable<AttributeListSyntax> GetAttributeListSyntaxes(SyntaxNode node)
        {
            return node switch
            {
                ClassDeclarationSyntax c => c.AttributeLists,
                MethodDeclarationSyntax m => m.AttributeLists,
                PropertyDeclarationSyntax p => p.AttributeLists,
                FieldDeclarationSyntax f => f.AttributeLists,
                _ => throw new NotImplementedException(),
            };
        }

        private static SyntaxNode BuildNodeWithAttributeLists(SyntaxNode node, SyntaxList<AttributeListSyntax> attributeLists)
        {
            return node switch
            {
                ClassDeclarationSyntax c => c.WithAttributeLists(attributeLists),
                MethodDeclarationSyntax m => m.WithAttributeLists(attributeLists),
                PropertyDeclarationSyntax p => p.WithAttributeLists(attributeLists),
                FieldDeclarationSyntax f => f.WithAttributeLists(attributeLists),
                _ => throw new NotImplementedException(),
            };
        }
    }
}
