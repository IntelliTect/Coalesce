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
            : base(genContext.CoalesceConfiguration, repository, genContext, loggerFactory)
        {
            Resolver = resolver;
            Compiler = compiler;
        }

        public ITemplateResolver Resolver { get; }
        public RazorTemplateCompiler Compiler { get; }
    }
}
