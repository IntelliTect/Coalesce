using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntelliTect.Coalesce.CodeGeneration.Templating;
using Microsoft.VisualStudio.Web.CodeGeneration;
using IntelliTect.Coalesce.CodeGeneration.Templating.Resolution;
using System.IO;
using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using IntelliTect.Coalesce.CodeGeneration.Configuration;
using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;
using IntelliTect.Coalesce.CodeGeneration.Utilities;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{
    public abstract class RazorTemplateGenerator<TModel> : IFileGenerator<TModel>
    {

        public CoalesceConfiguration CoalesceConfiguration { get; }
        protected ITemplateResolver Resolver { get; }
        protected RazorTemplateCompiler Compiler { get; }

        public RazorTemplateGenerator(CoalesceConfiguration coalesceConfig, ITemplateResolver resolver, RazorTemplateCompiler compiler)
        {
            CoalesceConfiguration = coalesceConfig;
            Resolver = resolver;
            Compiler = compiler;
        }

        public TModel Model { get; set; }
        public string OutputPath { get; set; }
        public abstract TemplateDescriptor Template { get; }

        public string AreaName => CoalesceConfiguration.Output.AreaName;
        public string ModulePrefix => CoalesceConfiguration.Output.TypescriptModulePrefix;


        public virtual async Task<Stream> GetOutputAsync()
        {
            if (Model == null)
            {
                throw new InvalidOperationException($"{nameof(Model)} is null - cannot generate {Template}");
            }

            Stream output;
            try
            {
                var resolvedTemplate = this.Resolver.Resolve(Template);
                var compiledTemplate = this.Compiler.GetCompiledTemplate(resolvedTemplate);
                compiledTemplate.Model = this;
                output = await compiledTemplate.GetOutputAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"There was an error running the template {Template}: {ex.Message}", ex);
            }

            if (OutputIsCSharp)
            {
                using (output)
                {
                    var syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(SourceText.From(output));
                    var root = syntaxTree.GetRoot();
                    root = Microsoft.CodeAnalysis.Formatting.Formatter.Format(root, new AdhocWorkspace());

                    Stream formattedOutput = new MemoryStream((int)output.Length);
                    root.SerializeTo(formattedOutput);
                    await formattedOutput.FlushAsync();
                    return formattedOutput;
                }
            }

            return output;
        }

        public async Task GenerateAsync()
        {
            if (Model == null)
            {
                throw new InvalidOperationException($"{nameof(OutputPath)} is null - cannot generate {Template}");
            }

            if (!ShouldGenerate())
            {
                return;
            }

            using (var contents = await GetOutputAsync())
            {
                Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));

                if (!await FileUtilities.HasDifferencesAsync(contents, OutputPath))
                {
                    return;
                }

                using (FileStream fileStream = new FileStream(OutputPath, FileMode.Create, FileAccess.Write))
                {
                    contents.Seek(0, SeekOrigin.Begin);
                    await contents.CopyToAsync(fileStream);
                };
            }
        }

        /// <summary>
        /// Override to declare that the output of a generator is C# code.
        /// If true, the code will be ran through the Roslyn formatter to clean up whitespace.
        /// </summary>
        public virtual bool OutputIsCSharp => false;

        /// <summary>
        /// Override to add logic that determines whether or not the generator needs to run or not.
        /// 
        /// Generators that are conditional on the state of the filesytem should perform that check in here.
        /// Checking the filesystem should not be done inside GetGenerators() on an ICompositeGenerator<>.
        /// </summary>
        /// <returns>False if the generator should not generate output nor persiste it to disk.</returns>
        public virtual bool ShouldGenerate() => true;

        // TODO: make this return a list of validation issues. These could be warnings or errors.

        /// <summary>
        /// Perform validation on the model of a generator.
        /// Return false if there is a validation issue preventing generation.
        /// </summary>
        /// <returns></returns>
        public virtual bool Validate() => true;
    }
}
