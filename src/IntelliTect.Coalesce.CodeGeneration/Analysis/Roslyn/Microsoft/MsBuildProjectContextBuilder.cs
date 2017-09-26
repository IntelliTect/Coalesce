// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

// Source: https://github.com/aspnet/Scaffolding/blob/463e4da9cd29e564a1ba9b952d55c0714878567c/src/Ext.ProjectModel.MsBuild.Sources/MsBuildProjectContextBuilder.cs

/* NOTICE OF MODIFICATION:
 *      - Changed namespace.
 *      - Modified by IntelliTect to change parameters passed to msbuild,
 *          and changed handling of output from modified build target.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Internal;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.ProjectModel;
using Newtonsoft.Json;
using Microsoft.VisualStudio.Web.CodeGeneration.Msbuild;
using Microsoft.DotNet.Cli.Utils;
using System.Linq;

namespace IntelliTect.Coalesce.CodeGeneration.Analysis.Roslyn.Microsoft
{
    public class MsBuildProjectContextBuilder
    {
        private string _projectPath;
        private string _targetLocation;
        private string _configuration;

        public MsBuildProjectContextBuilder(string projectPath, string targetsLocation, string configuration="Debug")
        {
            if (string.IsNullOrEmpty(projectPath))
            {
                throw new ArgumentNullException(nameof(projectPath));
            }

            if (string.IsNullOrEmpty(targetsLocation))
            {
                throw new ArgumentNullException(nameof(targetsLocation));
            }

            _configuration = configuration;
            _projectPath = projectPath;
            _targetLocation = targetsLocation;
        }

        public IProjectContext Build()
        {
            var errors = new List<string>();
            var output = new List<string>();
            var tmpFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            var result = Command.CreateDotNet(
                "msbuild",
                new string[]
                {
                    _projectPath,
                    "/v:q",
                    $"/t:EvaluateProjectInfoForCodeGeneration",
                    $"/p:CodeGenAssemblyFullName={typeof(ProjectContextWriter).Assembly.Location}",
                    $"/p:CustomBeforeMicrosoftCSharpTargets={_targetLocation}\\Imports.targets",
                    $"/p:OutputFile={tmpFile};CodeGenerationTargetLocation={_targetLocation};Configuration={_configuration}"
                })
                .OnErrorLine(e => errors.Add(e))
                .OnOutputLine(o => { Console.WriteLine(o); output.Add(o); })
                .Execute();

            if (result.ExitCode != 0)
            {
                throw CreateProjectContextCreationFailedException(_projectPath, errors);
            }
            try
            {
                var info = File.ReadAllText(tmpFile);

                var buildContext = JsonConvert.DeserializeObject<MsBuildProjectContext>(info);

                return buildContext;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Failed to read the BuildContext information.", ex);
            }
        }

        private Exception CreateProjectContextCreationFailedException(string _projectPath, List<string> errors)
        {
            var errorMsg = $"Failed to get Project Context for {_projectPath}.";
            if (errors != null)
            {
                errorMsg += $"{Environment.NewLine} { string.Join(Environment.NewLine, errors)} ";
            }

            return new InvalidOperationException(errorMsg);
        }
    }

}
