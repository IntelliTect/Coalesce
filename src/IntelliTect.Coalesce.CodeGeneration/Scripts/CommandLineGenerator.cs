using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.VisualStudio.Web.CodeGeneration;
using Microsoft.VisualStudio.Web.CodeGeneration.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using Microsoft.Extensions.ProjectModel;

namespace IntelliTect.Coalesce.CodeGeneration.Scripts
{
    [Alias("scripts")]
    public class CommandLineGenerator : ICodeGenerator
    {
        private IProjectContext _webProject;
        private IProjectContext _dataProject;

        public CommandLineGenerator(IProjectContext webProject, IProjectContext dataProject)
        {
            _webProject = webProject;
            _dataProject = dataProject;
        }

        public async Task GenerateCode(CommandLineGeneratorModel model)
        {
            var generator = new ScriptsGenerator(_webProject, _dataProject);
            await generator.Generate(model);

            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press Enter to quit");
                Console.Read();
            }

        }
    }
}
