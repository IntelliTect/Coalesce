using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.Tests.Util;
using IntelliTect.Coalesce.Validation;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace IntelliTect.Coalesce.CodeGeneration.Tests
{
    public class CodeGenTestBase
    {
        protected GenerationExecutor BuildExecutor()
        {
            return new GenerationExecutor(
                new Configuration.CoalesceConfiguration
                {
                    WebProject = new Configuration.ProjectConfiguration
                    {
                        RootNamespace = "MyProject"
                    }
                },
                Microsoft.Extensions.Logging.LogLevel.Information
            );
        }

        protected async Task<T> ConfigureAndValidateSuite<T>(T suite)
            where T : IRootGenerator
        {
            var cwd = new DirectoryInfo(Directory.GetCurrentDirectory());
            var project = GetRepoRoot().GetDirectory("src/IntelliTect.Coalesce.CodeGeneration.Tests");

            var suiteName = suite.GetType().Name;

            var tfmAttr = Assembly.GetExecutingAssembly().GetCustomAttribute<TargetFrameworkAttribute>();

            suite = suite
                .WithOutputPath(Path.Combine(project.FullName, "out", tfmAttr.FrameworkName, suiteName));

            var validationResult = ValidateContext.Validate(suite.Model);
            Assert.False(
                validationResult.Any(r => r.IsError), 
                string.Join("\n", validationResult.Where(r => r.IsError))
            );

            // Writing the output to disk is needed for the TS compilation tests.
            // It is also useful so we can look at the output on diskto diagnose a failures.
            await suite.GenerateAsync();
            return suite;
        }

        protected Task AssertSuiteCSharpOutputCompiles(IRootGenerator suite)
            => GetCSharpCompilation(suite, assertSuccess: true);

        public static async Task<CSharpCompilation> GetCSharpCompilation(IRootGenerator suite, bool assertSuccess = true)
        {
            var generators = suite
                .GetGeneratorsFlattened()
                .OfType<IFileGenerator>()
                .Where(g => g.EffectiveOutputPath.EndsWith(".cs"))
                .ToList();

            var tasks = generators.Select(gen => (Generator: gen, Output: gen.GetOutputAsync()));
            await Task.WhenAll(tasks.Select(t => t.Output));

            var generatedFiles = tasks
                .Select((task) => CSharpSyntaxTree.ParseText(
                    SourceText.From(new StreamReader(task.Output.Result).ReadToEnd()),
                    path: task.Generator.EffectiveOutputPath
                ))
                .ToArray();

            return ReflectionRepositoryFactory.GetCompilation(generatedFiles, assertSuccess);
        }

        private static ProcessStartInfo GetShellExecStartInfo(string program, IEnumerable<string> args, string workingDirectory = null)
        {
            var arguments = args.ToList();
            var exeToRun = program;
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                // On Windows, the node executable is a .cmd file, so it can't be executed
                // directly (except with UseShellExecute=true, but that's no good, because
                // it prevents capturing stdio). So we need to invoke it via "cmd /c".
                exeToRun = "cmd";
                arguments.Insert(0, program);
                arguments.Insert(0, "/c");
            }

            var start = new ProcessStartInfo(exeToRun)
            {
                WorkingDirectory = workingDirectory,
                WindowStyle = ProcessWindowStyle.Hidden
            };
            foreach (var arg in arguments) start.ArgumentList.Add(arg);

            return start;
        }

        protected static async Task AssertTypescriptProjectCompiles(
            string tsConfigPath, 
            string workingDirectory, 
            string tsVersion
        )
        {
            var tsPath = Path.GetFullPath("./ts" + tsVersion);
            await Process
                .Start(GetShellExecStartInfo("npm", new[] { "i", "typescript@" + tsVersion, "--prefix", tsPath }))
                .WaitForExitAsync();

            var start = GetShellExecStartInfo(
                $"{tsPath}/node_modules/.bin/tsc",
                new List<string>
                {
                    "--project",
                    tsConfigPath,
                    "--noEmit"
                },
                workingDirectory
            );
            start.RedirectStandardOutput = true;
            start.RedirectStandardError = true;

            var typescriptProcess = Process.Start(start);

            // Collect stdout and stderr so we can report it in the event of a failure.
            var streamTasks = new[]
            {
                typescriptProcess.StandardOutput,
                typescriptProcess.StandardError,
            }.Select(async stream =>
            {
                var sb = new StringBuilder();
                while (!typescriptProcess.HasExited)
                {
                    var content = stream.ReadToEnd();
                    if (content != "") sb.Append(content);
                    await Task.Delay(10);
                }
                return sb.ToString();
            }).ToArray();

            await typescriptProcess.WaitForExitAsync();
            var streams = await Task.WhenAll(streamTasks);
            Assert.True(0 == typescriptProcess.ExitCode, $"Typescript {tsVersion}\n:" + string.Join("\n\n", streams));
        }

        protected DirectoryInfo GetRepoRoot()
        {
            return 
                // Normal usage (e.g. executing out of a /bin folder
                new DirectoryInfo(Directory.GetCurrentDirectory())
                    .FindFileInAncestorDirectory("Coalesce.sln")
                    ?.Directory
            ??
                // For Live Unit Testing, which makes a copy of the whole repo elsewhere.
                new DirectoryInfo(Directory.GetCurrentDirectory())
                    .FindDirectoryInAncestorDirectory("b");
        }
    }
}
