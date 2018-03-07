using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntelliTect.Coalesce.CodeGeneration.Templating;
using IntelliTect.Coalesce.CodeGeneration.Templating.Resolution;
using IntelliTect.Coalesce.CodeGeneration.Templating.Razor;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{
    public abstract class RazorTemplateGenerator<TModel> : FileGenerator, IFileGenerator<TModel>
    {
        public RazorTemplateGenerator(RazorTemplateServices razorServices) 
            : base(razorServices)
        {
            RazorServices = razorServices;
        }

        public RazorTemplateServices RazorServices { get; }

        public GenerationContext GenerationContext => RazorServices.GenerationContext;
        protected ITemplateResolver Resolver => RazorServices.Resolver;
        protected RazorTemplateCompiler Compiler => RazorServices.Compiler;

        public abstract TemplateDescriptor Template { get; }
        public TModel Model { get; set; }

        public string AreaName => GenerationContext.AreaName;
        public string ModulePrefix => GenerationContext.TypescriptModulePrefix;


        public override async Task<Stream> GetOutputAsync()
        {
            Stream output;
            try
            {
                var resolvedTemplate = this.Resolver.Resolve(Template);
                var compiledTemplate = await this.Compiler.GetTemplateInstance(resolvedTemplate);
                compiledTemplate.SetModel(this);
                output = await compiledTemplate.GetOutputAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"There was an error running the template {Template}: {ex.Message}", ex);
            }

            return output;
        }

        public override string ToString()
        {
            if (EffectiveOutputPath != null)
            {
                return $"{Template.ToString()} => {EffectiveOutputPath}";
            }
            return Template.ToString();
        }
    }
}
