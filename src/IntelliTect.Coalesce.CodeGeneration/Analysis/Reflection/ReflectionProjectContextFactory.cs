using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IntelliTect.Coalesce.CodeGeneration.Configuration;
using System.IO;
using Microsoft.DotNet.Cli.Utils;
using System.Text.RegularExpressions;

namespace IntelliTect.Coalesce.CodeGeneration.Analysis.Reflection
{
    public class ReflectionProjectContextFactory : IProjectContextFactory
    {
        public ProjectContext CreateContext(ProjectConfiguration projectConfig)
        {
            var context = new ReflectionProjectContext(projectConfig);
            context.AssemblyFileInfo = ResolveAssembly(projectConfig);

            return context;
        }

        private static FileInfo ResolveAssembly(ProjectConfiguration projectConfig)
        {
            var assemblyLocation = projectConfig.Assembly;
            var projectName = Path.GetFileNameWithoutExtension(projectConfig.ProjectFile);

            if (projectConfig.Build)
            {
                var args = new List<string>
                {
                    projectConfig.ProjectFile,
                };

                if (!string.IsNullOrWhiteSpace(projectConfig.Configuration))
                {
                    args.Add($"--configuration");
                    args.Add($"{projectConfig.Configuration}");
                }
                if (!string.IsNullOrWhiteSpace(projectConfig.Framework))
                {
                    args.Add($"--framework");
                    args.Add($"{projectConfig.Framework}");
                }

                Command command = Command.CreateDotNet("build", args);
                Console.WriteLine($"dotnet {command.CommandArgs}");

                var result = command
                    .CaptureStdOut()
                    .OnOutputLine(l =>
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine("    " + l);
                        Console.ResetColor();
                    })
                    .OnErrorLine(e => { Console.Error.WriteLine(e); throw new Exception(e); })
                    .Execute();

                if (result.ExitCode != 0)
                {
                    throw new Exception($"{command.CommandName} exited with code {result.ExitCode}");
                    //return null;
                }
                if (assemblyLocation == null)
                {
                    var outputLines = result.StdOut.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    var buildLine = outputLines.FirstOrDefault(l => l.Trim().StartsWith(projectName + " -> "));
                    assemblyLocation = Regex.Match(buildLine, @" \-\> (.*)").Groups[1].Value;
                }
            }

            if (assemblyLocation == null)
            {
                throw new FileNotFoundException($"Couldnt not determine assembly to analyze for project {projectConfig.ProjectFile}. " +
                    $"Please specify it using the {nameof(ProjectConfiguration.Assembly)} config property, " +
                    $"or set {nameof(ProjectConfiguration.Build)}=true");
            }

            if (Directory.Exists(assemblyLocation))
            {
                var assemblyCandidates = Directory
                    .EnumerateFiles(assemblyLocation)
                    .Where(f =>
                        Path.GetFileNameWithoutExtension(f) == projectName
                        && new[] { "exe", "dll" }.Contains(Path.GetExtension(f)))
                    .ToList();

                if (assemblyCandidates.Count == 0)
                {
                    throw new FileNotFoundException($"Couldn't find compiled assembly for project {projectConfig.ProjectFile}");
                }
                else if (assemblyCandidates.Count > 1)
                {
                    throw new FileNotFoundException(
                        $"Found too many candidates for compiled assembly for project {projectConfig.ProjectFile}:"
                        + string.Concat(assemblyCandidates.Select(path => $"\n    {path}"))
                    );
                }
                else
                {
                    assemblyLocation = assemblyCandidates.Single();
                }
            }

            return new FileInfo(assemblyLocation);
        }
    }
}
