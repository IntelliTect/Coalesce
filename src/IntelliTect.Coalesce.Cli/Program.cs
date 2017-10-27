using IntelliTect.Coalesce.CodeGeneration.Configuration;
using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.CodeGeneration.Knockout.Generators;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
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
#if DEBUG
            if (args.Contains("--debug", StringComparer.OrdinalIgnoreCase))
            {
                WaitForDebugger();
            }
#endif

            var app = new CommandLineApplication(false)
            {
                Name = "Coalesce"
            };

            // Get the target framework (e.g. ".NETCoreApp,Version=v2.0") that Coalesce was compiled against.
            // I added this originally for debugging, but its kinda nice to show regardless.
            var attr = Assembly.GetEntryAssembly().GetCustomAttribute<TargetFrameworkAttribute>();

            Console.WriteLine($"Starting Coalesce, running under {attr.FrameworkName}");
            Console.WriteLine("https://github.com/IntelliTect/Coalesce");
            Console.WriteLine();

            var configFile = app.Argument("[config]", "coalesce.json configuration file that will drive generation.");
            
            app.OnExecute(async () =>
            {

                var configFilePath = LocateConfigFile(configFile.Value);
                Directory.SetCurrentDirectory(Path.GetDirectoryName(configFilePath));
                Console.WriteLine($"Working in '{Directory.GetCurrentDirectory()}', using '{Path.GetFileName(configFilePath)}'");

                var configRoot = new ConfigurationBuilder()
                    .AddJsonFile(configFilePath)
                    .Build();

                var config = configRoot.Get<CoalesceConfiguration>();

                var executor = new GenerationExecutor(config);
                await executor.GenerateAsync<KnockoutSuite>();

                if (Debugger.IsAttached)
                {
                    Console.WriteLine("Press Enter to quit");
                    Console.Read();
                }

                return 0;
            });

            try
            {
                app.Execute(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void WaitForDebugger()
        {
            Console.WriteLine($"Attach a debugger to processID: {System.Diagnostics.Process.GetCurrentProcess().Id}. Waiting...");
            var waitStep = 10;
            for (int i = 120; i > 0; i -= waitStep)
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

        private static string LocateConfigFile(string explicitLocation)
        {
            if (!string.IsNullOrWhiteSpace(explicitLocation))
            {
                if (!File.Exists(explicitLocation))
                {
                    throw new FileNotFoundException("Couldn't find Coalesce configuration file", Path.GetFullPath(explicitLocation));
                }
                return explicitLocation;
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
                        return matchingFiles.First().FullName;
                    }
                    curDirectory = curDirectory.Parent;
                }
            }

            throw new FileNotFoundException("Couldn't locate a coalesce.json configuration file");
        }
    }
}
