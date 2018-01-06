using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.CodeGeneration.Templating.Resolution;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.Extensions.Logging;

namespace IntelliTect.Coalesce.CodeGeneration.Templating.Razor
{
    /// <summary>
    /// Wrapper class to encapsulate all services needed for a RazorTemplateGenerator.
    /// This prevents constructors from being enormous due to all the services.
    /// </summary>
    public class RazorTemplateServices : GeneratorServices
    {
        public RazorTemplateServices(
            ReflectionRepository repository,
            ILoggerFactory loggerFactory,
            GenerationContext genContext,
            ITemplateResolver resolver,
            RazorTemplateCompiler compiler)
            : base(genContext.CoalesceConfiguration, loggerFactory)
        {
            ReflectionRepository = repository;
            GenerationContext = genContext;
            Resolver = resolver;
            Compiler = compiler;
        }

        public ReflectionRepository ReflectionRepository { get; }
        public GenerationContext GenerationContext { get; }
        public ITemplateResolver Resolver { get; }
        public RazorTemplateCompiler Compiler { get; }
    }
}
