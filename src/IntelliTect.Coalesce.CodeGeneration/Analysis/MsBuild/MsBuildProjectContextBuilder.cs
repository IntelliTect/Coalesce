// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// Modified heavily by IntelliTect from: 
// https://github.com/aspnet/Scaffolding/blob/463e4da9cd29e564a1ba9b952d55c0714878567c/src/Ext.ProjectModel.MsBuild.Sources/MsBuildProjectContextBuilder.cs
// https://github.com/aspnet/Scaffolding/blob/463e4da9cd29e564a1ba9b952d55c0714878567c/src/VS.Web.CG.Tools/TargetInstaller.cs


using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Microsoft.DotNet.Cli.Utils;
using System.Linq;
using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Diagnostics;

namespace IntelliTect.Coalesce.CodeGeneration.Analysis.MsBuild;

/// <summary>
/// Builds an MsBuildProjectContext for a project, which provides project metadata useful for code generation.
/// </summary>
public class MsBuildProjectContextBuilder
{
    private readonly ProjectContext _context;
    public string Configuration { get; private set; } = "Debug";
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

    private string InstallTargets()
    {
        string targetLocation = Path.Combine(_context.ProjectPath, "obj", "Coalesce");

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

        Parallel.ForEach(files, file =>
        {
            using var stream = thisAssembly.GetManifestResourceStream($"{baseResourceName}.{file.Replace("/", ".")}");
            var fileBytes = new byte[stream.Length];
            stream.ReadExactly(fileBytes);

            var filePath = Path.Combine(targetLocation, file);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

            if (!File.Exists(filePath) || !File.ReadAllBytes(filePath).SequenceEqual(fileBytes))
            {
                File.WriteAllBytes(filePath, fileBytes);
            }
            else
            {
                // Skipping the write here avoids updating the file timestamp,
                // which then in turn allows MSBuild to skip builds of our dependencies.
                // Since MSBuild considers the target files we inject to be build inputs,
                // it factors their file timestamps when determining whether existing build output is stale or not.
                Logger.LogDebug($"Skipped writing target {file}, output is identical.");
            }
        });

        return targetLocation;
    }

    public MsBuildProjectContextBuilder RestoreProjectPackages()
    {
        var projectPath = _context.ProjectFilePath;

        Logger?.LogInformation($"   {(projectPath)}: Restoring packages");
        var outputLines = new List<string>();
        var result = Command.CreateDotNet(
            "restore",
            new string[]
            {
                projectPath,
                "--verbosity", "quiet",
                $"/p:nowarn=NU1603"
            })
            .OnOutputLine(l => { outputLines.Add(l); Logger.LogDebug(l); })
            .OnErrorLine(l => { outputLines.Add(l); Logger.LogError(l); })
            .Execute();
        if (result.ExitCode != 0)
        {
            throw new ProjectAnalysisException(
                $"{result.StartInfo.FileName} {result.StartInfo.Arguments} exited with code {result.ExitCode}",
                outputLines);
        }

        return this;
    }


    public MsBuildProjectContext BuildProjectContext()
    {
        var timer = new Stopwatch();
        timer.Start();
         
        var projectPath = _context.ProjectFilePath;
        
        var targetsLocation = InstallTargets();

        var line = $"   {(projectPath)}";
        if (Framework != null) line += $" ({Framework})";
        Logger?.LogInformation(line);

        var projectInfoFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var args = new List<string>
        {
            projectPath,
            "/nologo",
            "/v:q",
            $"/t:EvaluateProjectInfoForCodeGeneration",
            $"/p:CustomBeforeMicrosoftCSharpTargets={targetsLocation}\\Imports.targets",
            $"/p:OutputFile={projectInfoFile}",
            $"/p:CodeGenerationTargetLocation={targetsLocation}",
            $"/p:Configuration={Configuration}",
            "/p:SkipGetTargetFrameworkProperties=true",
            $"/p:BuildProjectReferences={_context.ProjectConfiguration.BuildProjectReferences.ToString().ToLowerInvariant()}"
        };
        if (Framework != null) args.Add($"/p:TargetFramework={Framework}");

        if (Logger.IsEnabled(LogLevel.Debug))
        {
            var binlogPath = Path.Combine(Path.GetTempPath(), $"{_context.ProjectFileName}.binlog");
            Logger.LogDebug($"MSBuild binlog will write to {binlogPath}");
            args.Add($"-bl:\"{binlogPath}\"");
        }

        var outputLines = new List<string>();
        var result = Command.CreateDotNet("msbuild", args)
            .OnOutputLine(l => { outputLines.Add(l); Logger.LogDebug(l); })
            .OnErrorLine(l => { outputLines.Add(l); Logger.LogError(l); })
            .Execute();

        if (result.ExitCode != 0)
        {
            throw new ProjectAnalysisException(
                $"{projectPath}: Evaluating & building dependencies exited with code {result.ExitCode}",
                outputLines);
        }
        try
        {
            var info = File.ReadAllText(projectInfoFile);

            return JsonConvert.DeserializeObject<MsBuildProjectContext>(info);
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Failed to read the project information output from msbuild.", ex);
        }
        finally
        {
            Logger.LogDebug($"Project analysis for {projectPath} took {timer.ElapsedMilliseconds}ms");
            if (!Logger.IsEnabled(LogLevel.Debug))
            {
                File.Delete(projectInfoFile);
            }
            else
            {
                Logger.LogDebug($"Project analysis output written to {projectInfoFile}");
            }
        }
    }
}
