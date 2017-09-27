using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using IntelliTect.Coalesce.CodeGeneration.Analysis.Reflection;
using IntelliTect.Coalesce.CodeGeneration.Analysis.Roslyn;
using IntelliTect.Coalesce.CodeGeneration.Common;
using IntelliTect.Coalesce.CodeGeneration.Configuration;
using IntelliTect.Coalesce.CodeGeneration.Scripts;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;

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

            var attr = Assembly.GetEntryAssembly().GetCustomAttribute<TargetFrameworkAttribute>();

            Console.WriteLine($"Starting Coalesce, running under {attr.FrameworkName}");
            Console.WriteLine("https://github.com/IntelliTect/Coalesce");
            Console.WriteLine();

            app.HelpOption("-h|--help");
            var configFile = app.Argument("[config]", "coalesce.json configuration file that will drive generation.");

            string configFilePath = null;
            if (!string.IsNullOrWhiteSpace(configFile.Value))
            {
                configFilePath = configFile.Value;
                if (!File.Exists(configFilePath))
                {
                    throw new FileNotFoundException("Couldn't find Coalesce configuration file", Path.GetFullPath(configFilePath));
                }
            }
            else
            {
                var curDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
                const string configFileName = "coalesce.json";
                while (curDirectory != null)
                {
                    var matchingFiles = curDirectory.EnumerateFiles(configFileName);
                    if (matchingFiles.Any())
                    {
                        configFilePath = matchingFiles.First().FullName;
                        break;
                    }
                    curDirectory = curDirectory.Parent;
                }
            }
            if (configFilePath == null)
            {
                throw new FileNotFoundException("Couldn't find a coalesce.json configuration file");
            }
            Directory.SetCurrentDirectory(Path.GetDirectoryName(configFilePath));
            Console.WriteLine($"Working in '{Directory.GetCurrentDirectory()}', using '{Path.GetFileName(configFilePath)}'");


            var configRoot = new ConfigurationBuilder()
                .AddJsonFile(configFilePath)
                .Build();

            var config = configRoot.Get<CoalesceConfiguration>();




            var dataContextClass = app.Option("-dc|--dataContext", "Data Context containing the classes to scaffold", CommandOptionType.SingleValue);
            var validateOnly = app.Option("-vo|--validateOnly", "Validates the model but does not generate the models", CommandOptionType.SingleValue);
            var area = app.Option("-a|--area", "The area where the generated/scaffolded code should be placed", CommandOptionType.SingleValue);
            var module = app.Option("-m|--module", "The prefix to apply to the module name of the generated typescript files", CommandOptionType.SingleValue);
            var targetNamespace = app.Option("-ns|--namespace", "Target Namespace for the generated code", CommandOptionType.SingleValue);

            app.OnExecute(async () =>
            {
                // TODO: Move all configuration to coalesce.json, and remove these CLI parameters.
                var model = new CommandLineGeneratorModel
                {
                    DataContextClass = dataContextClass.Value() ?? "",
                    ValidateOnly = validateOnly.Value() != null && validateOnly.Value().ToLower() == "true",
                    AreaLocation = area.Value() ?? "",
                    TypescriptModulePrefix = module.Value() ?? "",
                    TargetNamespace = targetNamespace.Value() ?? ""
                };

                Console.WriteLine("Loading Projects");
                // Find the web project
                ProjectContext webContext = RoslynProjectContext.CreateContext(config.WebProject);
                if (webContext == null) throw new ArgumentException("Web project or target namespace was not found.");

                // Find the data project
                ProjectContext dataContext = RoslynProjectContext.CreateContext(config.DataProject);
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
                Console.WriteLine(ex.ToString());
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.ToString());
                }
            }
        }
    }
}
