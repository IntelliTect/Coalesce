using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using IntelliTect.Coalesce.CodeGeneration.Configuration;
using Microsoft.DotNet.Cli.Utils;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace IntelliTect.Coalesce.CodeGeneration.Analysis.Reflection
{
    public class ReflectionProjectContext : ProjectContext
    {
        public override TypeLocator TypeLocator => new ReflectionTypeLocator(this);

        public FileInfo AssemblyInfo { get; private set; }
        public Assembly Assembly { get; private set; }

        public override ICollection<MetadataReference> GetTemplateMetadataReferences()
        {
            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Select(asm => asm.GetName());

            var outputAssemblies = Directory
                .EnumerateFiles(Path.GetDirectoryName(Assembly.Location))
                .Where(f => new[] { "exe", "dll" }.Contains(Path.GetExtension(f)))
                .Select(path => AssemblyName.GetAssemblyName(path));

            return loadedAssemblies
                .Concat(outputAssemblies)
                // Get only one reference for each assembly.
                .GroupBy(name => name.FullName)
                .Select(group => group.First())
                .Select(name => new Uri(name.CodeBase).LocalPath)
                .Select(path => MetadataReference.CreateFromFile(path))
                .Cast<MetadataReference>()
                .ToList();

        }


        public static ReflectionProjectContext CreateContext(ProjectConfiguration projectConfig)
        {
            var assemblyInfo = ResolveAssembly(projectConfig);
            var projectFileAbsPath = Path.GetFullPath(projectConfig.ProjectFile);
            var context = new ReflectionProjectContext
            {
                ProjectFullPath = Path.GetDirectoryName(projectFileAbsPath),
                ProjectFilePath = projectFileAbsPath,
                AssemblyInfo = assemblyInfo,
                Assembly = Assembly.LoadFile(assemblyInfo.FullName)
            };


            return context;
        }

        private static FileInfo ResolveAssembly(ProjectConfiguration projectConfig)
        {
            var assemblyLocation = projectConfig.Assembly;
            var projectName = Path.GetFileNameWithoutExtension(projectConfig.ProjectFile);

            if (projectConfig.Build != null)
            {
                string[] args = null;
                if (!string.IsNullOrWhiteSpace(projectConfig.Build.Args))
                {
                    args = projectConfig.Build.Args.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                }
                else if (!string.IsNullOrWhiteSpace(projectConfig.Build.Output))
                {
                    if (string.IsNullOrWhiteSpace(assemblyLocation))
                    {
                        assemblyLocation = projectConfig.Build.Output;
                    }

                    args = new[]
                    {
                        projectConfig.ProjectFile,
                        $"-o \"{projectConfig.Build.Output}\"",
                        "-f netcoreapp20"
                    };
                }
                else
                {
                    args = new[] {
                        projectConfig.ProjectFile
                    };
                }

                Command command = Command.CreateDotNet("build", args);
                Console.WriteLine($"Running {command.CommandName} {command.CommandArgs}");

                var result = command
                    .CaptureStdOut()
                    .OnOutputLine(Console.WriteLine)
                    .OnErrorLine(e => throw new Exception(e))
                    .Execute();

                if (result.ExitCode != 0)
                {
                    throw new Exception($"{command.CommandName} exited with code {result.ExitCode}");
                }
                if (assemblyLocation == null)
                {
                    var outputLines = result.StdOut.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                    var buildLine = outputLines.FirstOrDefault(l => l.Trim().StartsWith(projectName + " -> "));
                    assemblyLocation = Regex.Match(buildLine, @" \-\> (.*)").Groups[1].Value;
                }


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
