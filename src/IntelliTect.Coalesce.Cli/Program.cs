using IntelliTect.Coalesce.CodeGeneration.Common;
using IntelliTect.Coalesce.CodeGeneration.Scripts;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.ProjectModel;
using System;
using System.IO;
using System.Reflection;

namespace IntelliTect.Coalesce.Cli
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var app = new CommandLineApplication(false)
            {
                Name = "Coalesce"
            };

            app.HelpOption("-h|--help");
            var dataContextClass = app.Option("-dc|--dataContext", "Data Context containing the classes to scaffold", CommandOptionType.SingleValue);
            var dataAssembly = app.Option("-da|--dataassembly", "Name of the data assembly", CommandOptionType.SingleValue);
            var force = app.Option("-f|--force", "Use this option to overwrite existing files", CommandOptionType.SingleValue);
            var relativeFolderPath = app.Option("-outDir|--relativeFolderPath", "Specify the relative output folder path from project where the file needs to be generated, if not specified, file will be generated in the project folder", CommandOptionType.SingleValue);
            var onlyGenerateFiles = app.Option("-filesOnly|--onlyGenerateFiles", "Will only generate the file output and will not restore any of the packages", CommandOptionType.SingleValue);
            var validateOnly = app.Option("-vo|--validateOnly", "Validates the model but does not generate the models", CommandOptionType.SingleValue);
            var area = app.Option("-a|--area", "The area where the generated/scaffolded code should be placed", CommandOptionType.SingleValue);
            var module = app.Option("-m|--module", "The prefix to apply to the module name of the generated typescript files", CommandOptionType.SingleValue);
            var webProject = app.Option("-wp|--webproject", "Relative path to the web project; if empty will search up from current folder for first project.json", CommandOptionType.SingleValue);
            var dataProject = app.Option("-dp|--dataproject", "Relative path to the data project", CommandOptionType.SingleValue);
            var targetNamespace = app.Option("-ns|--namespace", "Target Namespace for the generated code", CommandOptionType.SingleValue);

            app.OnExecute(async () =>
            {
                var model = new CommandLineGeneratorModel
                {
                    DataContextClass = dataContextClass.Value() ?? "",
                    Force = force.Value() != null && force.Value().ToLower() == "true",
                    RelativeFolderPath = relativeFolderPath.Value() ?? "",
                    OnlyGenerateFiles = onlyGenerateFiles.Value() != null && onlyGenerateFiles.Value().ToLower() == "true",
                    ValidateOnly = validateOnly.Value() != null && validateOnly.Value().ToLower() == "true",
                    AreaLocation = area.Value() ?? "",
                    TypescriptModulePrefix = module.Value() ?? "",
                    TargetNamespace = targetNamespace.Value() ?? "",
                    DataProject = dataProject.Value() ?? "",
                    DataAssembly = dataAssembly.Value() ?? ""
                };

                // Find the web project
                //ProjectContext webContext = DependencyProvider.ProjectContext(webProject.Value());
                ProjectContext webContext = new ProjectContext(webProject.Value(), model.TargetNamespace);
                if (webContext == null) throw new ArgumentException("Web project or target namespace was not found.");

                // Find the data project
                //ProjectContext dataContext = DependencyProvider.ProjectContext(dataProject.Value());
                // Get the data assembly
                ProjectContext dataContext = new ProjectContext(model.DataProject, model.DataAssembly);
                if (dataContext == null) throw new ArgumentException("Data project was not found.");

                // Check the target namespace.
                if (string.IsNullOrWhiteSpace(model.TargetNamespace)) throw new ArgumentException("Target Namespace was not found.");


                CommandLineGenerator generator = new CommandLineGenerator(webContext, dataContext);

                await generator.GenerateCode(model);

                return 0;
            });

            try
            {
                app.Execute(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                    Console.WriteLine(ex.InnerException.StackTrace);
                }
            }
        }
    }
}
