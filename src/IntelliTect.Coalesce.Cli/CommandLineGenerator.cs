using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Web.CodeGeneration;
using Microsoft.VisualStudio.Web.CodeGeneration.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using IntelliTect.Coalesce.CodeGeneration.Knockout;

namespace IntelliTect.Coalesce.CodeGeneration.Scripts
{
    [Alias("scripts")]
    public class CommandLineGenerator : ICodeGenerator
    {
        private ProjectContext _webProject;
        private ProjectContext _dataProject;

        public CommandLineGenerator(ProjectContext webProject, ProjectContext dataProject)
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
