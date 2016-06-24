using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Web.CodeGeneration;
using Microsoft.VisualStudio.Web.CodeGeneration.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.DotNet.ProjectModel;

namespace Intellitect.Extensions.CodeGenerators.Mvc.Scripts
{
    [Alias("scripts")]
    public class CommandLineGenerator : ICodeGenerator
    {
        //public IServiceProvider ServiceProvider { get; }
        private ProjectContext _webProject;
        private ProjectContext _dataProject;

        //public CommandLineGenerator(IServiceProvider serviceProvider)
        public CommandLineGenerator(ProjectContext webProject, ProjectContext dataProject)
        {
            _webProject = webProject;
            _dataProject = dataProject;
            //ServiceProvider = serviceProvider;
        }

        public async Task GenerateCode(CommandLineGeneratorModel model)
        {
            //var generator = ActivatorUtilities.CreateInstance<ScriptsGenerator>(ServiceProvider);
            var generator = new ScriptsGenerator(_webProject, _dataProject);
            await generator.Generate(model);
        }
    }
}
