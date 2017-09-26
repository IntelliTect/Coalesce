// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.


/* NOTICE OF MODIFICATION:
 *      - Modified by IntelliTect to pull target files from different locations than what is done
 *          by the original from https://github.com/aspnet/scaffolding.
 *      - Removed unused ILogger ctor param.
 */


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Microsoft.VisualStudio.Web.CodeGeneration.Tools
{
    public class TargetInstaller
    {
        public bool EnsureTargetImported(string targetLocation)
        {
            if (string.IsNullOrEmpty(targetLocation))
            {
                throw new ArgumentNullException(nameof(targetLocation));
            }

            var toolType = typeof(TargetInstaller);
            var toolAssembly = toolType.GetTypeInfo().Assembly;
            var toolNamespace = toolType.Namespace;

            var baseResourceName = $"{toolAssembly.GetName().Name}.Analysis.Roslyn.Microsoft.Targets";

            var files = new[]
            {
                "Imports.targets",
                "build/Microsoft.VisualStudio.Web.CodeGeneration.Tools.targets",
                "buildMultiTargeting/Microsoft.VisualStudio.Web.CodeGeneration.Tools.targets"
            };

            foreach (var file in files)
            {
                using (var stream = toolAssembly.GetManifestResourceStream($"{baseResourceName}.{file.Replace("/", ".")}"))
                {
                    var targetBytes = new byte[stream.Length];
                    stream.Read(targetBytes, 0, targetBytes.Length);
                    var filePath = Path.Combine(targetLocation, file);
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                    File.WriteAllBytes(filePath, targetBytes);
                }
            }
            return true;
        }
    }
}
