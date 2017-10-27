using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.CodeGeneration.Templating.Resolution;
using Microsoft.Extensions.Logging;

namespace IntelliTect.Coalesce.CodeGeneration.Templating.Razor
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
}
