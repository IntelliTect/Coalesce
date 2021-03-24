using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using IntelliTect.Coalesce.CodeGeneration.Analysis;
using IntelliTect.Coalesce.CodeGeneration.Configuration;
using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.CodeGeneration.Utilities;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

// ReSharper disable UnassignedGetOnlyAutoProperty

namespace IntelliTect.Coalesce.Cli
{
    [HelpOption]
    public class Program
    {
        [Option(CommandOptionType.NoValue,
            Description = "Wait for a debugger to be attached before starting generation", LongName = "debug",
            ShortName = "d")]
        public bool Debug { get; }

        [Argument(0, "config", Description =
            "Path to a coalesce.json configuration file that will drive generation.  If not specified, it will search in current folder.")]
        public string ConfigFile { get; }

        [Option(CommandOptionType.SingleValue, ShortName = "v", LongName = "verbosity",
            Description = "Output verbosity. Options are Trace, Debug, Information, Warning, Error, Critical, None.")]
        // TODO: Change this type to be the Enum once Nate McMaster ships v2.2.0 of his library.
        public string LogLevelOption { get; }

        private static Task<int> Main(string[] args)
        {
            ApplicationTimer.Stopwatch.Start();
            return CommandLineApplication.ExecuteAsync<Program>(args);
        }

        private async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
#if DEBUG
            if (Debug) WaitForDebugger();
#endif

            // Get the target framework (e.g. ".NETCoreApp,Version=v2.0") that Coalesce was compiled against.
            // I added this originally for debugging, but its kinda nice to show regardless.
            var attr = Assembly.GetEntryAssembly().GetCustomAttribute<TargetFrameworkAttribute>();

            // This reflects the version of the nuget package.
            string version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;

            Console.WriteLine($"Starting Coalesce {version}, running under {attr.FrameworkName}");
            Console.WriteLine("https://github.com/IntelliTect/Coalesce");
            Console.WriteLine();


            FileInfo configFile = LocateConfigFile(ConfigFile);

            CoalesceConfiguration config;

            using (var reader = new StreamReader(configFile.FullName))
            using (var jsonReader = new JsonTextReader(reader))
            {
                var serializer = new JsonSerializer();
                config = serializer.Deserialize<CoalesceConfiguration>(jsonReader);
            }

            // Must go AFTER we load in the config file, since if the config file was a relative path, changing this ruins that.
            Directory.SetCurrentDirectory(configFile.DirectoryName);

            Console.WriteLine(
                $"Working in '{Directory.GetCurrentDirectory()}', using '{Path.GetFileName(configFile.FullName)}'");

            if (!Enum.TryParse(LogLevelOption, true, out LogLevel logLevel)) logLevel = LogLevel.Information;

            // TODO: dynamic resolution of the specific generator.
            // For now, we hard-reference all of them and then try and match one of them.
            // This may ultimately be the best approach in the long run, since it lets us easily do partial matching as below:
            var rootGeneratorName = config.RootGenerator ?? "Knockout";
            var rootGenerators = new[]
            {
                typeof(CodeGeneration.Vue.Generators.VueSuite),
                typeof(CodeGeneration.Knockout.Generators.KnockoutSuite),
            };

            Type rootGenerator =
                   rootGenerators.FirstOrDefault(t => t.FullName == rootGeneratorName)
                ?? rootGenerators.FirstOrDefault(t => t.Name == rootGeneratorName)
                ?? rootGenerators.SingleOrDefault(t => t.FullName.Contains(rootGeneratorName));

            if (rootGenerator == null)
            {
                Console.Error.WriteLine($"Couldn't find a root generator that matches {rootGeneratorName}");
                Console.Error.WriteLine($"Valid root generators are: {string.Join(",", rootGenerators.Select(g => g.FullName))}");
                return -1;
            }

            var executor = new GenerationExecutor(config, logLevel);
            try
            {
                await executor.GenerateAsync(rootGenerator);
            }
            catch (ProjectAnalysisException e)
            {
                // Only write the message here, not a full exception.ToString() w/ stack trace
                executor.Logger.LogError(e.Message);
                return -1;
            }
            catch (Exception e)
            {
                executor.Logger.LogError(e.ToString());
                return -1;
            }

            return 0;
        }


        private static void WaitForDebugger()
        {
            Console.WriteLine($"Attach a debugger to processID: {Process.GetCurrentProcess().Id}. Waiting...");
            var waitStep = 10;
            for (var i = 60; i > 0; i -= waitStep)
            {
                if (Debugger.IsAttached)
                {
                    Console.WriteLine("Debugger attached.");
                    break;
                }

                Console.WriteLine($"Waiting {i}...");
                Thread.Sleep(1000 * waitStep);
            }
        }

        private static FileInfo LocateConfigFile(string explicitLocation)
        {
            FileInfo file = null;
            if (!string.IsNullOrWhiteSpace(explicitLocation))
            {
                file = new FileInfo(explicitLocation);
                if (!file.Exists)
                    throw new FileNotFoundException("Couldn't find Coalesce configuration file",
                        file.FullName);
                return file;
            }

            const string configFileName = "coalesce.json";
            file = new DirectoryInfo(Directory.GetCurrentDirectory())
                .FindFileInAncestorDirectory(configFileName);

            if (file != null) return file;

            throw new FileNotFoundException("Couldn't locate a coalesce.json configuration file");
        }
    }
}