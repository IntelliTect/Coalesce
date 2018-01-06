using System;
using Microsoft.Extensions.Logging;
using IntelliTect.Coalesce.CodeGeneration.Configuration;

namespace IntelliTect.Coalesce.CodeGeneration.Generation
{
    public class CompositeGeneratorServices : GeneratorServices
    {
        public CompositeGeneratorServices(
            CoalesceConfiguration config,
            IServiceProvider serviceProvider,
            ILoggerFactory loggerFactory)
            : base(config, loggerFactory)
        {
            ServiceProvider = serviceProvider;
        }
        
        public IServiceProvider ServiceProvider { get; }
    }
}
