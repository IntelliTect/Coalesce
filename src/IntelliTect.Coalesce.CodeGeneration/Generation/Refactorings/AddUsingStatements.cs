using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis.Editing;

namespace IntelliTect.Coalesce.CodeGeneration.Generation.Refactorings
{
    /// <summary>
    /// An idea for a code generation technique.
    /// Ended up not pursuing it because in order to do it the "right" way,
    /// it ends up taking a SIGNIFICANT chunk of time to run.
    /// </summary>
    public class AddUsingStatements : RefactoringBase
    {
        private List<NameSyntax> Usings = new List<NameSyntax>();

        public AddUsingStatements(ProjectContext projectContext) : base(projectContext)
        {
        }

        private CompilationUnitSyntax RefactorOneByOne(CompilationUnitSyntax root)
        {

            var candidateNames = root
                .DescendantNodes(n => !(n is QualifiedNameSyntax))
                .OfType<QualifiedNameSyntax>()
                .Where(node => !(
                        // Part of a longer qualified name
                        node.Parent.IsKind(SyntaxKind.QualifiedName)
                     // Already part of a using statement
                     || node.Parent.IsKind(SyntaxKind.UsingDirective)
                     // Part of a namespace declaration
                     || node.Parent.IsKind(SyntaxKind.NamespaceDeclaration))
                 )
                 .Where(node => node.Right.IsKind(SyntaxKind.IdentifierName))
                 .ToList();

            root = root.TrackNodes(candidateNames);

            var workspace = new AdhocWorkspace();

            foreach (var originalCandidate in candidateNames)
            {
                var node = root.GetCurrentNode(originalCandidate);

                var editor = new SyntaxEditor(root, workspace);

                editor.InsertAfter(root.Usings.Last(), UsingDirective(node.Left).NormalizeWhitespace().WithTrailingTrivia(LineFeed));

                var annotation = new SyntaxAnnotation(Guid.NewGuid().ToString());
                editor.ReplaceNode(node, IdentifierName(node.Right.Identifier).WithAdditionalAnnotations(annotation));

                var newRoot = editor.GetChangedRoot();
                var newSemModel = GetCompilation(newRoot.SyntaxTree).GetSemanticModel(newRoot.SyntaxTree);
                var newNode = newRoot.GetAnnotatedNodes(annotation).Single();
                var typeInfo = newSemModel.GetTypeInfo(newNode);
                if (typeInfo.Type is IErrorTypeSymbol errorSymbol && errorSymbol.CandidateReason == CandidateReason.Ambiguous)
                {
                    Console.WriteLine($"ambig! {node} -> {newNode}");
                }
                else
                {
                    root = newRoot as CompilationUnitSyntax;
                }
            }


            var newUsings = root.Usings
                .Distinct<UsingDirectiveSyntax>(new SyntaxNodeComparer())
                // Order static usings last. Order all usings by the namespace referenced.
                .OrderBy(u => u.StaticKeyword.Value != null).ThenBy(u => u.Name.ToString())
                .ToArray();

            root = root.WithUsings(new SyntaxList<UsingDirectiveSyntax>().AddRange(newUsings));

            return root;
        }

        private CompilationUnitSyntax RefactorAllAtOnceQuickly(CompilationUnitSyntax root)
        {
            //var rewriter = new Rewriter(this);
            //initialSemanticModel = GetCompilation(root.SyntaxTree).GetSemanticModel(root.SyntaxTree);
            var originalRoot = root;

            var candidateNames = root
                .DescendantNodes(n => !(n is QualifiedNameSyntax))
                .OfType<QualifiedNameSyntax>()
                .Where(node => !(
                        // Part of a longer qualified name
                        node.Parent.IsKind(SyntaxKind.QualifiedName)
                        // Already part of a using statement
                     || node.Parent.IsKind(SyntaxKind.UsingDirective)
                        // Part of a namespace declaration
                     || node.Parent.IsKind(SyntaxKind.NamespaceDeclaration))
                 )
                 .Where(node => node.Right.IsKind(SyntaxKind.IdentifierName))
                 .ToList();

            var workspace = new AdhocWorkspace();
            var editor = new SyntaxEditor(root, workspace);
            var annotations = new List<SyntaxAnnotation>();

            foreach (var originalCandidate in candidateNames)
            {
                var node = originalCandidate;
                editor.InsertAfter(root.Usings.Last(), UsingDirective(node.Left).NormalizeWhitespace().WithTrailingTrivia(LineFeed));

                var annotation = new SyntaxAnnotation(Guid.NewGuid().ToString());
                editor.ReplaceNode(node, IdentifierName(node.Right.Identifier).WithAdditionalAnnotations(annotation));
                annotations.Add(annotation);
            }

            root = editor.GetChangedRoot() as CompilationUnitSyntax;
            var newSemModel = GetCompilation(root.SyntaxTree).GetSemanticModel(root.SyntaxTree);
            foreach (var annotation in annotations)
            {
                var newNode = root.GetAnnotatedNodes(annotation).Single();
                if (newNode != null)
                {
                    var typeInfo = newSemModel.GetTypeInfo(newNode);
                    if (typeInfo.Type is IErrorTypeSymbol errorSymbol)
                    {
                        Console.WriteLine("err symbol");
                        if (errorSymbol.CandidateReason == CandidateReason.Ambiguous)
                        {
                            // Our refactor introduced an ambiguous reference.
                            // Since this is the version that runs quickly,
                            // we aren't doing surgical attempts at checking each using one by one.
                            // So, just give up and return what we started with.
                            return originalRoot;
                        }
                    }
                }
            }

            var newUsings = root.Usings
                .Distinct<UsingDirectiveSyntax>(new SyntaxNodeComparer())
                // Order static usings last. Order all usings by the namespace referenced.
                .OrderBy(u => u.StaticKeyword.Value != null).ThenBy(u => u.Name.ToString())
                .ToArray();

            root = root.WithUsings(new SyntaxList<UsingDirectiveSyntax>().AddRange(newUsings));

            return root;
        }

        public override Task<CompilationUnitSyntax> RefactorAsync(CompilationUnitSyntax root)
        {
            // not actually anything async here.
            return Task.FromResult(RefactorAllAtOnceQuickly(root));
        }

        private class SyntaxNodeComparer : IEqualityComparer<SyntaxNode>
        {
            public bool Equals(SyntaxNode x, SyntaxNode y) => x.ToString() == y.ToString();
            public int GetHashCode(SyntaxNode obj) => obj.ToString().GetHashCode();
        }
    }
}
