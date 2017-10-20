using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntelliTect.Coalesce.CodeGeneration.Templating;
using Microsoft.VisualStudio.Web.CodeGeneration;
using IntelliTect.Coalesce.CodeGeneration.Templating.Resolution;
using System.IO;
using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{
    public abstract class RazorTemplateGenerator<TModel> : IFileGenerator<TModel>
    {
        public RazorTemplateGenerator(ITemplateResolver resolver, RazorTemplateCompiler compiler)
        {
            Resolver = resolver;
            Compiler = compiler;
        }

        public TModel Model { get; set; }
        public string OutputPath { get; set; }

        protected ITemplateResolver Resolver { get; }
        protected RazorTemplateCompiler Compiler { get; }


        public abstract TemplateDescriptor Template { get; }


        public virtual async Task<Stream> GetOutputAsync()
        {
            if (Model == null)
            {
                throw new InvalidOperationException($"{nameof(Model)} is null - cannot generate {Template}");
            }

            var resolvedTemplate = this.Resolver.Resolve(Template);
            var compiledTemplate = this.Compiler.GetCachedCompiledTemplate(resolvedTemplate);
            return await this.Compiler.RunTemplateAsync(compiledTemplate, Model);
        }

        public async Task GenerateAsync()
        {
            if (Model == null)
            {
                throw new InvalidOperationException($"{nameof(OutputPath)} is null - cannot generate {Template}");
            }

            using (var contents = await GetOutputAsync())
            {
                using (var writer = File.OpenWrite(OutputPath))
                {
                    await contents.CopyToAsync(writer);
                }
            }

            //if (outputPath.EndsWith(".cs"))
            //{
            //    var syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(result);
            //    var root = syntaxTree.GetRoot();
            //    root = Microsoft.CodeAnalysis.Formatting.Formatter.Format(root, new AdhocWorkspace());
            //    result = root.ToFullString();
            //}
        }

        public virtual bool OutputIsCSharp { get; set; }

        public virtual bool ShouldGenerate(IFileSystem fileSystem) => true;

        public virtual bool Validate() => true;
    }
}
