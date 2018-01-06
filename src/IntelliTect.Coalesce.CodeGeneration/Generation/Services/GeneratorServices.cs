using Microsoft.Extensions.Logging;
using IntelliTect.Coalesce.CodeGeneration.Configuration;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{
    /// <summary>
    /// Wrapper class to encapsulate all services needed for a Generator.
    /// This prevents constructors from being enormous due to all the services.
    /// </summary>
    public class GeneratorServices
    {
        public GeneratorServices(
            CoalesceConfiguration config,
            ILoggerFactory loggerFactory)
        {
            CoalesceConfiguration = config;
            LoggerFactory = loggerFactory;
        }
        
        public CoalesceConfiguration CoalesceConfiguration { get; }
        public ILoggerFactory LoggerFactory { get; }
    }
}
