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
using System.Runtime.Versioning;

namespace IntelliTect.Coalesce.CodeGeneration.Analysis.Reflection
{
    public class ReflectionProjectContext : ProjectContext
    {
        public override TypeLocator TypeLocator => new ReflectionTypeLocator(this);

        public FileInfo AssemblyFileInfo { get; private set; }
        public Assembly Assembly { get; private set; }

        public override ICollection<MetadataReference> GetTemplateMetadataReferences()
        {
            // Force load required assemblies into the current appdomain.
            // This was taken from commit 09c9be3, RazorTemplating.cs
            // Load Microsoft.CSharp.RuntimeBinder
            new Microsoft.CSharp.RuntimeBinder.RuntimeBinderException();
            // Load Micosoft.AspNetCore.Html
            new Microsoft.AspNetCore.Html.HtmlString("");



            var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Select(asm => asm.GetName());

            var outputAssemblies = AssemblyFileInfo.Directory
                .EnumerateFiles()
                .Where(f => new[] { "exe", "dll" }.Contains(f.Extension))
                .Select(f => AssemblyName.GetAssemblyName(f.FullName));

            return loadedAssemblies
                .Concat(outputAssemblies)
                // Get only one reference for each assembly. There may be duplicates because we pulled from
                // both our current AppDomain and the target assembly's output dir.
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

            //var asm = Assembly.ReflectionOnlyLoadFrom(assemblyInfo.FullName);
            //var attrs2 = asm.GetCustomAttributes(typeof(System.Runtime.Versioning.TargetFrameworkAttribute), false);


            var context = new ReflectionProjectContext
            {
                ProjectFilePath = projectFileAbsPath,
                AssemblyFileInfo = assemblyInfo,
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
                        "/nologo",
                        $"-o \"{projectConfig.Build.Output}\"",
                        "-f netcoreapp20"
                    };
                }
                else
                {
                    args = new[] {
                        projectConfig.ProjectFile,
                        "/nologo",
                    };
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
                    .OnErrorLine(e => throw new Exception(e))
                    .Execute();

                if (result.ExitCode != 0)
                {
                    //throw new Exception($"{command.CommandName} exited with code {result.ExitCode}");
                    return null;
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
