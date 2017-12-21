// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// Modified heavily by IntelliTect from: 
// https://github.com/aspnet/Scaffolding/blob/463e4da9cd29e564a1ba9b952d55c0714878567c/src/Ext.ProjectModel.MsBuild.Sources/MsBuildProjectContextBuilder.cs
// https://github.com/aspnet/Scaffolding/blob/463e4da9cd29e564a1ba9b952d55c0714878567c/src/VS.Web.CG.Tools/TargetInstaller.cs


using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Internal;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.ProjectModel;
using Newtonsoft.Json;
using Microsoft.DotNet.Cli.Utils;
using System.Linq;
using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using Microsoft.Extensions.Logging;

namespace IntelliTect.Coalesce.CodeGeneration.Analysis.MsBuild
{
    /// <summary>
    /// Builds an MsBuildProjectContext for a project, which provides project metadata useful for code generation.
    /// </summary>
    public class MsBuildProjectContextBuilder
    {
        private ProjectContext _context;
        public string TargetsLocation { get; private set; }
        public string Configuration { get; private set; } = "Release";
        public string Framework { get; private set; } = null;
        public ILogger Logger { get; }

        public MsBuildProjectContextBuilder(ILogger logger, ProjectContext context)
        {
            Logger = logger;
            _context = context;
            if (_context.Configuration != null) Configuration = _context.Configuration;
            if (_context.Framework != null) Framework = _context.Framework;
        }

        public MsBuildProjectContextBuilder WithBuildConfiguration(string configuration)
        {
            Configuration = configuration;
            return this;
        }

        public MsBuildProjectContextBuilder WithTargetFramework(string tfm)
        {
            Framework = tfm;
            return this;
        }

        public MsBuildProjectContextBuilder InstallTargets(string targetLocation)
        {
            if (string.IsNullOrEmpty(targetLocation))
            {
                throw new ArgumentNullException(nameof(targetLocation));
            }

            TargetsLocation = targetLocation;

            var toolType = typeof(MsBuildProjectContextBuilder);
            var thisAssembly = toolType.GetTypeInfo().Assembly;
            var toolNamespace = toolType.Namespace;

            var baseResourceName = $"{thisAssembly.GetName().Name}.Analysis.MsBuild.Targets";

            var files = new[]
            {
                "Imports.targets",
                "build/IntelliTect.Coalesce.CodeGeneration.Analysis.MsBuild.targets",
                "buildMultiTargeting/IntelliTect.Coalesce.CodeGeneration.Analysis.MsBuild.targets"
            };

            foreach (var file in files)
            {
                using (var stream = thisAssembly.GetManifestResourceStream($"{baseResourceName}.{file.Replace("/", ".")}"))
                {
                    var targetBytes = new byte[stream.Length];
                    stream.Read(targetBytes, 0, targetBytes.Length);
                    var filePath = Path.Combine(targetLocation, file);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    File.WriteAllBytes(filePath, targetBytes);
                }
            }

            return this;
        }


        public MsBuildProjectContextBuilder RestoreProjectPackages()
        {
            var projectPath = _context.ProjectFilePath;

            Logger?.LogInformation($"   {(projectPath)}: Restoring packages");
            var result = Command.CreateDotNet(
                "restore",
                new string[]
                {
                    projectPath,
                    "--verbosity", "quiet",
                    $"/p:nowarn=NU1603"
                })
                .OnOutputLine(l => Logger.LogInformation(l))
                .OnErrorLine(l => Logger.LogError(l))
                .Execute();
            if (result.ExitCode != 0)
            {
                throw new Exception(
                    $"{result.StartInfo.FileName} {result.StartInfo.Arguments} exited with code {result.ExitCode}");
            }

            return this;
        }


        public MsBuildProjectContext BuildProjectContext()
        {
            var projectPath = _context.ProjectFilePath;

            var cleanupTargets = false;
            if (TargetsLocation == null)
            {
                cleanupTargets = true;
                TargetsLocation = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                InstallTargets(TargetsLocation);
            }

            var line = $"   {(projectPath)}";
            if (Framework != null) line += $" ({Framework})";
            line += $": Evaluating & building dependencies";
            Logger?.LogInformation(line);

            var projectInfoFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var args = new List<string>
            {
                projectPath,
                "/nologo",
                "/v:q",
                $"/t:EvaluateProjectInfoForCodeGeneration",
                $"/p:CustomBeforeMicrosoftCSharpTargets={TargetsLocation}\\Imports.targets",
                $"/p:OutputFile={projectInfoFile}",
                $"/p:CodeGenerationTargetLocation={TargetsLocation}",
                $"/p:Configuration={Configuration}"
            };
            if (Framework != null) args.Add($"/p:TargetFramework={Framework}");

            var result = Command.CreateDotNet("msbuild", args)
                .OnOutputLine(l => Logger.LogInformation(l))
                .OnErrorLine(l => Logger.LogError(l))
                .Execute();

            if (result.ExitCode != 0)
            {
                throw new Exception(
                    $"Evaluating & building dependencies exited with code {result.ExitCode}");
            }
            try
            {
                var info = File.ReadAllText(projectInfoFile);

                var buildContext = JsonConvert.DeserializeObject<MsBuildProjectContext>(info);

                return buildContext;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to read the project information.", ex);
            }
            finally
            {
                File.Delete(projectInfoFile);
                if (cleanupTargets)
                {
                    Directory.Delete(TargetsLocation, true);
                    TargetsLocation = null;
                }
            }
        }
    }

}
