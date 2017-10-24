using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntelliTect.Coalesce.CodeGeneration.Templating;
using IntelliTect.Coalesce.CodeGeneration.Templating.Resolution;
using IntelliTect.Coalesce.CodeGeneration.Configuration;
using IntelliTect.Coalesce.CodeGeneration.Utilities;
using Microsoft.Extensions.Logging;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{
    /// <summary>
    /// Wrapper class to encapsulate all services needed for a RazorTemplateGenerator.
    /// This prevents constructors from being enormous due to all the services.
    /// </summary>
    public class RazorTemplateServices
    {
        public RazorTemplateServices(
            ILoggerFactory loggerFactory,
            GenerationContext genContext,
            ITemplateResolver resolver,
            RazorTemplateCompiler compiler)
        {
            LoggerFactory = loggerFactory;
            GenerationContext = genContext;
            Resolver = resolver;
            Compiler = compiler;
        }

        public ILoggerFactory LoggerFactory { get; }
        public GenerationContext GenerationContext { get; }
        public ITemplateResolver Resolver { get; }
        public RazorTemplateCompiler Compiler { get; }
    }

    public abstract class RazorTemplateGenerator<TModel> : FileGenerator, IFileGenerator<TModel>
    {
        public RazorTemplateGenerator(RazorTemplateServices razorServices)
        {
            RazorServices = razorServices;
            Logger = razorServices.LoggerFactory.CreateLogger(GetType().Name);
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
                compiledTemplate.Model = this;
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
            if (OutputPath != null)
            {
                return $"{Template.ToString()} => {OutputPath}";
            }
            return Template.ToString();
        }
    }
}
