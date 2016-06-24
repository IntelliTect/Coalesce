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
        private ProjectContext _webProject;
        private ProjectContext _dataProject;
        private ProjectContext _cliProject;

        public CommandLineGenerator(ProjectContext webProject, ProjectContext dataProject, ProjectContext cliProject)
        {
            _webProject = webProject;
            _dataProject = dataProject;
            _cliProject = cliProject;
        }

        public async Task GenerateCode(CommandLineGeneratorModel model)
        {
            var generator = new ScriptsGenerator(_webProject, _dataProject, _cliProject);
            await generator.Generate(model);
        }
    }
}
