using Microsoft.VisualStudio.Web.CodeGeneration;
using Microsoft.VisualStudio.Web.CodeGeneration.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intellitect.Extensions.CodeGenerators.Mvc.Documentation
{
    [Alias("scaffoldingDocs")]
    public class CommandLineGenerator : ICodeGenerator
    {
        public IServiceProvider ServiceProvider { get; }

        public CommandLineGenerator(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public async Task GenerateCode(CommandLineGeneratorModel model)
        {
            var generator = ActivatorUtilities.CreateInstance<DocumentationGenerator>(ServiceProvider);
            await generator.Generate(model);
        }
    }
}
