using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Generation.Refactorings
{
    public abstract class RefactoringBase
    {
        public RefactoringBase(ProjectContext projectContext)
        {
            this.ProjectContext = projectContext;
        }

        protected ProjectContext ProjectContext { get; private set; }

        protected CSharpCompilation GetCompilation(SyntaxTree tree)
        {
            var assemblyName = Path.GetRandomFileName();
            return CSharpCompilation.Create(assemblyName)
                                           .AddReferences(ProjectContext.GetTemplateMetadataReferences())
                                           .AddSyntaxTrees(tree);

        }

        public abstract Task<CompilationUnitSyntax> RefactorAsync(CompilationUnitSyntax root);

        public async Task<(CompilationUnitSyntax root, ICollection<Diagnostic> errors)> RunAsync(CompilationUnitSyntax root)
        {
            var newRoot = await RefactorAsync(root);

            // Don't need a semantic model for what we're doing.
            // Leaving this here for a minute - might use it later
            var diagnostics = GetCompilation(newRoot.SyntaxTree).GetDiagnostics();

            var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            if (errors.Any())
            {
                return (root, errors);
            }
            else
            {
                return (newRoot, new Diagnostic[0]);
            }
        }
    }
}
