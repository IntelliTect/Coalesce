// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Internal;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;
using NuGet.Frameworks;
using System.Text;
using IntelliTect.Coalesce.CodeGeneration.Common;
using Microsoft.DotNet.Cli.Utils;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.ProjectModel;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Web.CodeGeneration.DotNet;
using IntelliTect.Coalesce.CodeGeneration.Scripts;
using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;

namespace IntelliTect.Coalesce.CodeGeneration.Analysis.Roslyn
{
    public class RoslynProjectContext : ProjectContext
    {
        public List<ResolvedReference> CompilationAssemblies { get; set; }


        private TypeLocator _typeLocator;
        public override TypeLocator TypeLocator => _typeLocator = (_typeLocator ?? RoslynTypeLocator.FromProjectContext(this));

        public override ICollection<MetadataReference> GetTemplateMetadataReferences() => GetMetadataReferences();

        public static RoslynProjectContext CreateContext(string projectPath)
        {
            if (string.IsNullOrEmpty(projectPath))
                throw new ArgumentException($"{nameof(projectPath)} is required.");

            // Check for uri paths
            if (projectPath.StartsWith("file:")) projectPath = new Uri(projectPath).LocalPath;

            // Search up the folders from the path provided and find a project.json
            var foundProjectJsonPath = "";
            var foundProjectJsonFile = "";
            var curDirectory = new DirectoryInfo(projectPath);
            var rootDirectory = curDirectory.Root.FullName;
            while (curDirectory.FullName != rootDirectory)
            {
                var files = curDirectory.EnumerateFiles("*.csproj", SearchOption.TopDirectoryOnly);
                if (files.Count() == 1)
                {
                    foundProjectJsonPath = curDirectory.FullName;
                    foundProjectJsonFile = files.Single().FullName;
                    break;
                }
                curDirectory = curDirectory.Parent;
            }
            if (string.IsNullOrEmpty(foundProjectJsonPath)) throw new ArgumentException("Project path not found.");

            var configuration = "Debug";
#if RELEASE
            configuration = "Release";
#endif

            var tempFile = Path.GetTempFileName();
            var assembly = Assembly.GetExecutingAssembly();
            string sourceFile = assembly.GetName().Name + ".Microsoft.VisualStudio.Web.CodeGeneration.Tools.targets";
            var stream = assembly.GetManifestResourceStream(sourceFile);
            var output = File.OpenWrite(tempFile);
            stream.CopyTo(output);
            output.Close();

            var context = CreateContext(
                foundProjectJsonPath,
                foundProjectJsonFile,
                tempFile,
                configuration);

            File.Delete(tempFile);

            return context;
        }

        public static RoslynProjectContext CreateContext(string projectPath, string projectFile, string targetsLocation, string configuration = "Debug")
        {
            var errors = new List<string>();
            var tmpFile = Path.GetTempFileName();

            Console.WriteLine($"   {Path.GetFileName(projectFile)}: Restoring packages");
            var result = Command.CreateDotNet(
                "msbuild",
                new string[]
                {
                    projectPath,
                    "/nologo",
                    "/v:q", // Can't call 'dotnet restore' because it ignores verbosity: https://github.com/dotnet/cli/issues/5989
                    "/t:restore"
                })
                .OnOutputLine(Console.WriteLine)
                .OnErrorLine(e => { Console.Error.WriteLine(e); errors.Add(e); })
                .Execute();
            if (result.ExitCode != 0) errors.Add($"Restore packages exited with code {result.ExitCode}");

            if (!errors.Any())
            {
                Console.WriteLine($"   {Path.GetFileName(projectFile)}: Evaluating & building dependencies");
                result = Command.CreateDotNet(
                    "msbuild",
                    new string[]
                    {
                        projectPath,
                        "/nologo",
                        "/v:q",
                        "/t:EvaluateProjectInfoForCodeGeneration",
                        $"/p:OutputFile={tmpFile};CustomBeforeMicrosoftCSharpTargets={targetsLocation};Configuration={configuration};BuildProjectReferences=true"
                    })
                    .OnOutputLine(Console.WriteLine)
                    .OnErrorLine(e => { Console.Error.WriteLine(e); errors.Add(e); })
                    .Execute();
                if (result.ExitCode != 0) errors.Add($"Evaluating & building dependencies exited with code {result.ExitCode}");
            }

            if (errors.Any())
            {
                var errorMsg = $"Failed to get Project Context for {projectPath}: {string.Concat(errors.Select(e => $"{Environment.NewLine}    {e}"))}";
                throw new InvalidOperationException(errorMsg);
            }

            try
            {
                var lines = File.ReadAllLines(tmpFile);
                var references = new List<ResolvedReference>();

                foreach (var line in lines)
                {
                    references.Add(new ResolvedReference(Path.GetFileName(line), line));
                }

                return new RoslynProjectContext
                {
                    CompilationAssemblies = references,
                    ProjectFullPath = projectPath,
                    ProjectFilePath = projectFile
                };
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to read the BuildContext information.", ex);
            }
        }

        public ICollection<MetadataReference> GetMetadataReferences()
        {
            var references = new List<MetadataReference>();

            // Todo: When the target app references scaffolders as nuget packages rather than project references,
            // we need to ensure all dependencies for compiling the generated razor template.
            // This requires further thinking for custom scaffolders because they may be using
            // some other references which are not available in any of these closures.
            // As the above comment, the right thing to do here is to use the dependency closure of
            // the assembly which has the template.
            var exports = CompilationAssemblies;

            if (exports != null)
            {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var export in exports)
                {
                    var loadedAssembly = assemblies.FirstOrDefault(a => !a.IsDynamic && Path.GetFileName(a.Location) == export.Name);

                    // If the referenced assembly is a core Coalesce assembly, we want to use the assembly that is loaded into the current AppDomain,
                    // as opposed to the path referenced in the 'export' object. export.ResolvedPath is going to be in the output directory of the target web project,
                    // which isn't going to have the most up to date Coalesce code unless the web project is rebuild before running coalesce generation.
                    // The web project very likely can't be rebuilt when someone is regenerating coalesce (because generated code may not match up with the current data model),
                    // and besides, that's really terrible to require recompilation of the web project before being able to regenerate it.

                    if (loadedAssembly != null && loadedAssembly.FullName.StartsWith($"{nameof(IntelliTect)}.{nameof(Coalesce)}"))
                    {
                        references.Add(MetadataReference.CreateFromFile(loadedAssembly.Location));
                    }
                    else
                    {
                        references.AddRange(export.GetMetadataReference(throwOnError: true));
                    }

                    // TODO: Coalesce has a bit of a chicken-and-egg problem. If essential DLLs aren't in the output folder of the web project (someone ran a Clean Solution, for eg),
                    // code generation will fail. It can also be true that the web project is in a state that can't be built without running generation first.
                    // So, something else that this code needs to do is recoginze that the path pointed to by export.ResolvedPath doesn't exist,
                    // and then attempt to find that DLL elsewhere.
                    // The vast majority of references will resolve to the .nuget package cache, which is fine. But, for those that don't,
                    // we need to make attempts to find them elsewhere.
                    // The most common case for this would be finding the Data Project dll directly in the output of the data project (or, perhaps even building the data project ourselves).
                }
            }

            return references;
        }
    }
}
