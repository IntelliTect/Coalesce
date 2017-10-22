using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntelliTect.Coalesce.CodeGeneration.Templating;
using IntelliTect.Coalesce.CodeGeneration.Templating.Resolution;
using IntelliTect.Coalesce.CodeGeneration.Configuration;
using IntelliTect.Coalesce.CodeGeneration.Utilities;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{
    /// <summary>
    /// Wrapper class to encapsulate all services needed for a RazorTemplateGenerator.
    /// This prevents constructors from being enormous due to all the services.
    /// </summary>
    public class RazorServices
    {
        public RazorServices(GenerationContext genContext, ITemplateResolver resolver, RazorTemplateCompiler compiler)
        {
            GenerationContext = genContext;
            Resolver = resolver;
            Compiler = compiler;
        }

        public GenerationContext GenerationContext { get; }
        public ITemplateResolver Resolver { get; }
        public RazorTemplateCompiler Compiler { get; }
    }

    public abstract class RazorTemplateGenerator<TModel> : IFileGenerator<TModel>
    {
        public RazorTemplateGenerator(RazorServices razorServices)
        {
            RazorServices = razorServices;
        }

        public RazorServices RazorServices { get; }

        public GenerationContext GenerationContext => RazorServices.GenerationContext;
        protected ITemplateResolver Resolver => RazorServices.Resolver;
        protected RazorTemplateCompiler Compiler => RazorServices.Compiler;

        public abstract TemplateDescriptor Template { get; }
        public TModel Model { get; set; }
        public string OutputPath { get; set; }

        public string AreaName => GenerationContext.AreaName;
        public string ModulePrefix => GenerationContext.TypescriptModulePrefix;


        public virtual async Task<Stream> GetOutputAsync()
        {
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

            return output;
        }

        public async Task GenerateAsync()
        {
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

        public override string ToString()
        {
            if (OutputPath != null)
            {
                return $"{Template.ToString()} => {OutputPath}";
            }
            return Template.ToString();
        }
    }
}
