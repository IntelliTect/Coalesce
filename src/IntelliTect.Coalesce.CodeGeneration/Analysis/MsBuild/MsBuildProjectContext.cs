using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.ProjectModel;
using System.Linq;

namespace IntelliTect.Coalesce.CodeGeneration.Analysis.MsBuild
{
    /// <summary>
    /// Provides project metadata useful for code generation.
    /// </summary>
    public class MsBuildProjectContext : IProjectContext
    {
        public class PackageDependency
        {
            public string ItemSpec => Path;
            public string Path { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public string Target { get; set; }
            public string Version { get; set; }
            public string Resolved { get; set; }
            public string Dependencies { get; set; }
        }

        public PackageDependency[] DependenciesDesignTime { get; set; }
        public string[] CompilationItems { get; set; }
        public string[] ProjectReferences { get; set; }
        public string[] ResolvedReferences { get; set; }
        public string[] EmbededItems { get; set; }

        public string ProjectName { get; set; }
        public string ProjectFullPath { get; set; }
        public string AssemblyFullPath { get; set; }
        public string OutputType { get; set; }
        public string Platform { get; set; }
        public string RootNamespace { get; set; }
        public string TargetDirectory { get; set; }
        public string DepsFile { get; set; }
        public string RuntimeConfig { get; set; }
        public string Configuration { get; set; }
        public string TargetFramework { get; set; }
        public string LangVersion { get; set; }
        public string DefineConstants { get; set; }

        public bool IsClassLibrary => "Library".Equals(OutputType, StringComparison.OrdinalIgnoreCase);
        public string Config => AssemblyFullPath + ".config";

        public string PackagesDirectory => null;

        public string AssemblyName => Path.GetFileName(AssemblyFullPath);

        public IEnumerable<ResolvedReference> CompilationAssemblies => 
            ResolvedReferences.Select(path => new ResolvedReference(Path.GetFileName(path), path));

        public IEnumerable<DependencyDescription> PackageDependencies =>
            GetPackageDependencies(DependenciesDesignTime);

        /// <summary>
        /// This has been changed to return null because it is currently causing errors.
        /// It shouldn't cause any adverse effects, because even if we don't have project references,
        /// we still end up with references to their compiled DLLs through CompilationAssemblies.
        /// 
        /// The error we get is from the call to ProjectCollection.GlobalProjectCollection.LoadProject in ProjectReferenceInformationProvider.
        ///     Exception thrown: 'Microsoft.Build.Exceptions.InvalidProjectFileException' in Microsoft.Build.dll
        ///     The SDK 'Microsoft.NET.Sdk' specified could not be found.
        ///     
        /// This could be related to https://github.com/IntelliTect/Coalesce/issues/19
        /// </summary>
        public IEnumerable<ProjectReferenceInformation> ProjectReferenceInformation =>
            null;//GetProjectDependency(ProjectReferences, ProjectFullPath);

        IEnumerable<string> IProjectContext.CompilationItems => CompilationItems;
        IEnumerable<string> IProjectContext.EmbededItems => EmbededItems;
        IEnumerable<string> IProjectContext.ProjectReferences => ProjectReferences;




        // Below is modified from https://github.com/aspnet/Scaffolding/blob/463e4da9cd29e564a1ba9b952d55c0714878567c/src/VS.Web.CG.Msbuild/ProjectContextWriter.cs
        // Copyright (c) .NET Foundation. All rights reserved.
        // Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

        // Major modifications by IntelliTect to support our IProjectContext POCO that we deserialize into.

        private IEnumerable<ProjectReferenceInformation> GetProjectDependency(
            IEnumerable<string> projectReferences,
            string rootProjectFullpath)
        {
            return ProjectReferenceInformationProvider.GetProjectReferenceInformation(
                rootProjectFullpath,
                projectReferences);
        }

        private IEnumerable<DependencyDescription> GetPackageDependencies(PackageDependency[] packageDependecyItems)
        {

            var packages = packageDependecyItems
                .Select(item => (Key: item.Path, Value: GetPackageDependency(item) ))
                .Where(package => package.Value != null)
                .ToDictionary(item => item.Key, item => item.Value);

            var packageMap = new Dictionary<string, DependencyDescription>(StringComparer.OrdinalIgnoreCase);
            foreach (var package in packages) packageMap.Add(package.Key, package.Value);

            PopulateDependencies(packageMap, packageDependecyItems);

            return packageMap.Values;
        }

        private void PopulateDependencies(Dictionary<string, DependencyDescription> packageMap, PackageDependency[] packageDependecyItems)
        {
            foreach (var item in packageDependecyItems)
            {
                var depSpecs = item.Dependencies
                    ?.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                DependencyDescription current = null;
                if (depSpecs == null || !packageMap.TryGetValue(item.ItemSpec, out current))
                {
                    return;
                }

                foreach (var depSpec in depSpecs)
                {
                    var spec = item.ItemSpec.Split('/').FirstOrDefault() + "/" + depSpec;
                    DependencyDescription d = null;
                    if (packageMap.TryGetValue(spec, out d))
                    {
                        current.AddDependency(new Dependency(d.Name, d.Version));
                    }
                }
            }
        }

        private DependencyDescription GetPackageDependency(PackageDependency item)
        {
            var type = item.Type;
            var name = ("Target".Equals(type, StringComparison.OrdinalIgnoreCase))
                ? item.ItemSpec
                : item.Name;

            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            var framework = item.ItemSpec.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).First();
            DependencyType dt;
            dt = Enum.TryParse(type, out dt)
                ? dt
                : DependencyType.Unknown;

            bool.TryParse(item.Resolved.ToLower(), out bool isResolved);
            return new DependencyDescription(name, item.Version, item.Path, framework, dt, isResolved);
        }
    }

}
