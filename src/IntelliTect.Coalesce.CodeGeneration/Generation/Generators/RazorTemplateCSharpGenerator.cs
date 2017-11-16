using IntelliTect.Coalesce.CodeGeneration.Templating.Razor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{
    public abstract class RazorTemplateCSharpGenerator<TModel> : RazorTemplateGenerator<TModel>
    {
        public RazorTemplateCSharpGenerator(RazorTemplateServices razorServices) : base(razorServices)
        {
        }

        public string Namespace => GenerationContext.OutputNamespaceRoot;

        public override async Task<Stream> GetOutputAsync()
        {
            using (var output = await base.GetOutputAsync())
            {
                await output.FlushAsync();
                var syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(SourceText.From(output));
                var root = syntaxTree.GetRoot();

                // Abandoned idea for refactoring code with Roslyn to clean up fully qualified names.
                // May revisit at some point if there's any interest.
                // The point of it is to add using statements for names that are fully-qualified,
                // and then remove that full qualification.
                //var refactors = new[]
                //{
                //    new Refactorings.AddUsingStatements(GenerationContext.WebProject)
                //};
                //foreach (var refactor in refactors)
                //{
                //    root = await refactor.RefactorAsync(root as CompilationUnitSyntax);
                //}

                root = Microsoft.CodeAnalysis.Formatting.Formatter.Format(root, new AdhocWorkspace());

                Stream formattedOutput = new MemoryStream((int)output.Length);
                var writer = new StreamWriter(formattedOutput, System.Text.Encoding.UTF8);
                root.WriteTo(writer);
                await writer.FlushAsync();
                formattedOutput.Seek(0, SeekOrigin.Begin);
                return formattedOutput;
            }
        }
    }
}
