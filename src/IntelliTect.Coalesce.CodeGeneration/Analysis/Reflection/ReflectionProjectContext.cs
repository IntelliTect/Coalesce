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
using PC = IntelliTect.Coalesce.CodeGeneration.Configuration.ProjectConfiguration;

namespace IntelliTect.Coalesce.CodeGeneration.Analysis.Reflection
{
    public class ReflectionProjectContext : ProjectContext
    {
        public FileInfo AssemblyFileInfo { get; private set; }

        public ReflectionProjectContext(ProjectConfiguration projectConfig) : base(projectConfig)
        {
            throw new NotImplementedException("ReflectionProjectContext is in a somewhat unfinished state." +
                "If reflection-based generation is needed, it will probably need some work before it is robust enough for use..");
        }

        public override TypeLocator TypeLocator => new ReflectionTypeLocator(this);

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
                    $"Please specify it using the {nameof(PC.Assembly)} config property, " +
                    $"or set {nameof(PC.Build)}=true");
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
