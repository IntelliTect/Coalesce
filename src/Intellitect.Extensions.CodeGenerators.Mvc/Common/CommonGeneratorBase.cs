// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.PlatformAbstractions;
using Intellitect.Extensions.CodeGenerators.Mvc.Scripts;

namespace Intellitect.Extensions.CodeGenerators.Mvc
{
    /// <summary>
    /// Common generator functionality for Controllers and Views
    /// </summary>
    public abstract class CommonGeneratorBase
    {
        protected CommonGeneratorBase(IApplicationEnvironment applicationEnvironment)
        {
            ApplicationEnvironment = applicationEnvironment;
        }

        protected IApplicationEnvironment ApplicationEnvironment
        {
            get;
            private set;
        }

        protected string ValidateAndGetOutputPath(CommandLineGeneratorModel commandLineModel, string outputFileName)
        {
            string outputFolder = String.IsNullOrEmpty(commandLineModel.RelativeFolderPath)
                ? ApplicationEnvironment.ApplicationBasePath
                : Path.Combine(ApplicationEnvironment.ApplicationBasePath, commandLineModel.RelativeFolderPath);

            var outputPath = Path.Combine(outputFolder, outputFileName);

            if (File.Exists(outputPath) && !commandLineModel.Force)
            {
                throw new InvalidOperationException(string.Format(
                    CultureInfo.CurrentCulture,
                    "The file {0} exists, use -f option to overwrite",
                    outputPath));
            }

            return outputPath;
        }
    }
}
