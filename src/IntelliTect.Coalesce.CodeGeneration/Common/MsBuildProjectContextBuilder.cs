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
using Microsoft.Extensions.ProjectModel.Resolution;
using IntelliTect.Coalesce.CodeGeneration.Common;
using Microsoft.DotNet.Cli.Utils;

namespace IntelliTect.Coalesce.CodeGeneration.Common
{
    public class MsBuildProjectContextBuilder
    {
        public static ProjectContext CreateContext(string projectPath)
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

        public static ProjectContext CreateContext(string projectPath, string projectFile, string targetsLocation, string configuration = "Debug")
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

                return new ProjectContext
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
    }
}
