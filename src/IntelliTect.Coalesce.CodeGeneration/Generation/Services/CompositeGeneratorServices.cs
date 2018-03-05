using System;
using Microsoft.Extensions.Logging;
using IntelliTect.Coalesce.CodeGeneration.Configuration;
using IntelliTect.Coalesce.TypeDefinition;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{
    public class CompositeGeneratorServices : GeneratorServices
    {
        public CompositeGeneratorServices(
            CoalesceConfiguration config,
            ReflectionRepository reflectionRepository,
            GenerationContext generationContext,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory)
            : base(config, reflectionRepository, generationContext, loggerFactory)
        {
            ServiceProvider = serviceProvider;
        }
        
        public IServiceProvider ServiceProvider { get; }
    }
}
