// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.Language;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;

namespace IntelliTect.Coalesce.CodeGeneration.Templating.Razor
{
    internal class CoalesceRazorTemplateEngine : MvcRazorTemplateEngine
    {
        private static readonly string[] _defaultNamespaces = new[]
        {
            "System",
            "System.Linq",
            "System.Collections.Generic",
            "System.Dynamic",
            "IntelliTect.Coalesce.Templating",
        };

        public CoalesceRazorTemplateEngine(RazorEngine engine, RazorProject project) : base(engine, project)
        {
            Options.DefaultImports = GetDefaultImports();
        }

        private static RazorSourceDocument GetDefaultImports()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                foreach (var ns in _defaultNamespaces)
                {
                    writer.WriteLine($"@using {ns}");
                }
                writer.Flush();

                stream.Position = 0;
                return RazorSourceDocument.ReadFrom(stream, fileName: null, encoding: Encoding.UTF8);
            }
        }
    }
}