// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.ProjectModel;
using Microsoft.CodeAnalysis;
using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using IntelliTect.Coalesce.CodeGeneration.Configuration;
using IntelliTect.Coalesce.CodeGeneration.Analysis.MsBuild;
using Microsoft.CodeAnalysis.CSharp;

namespace IntelliTect.Coalesce.CodeGeneration.Analysis.Roslyn;

public class RoslynProjectContext : ProjectContext
{
    private TypeLocator _typeLocator;

    public RoslynProjectContext(ProjectConfiguration projectConfig) : base(projectConfig)
    {
    }

    public override TypeLocator TypeLocator => _typeLocator ??= RoslynTypeLocator.FromProjectContext(this);

    public override string RootNamespace => ProjectConfiguration.RootNamespace ?? MsBuildProjectContext.RootNamespace;

    public MsBuildProjectContext MsBuildProjectContext { get; internal set; }

    public LanguageVersion? LangVersion { get; internal set; }

    public ICollection<MetadataReference> GetMetadataReferences()
    {
        return (MsBuildProjectContext.CompilationAssemblies ?? Enumerable.Empty<ResolvedReference>())
            .AsParallel()
            .Select(export => MetadataReference.CreateFromFile(export.ResolvedPath))
            .ToList<MetadataReference>();
    }
}
