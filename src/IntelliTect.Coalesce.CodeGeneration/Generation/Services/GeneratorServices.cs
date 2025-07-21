using Microsoft.Extensions.Logging;
using IntelliTect.Coalesce.CodeGeneration.Configuration;
using IntelliTect.Coalesce.TypeDefinition;

namespace IntelliTect.Coalesce.CodeGeneration.Generation;

/// <summary>
/// Wrapper class to encapsulate all services needed for a Generator.
/// This prevents constructors from being enormous due to all the services.
/// </summary>
public class GeneratorServices
{
    public GeneratorServices(
        CoalesceConfiguration config,
        ReflectionRepository reflectionRepository,
        GenerationContext generationContext,
        ILoggerFactory loggerFactory)
    {
        CoalesceConfiguration = config;
        ReflectionRepository = reflectionRepository;
        GenerationContext = generationContext;
        LoggerFactory = loggerFactory;
    }
    
    public CoalesceConfiguration CoalesceConfiguration { get; }
    public ReflectionRepository ReflectionRepository { get; }
    public GenerationContext GenerationContext { get; }
    public ILoggerFactory LoggerFactory { get; }
}
