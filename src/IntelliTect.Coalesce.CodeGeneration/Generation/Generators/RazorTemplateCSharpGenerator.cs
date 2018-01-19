using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using IntelliTect.Coalesce.CodeGeneration.Templating.Razor;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Formatting;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;
using Newtonsoft.Json.Linq;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{
    public abstract class RazorTemplateCSharpGenerator<TModel> : RazorTemplateGenerator<TModel>
    {
        public RazorTemplateCSharpGenerator(RazorTemplateServices razorServices) : base(razorServices)
        {
        }

        public string Namespace => GenerationContext.OutputNamespaceRoot;

        [GeneratorConfig]
        public int IndentationSize { get; set; } = 4;

        public override async Task<Stream> GetOutputAsync()
        {
            using (var output = await base.GetOutputAsync())
            {
                await output.FlushAsync();
                var syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(output));
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
                using (var workspace = new AdhocWorkspace())
                {
                    var options = workspace.Options
                        .WithChangedOption(FormattingOptions.NewLine, LanguageNames.CSharp, Environment.NewLine)
                        .WithChangedOption(FormattingOptions.UseTabs, LanguageNames.CSharp, false)
                        .WithChangedOption(FormattingOptions.IndentationSize, LanguageNames.CSharp, IndentationSize)
                        .WithChangedOption(FormattingOptions.SmartIndent, LanguageNames.CSharp,
                            FormattingOptions.IndentStyle.Smart)
                        .WithChangedOption(CSharpFormattingOptions.WrappingKeepStatementsOnSingleLine, true);

                    root = Formatter.Format(root, workspace, options);


                    Stream formattedOutput = new MemoryStream((int) output.Length);
                    using (var writer = new StreamWriter(formattedOutput, Encoding.UTF8, 1024, true))
                    {
                        root.WriteTo(writer);
                        await writer.FlushAsync();
                    }

                    formattedOutput.Seek(0, SeekOrigin.Begin);
                    return formattedOutput;
                }
            }
        }
    }
}