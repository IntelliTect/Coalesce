using System;
using System.Collections.Generic;
#if NET451
using System.ComponentModel;
#endif
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Frameworks;
using System.IO;
using Microsoft.VisualStudio.Web.CodeGeneration;
using Microsoft.Extensions.ProjectModel;
using Microsoft.VisualStudio.Web.CodeGeneration.DotNet;
using Microsoft.VisualStudio.Web.CodeGeneration.Templating;
using Microsoft.VisualStudio.Web.CodeGeneration.Templating.Compilation;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using IntelliTect.Coalesce.CodeGeneration.Common;

namespace IntelliTect.Coalesce.CodeGeneration.Scripts
{
    public static class DependencyProvider
    {
        private static NuGetFramework framework;

        static DependencyProvider()
        {
#if NET46
            framework = FrameworkConstants.CommonFrameworks.Net46;
#else
            framework = FrameworkConstants.CommonFrameworks.NetStandard16;
#endif
        }

        public static ProjectContext ProjectContext(string projectPath)
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

            var context = MsBuildProjectContextBuilder.Build(
                foundProjectJsonPath,
                foundProjectJsonFile,
                tempFile,
                configuration);

            File.Delete(tempFile);

            return context;
        }


        public static ModelTypesLocator ModelTypesLocator(ProjectContext project)
        {
            var workspace = MSBuildWorkspace.Create();
            workspace.WorkspaceFailed += (object sender, WorkspaceDiagnosticEventArgs e) =>
            {
                //if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
                    //throw new Exception(e.Diagnostic.Message);
            };

            var result = workspace.OpenProjectAsync(project.ProjectFilePath).Result;

            //var workspace = new ProjectJsonWorkspace(project.ProjectDirectory);

            return new ModelTypesLocator(workspace, project);
        }

        public static CodeGeneratorActionsService CodeGeneratorActionsService(ProjectContext project)
        {
            IFilesLocator files = new FilesLocator();

            return new CodeGeneratorActionsService(new RazorTemplating(project), files);
        }
    }

    internal static class RoslynUtilities
    {
        public static IEnumerable<ITypeSymbol> GetDirectTypesInCompilation(Compilation compilation)
        {
            if (compilation == null)
            {
                throw new ArgumentNullException(nameof(compilation));
            }

            var types = new List<ITypeSymbol>();
            CollectTypes(compilation.Assembly.GlobalNamespace, types);
            return types;
        }

        private static void CollectTypes(INamespaceSymbol ns, List<ITypeSymbol> types)
        {
            types.AddRange(ns.GetTypeMembers().Cast<ITypeSymbol>());

            foreach (var nestedNs in ns.GetNamespaceMembers())
            {
                CollectTypes(nestedNs, types);
            }
        }
    }

    public class ModelTypesLocator : IModelTypesLocator
    {
        private Workspace _projectWorkspace;
        private ProjectContext _projectContext;

        public ModelTypesLocator(Workspace projectWorkspace, ProjectContext projectContext)
        {
            if (projectWorkspace == null)
            {
                throw new ArgumentNullException(nameof(projectWorkspace));
            }

            _projectWorkspace = projectWorkspace;
            _projectContext = projectContext;
        }

        public IEnumerable<ModelType> GetAllTypes()
        {
            return _projectWorkspace.CurrentSolution.Projects
                .Select(project => project
                    .AddMetadataReferences(_projectContext.CompilationAssemblies.Select(rr => MetadataReference.CreateFromFile(rr.ResolvedPath)))
                    .GetCompilationAsync().Result
                )
                .Select(comp => RoslynUtilities.GetDirectTypesInCompilation(comp))
                .Aggregate((col1, col2) => col1.Concat(col2).ToList())
                .Distinct(new TypeSymbolEqualityComparer())
                .Select(ts => ModelType.FromITypeSymbol(ts));
        }

        public IEnumerable<ModelType> GetType(string typeName)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException(nameof(typeName));
            }

            return GetAllTypes()
                .Where(type => string.Equals(type.Name, typeName, StringComparison.Ordinal));
        }

        private class TypeSymbolEqualityComparer : IEqualityComparer<ITypeSymbol>
        {
            public bool Equals(ITypeSymbol x, ITypeSymbol y)
            {
                if (Object.ReferenceEquals(x, y))
                {
                    return true;
                }
                if (Object.ReferenceEquals(x, null) || Object.ReferenceEquals(y, null))
                {
                    return false;
                }

                //Check for namespace to be the same.
                var isNamespaceEqual = (Object.ReferenceEquals(x.ContainingNamespace, y.ContainingNamespace)
                        || ((x.ContainingNamespace != null && y.ContainingNamespace != null)
                            && (x.ContainingNamespace.Name == y.ContainingNamespace.Name)));
                //Check for assembly to be the same.
                var isAssemblyEqual = (object.ReferenceEquals(x.ContainingAssembly, y.ContainingAssembly)
                        || ((x.ContainingAssembly != null && y.ContainingAssembly != null)
                            && (x.ContainingAssembly.Name == y.ContainingAssembly.Name)));

                return x.Name == y.Name
                    && isNamespaceEqual
                    && isAssemblyEqual;

            }

            public int GetHashCode(ITypeSymbol obj)
            {
                if (Object.ReferenceEquals(obj, null))
                {
                    return 0;
                }
                var hashName = obj.Name == null ? 0 : obj.Name.GetHashCode();
                var hashNamespace = obj.ContainingNamespace?.Name == null ? 0 : obj.ContainingNamespace.Name.GetHashCode();
                var hashAssembly = obj.ContainingAssembly?.Name == null ? 0 : obj.ContainingAssembly.Name.GetHashCode();

                return hashName ^ hashNamespace ^ hashAssembly;
            }
        }
    }
}
