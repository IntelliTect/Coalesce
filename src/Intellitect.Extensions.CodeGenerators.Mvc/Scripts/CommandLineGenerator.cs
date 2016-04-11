using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.CodeGeneration;
using Microsoft.Extensions.CodeGeneration.CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace Intellitect.Extensions.CodeGenerators.Mvc.Scripts
{
    [Alias("scripts")]
    public class CommandLineGenerator : ICodeGenerator
    {
        public IServiceProvider ServiceProvider { get; }

        public CommandLineGenerator(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public async Task GenerateCode(CommandLineGeneratorModel model)
        {
            var generator = ActivatorUtilities.CreateInstance<ScriptsGenerator>(ServiceProvider);
            await generator.Generate(model);
        }
    }
}
