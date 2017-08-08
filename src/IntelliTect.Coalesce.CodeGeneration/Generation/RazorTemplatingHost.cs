// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Razor;
using Microsoft.AspNetCore.Razor.CodeGenerators;

namespace IntelliTect.Coalesce.CodeGeneration.Scripts
{ 
    internal class RazorTemplatingHost : RazorEngineHost
    {
        private static readonly string[] _defaultNamespaces = new[]
        {
            "System",
            "System.Linq",
            "System.Collections.Generic",
            "System.Dynamic",
            "Microsoft.VisualStudio.Web.CodeGeneration",
            "Microsoft.VisualStudio.Web.CodeGeneration.Templating",
        };

        public RazorTemplatingHost(Type baseType)
            : base(new CSharpRazorCodeLanguage())
        {
            if (baseType == null)
            {
                throw new ArgumentNullException(nameof(baseType));
            }

            DefaultBaseClass = baseType.FullName;

            foreach (var ns in _defaultNamespaces)
            {
                NamespaceImports.Add(ns);
            }
        }
    }
}