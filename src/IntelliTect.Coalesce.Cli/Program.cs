using IntelliTect.Coalesce.CodeGeneration.Common;
using IntelliTect.Coalesce.CodeGeneration.Scripts;
using Microsoft.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Linq;
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

#if DEBUG
            if (args.Contains("--debug", StringComparer.OrdinalIgnoreCase))
            {
                Console.WriteLine($"Attach a debugger to processID: {System.Diagnostics.Process.GetCurrentProcess().Id}. Waiting...");
                var waitStep = 10;
                for (int i = 120; i > 0; i-= waitStep)
                {
                    if (System.Diagnostics.Debugger.IsAttached)
                    {
                        Console.WriteLine("Debugger attached.");
                        break;
                    }
                    if (i % 10 == 0) Console.WriteLine($"Waiting {i}...");
                    System.Threading.Thread.Sleep(1000 * waitStep);
                }
            }
#endif

            app.HelpOption("-h|--help");
            var dataContextClass = app.Option("-dc|--dataContext", "Data Context containing the classes to scaffold", CommandOptionType.SingleValue);
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
                Console.WriteLine("Staring Coalesce");
                Console.WriteLine("https://github.com/IntelliTect/Coalesce");
                Console.WriteLine();

                var model = new CommandLineGeneratorModel
                {
                    DataContextClass = dataContextClass.Value() ?? "",
                    Force = force.Value() != null && force.Value().ToLower() == "true",
                    RelativeFolderPath = relativeFolderPath.Value() ?? "",
                    OnlyGenerateFiles = onlyGenerateFiles.Value() != null && onlyGenerateFiles.Value().ToLower() == "true",
                    ValidateOnly = validateOnly.Value() != null && validateOnly.Value().ToLower() == "true",
                    AreaLocation = area.Value() ?? "",
                    TypescriptModulePrefix = module.Value() ?? "",
                    TargetNamespace = targetNamespace.Value() ?? ""
                };

                Console.WriteLine("Loading Projects");
                // Find the web project
                ProjectContext webContext = MsBuildProjectContextBuilder.CreateContext(webProject.Value());
                if (webContext == null) throw new ArgumentException("Web project or target namespace was not found.");

                // Find the data project
                ProjectContext dataContext = MsBuildProjectContextBuilder.CreateContext(dataProject.Value());
                if (dataContext == null) throw new ArgumentException("Data project was not found.");

                Console.WriteLine("");

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
