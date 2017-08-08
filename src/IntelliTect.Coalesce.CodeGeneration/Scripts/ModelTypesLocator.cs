using System;
using System.Collections.Generic;
#if NET451
using System.ComponentModel;
#endif
using System.Linq;
using Microsoft.VisualStudio.Web.CodeGeneration;
using Microsoft.CodeAnalysis;
using IntelliTect.Coalesce.CodeGeneration.Common;
using Microsoft.CodeAnalysis.MSBuild;
using Microsoft.Build.Exceptions;
using System.Diagnostics;

namespace IntelliTect.Coalesce.CodeGeneration.Scripts
{
    public class ModelTypesLocator : IModelTypesLocator
    {
        private Workspace _projectWorkspace;
        private ProjectContext _projectContext;

        private IEnumerable<ModelType> _types;

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
            return _types = _types ?? _projectWorkspace.CurrentSolution.Projects
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

        public static ModelTypesLocator FromProjectContext(ProjectContext project)
        {
            var workspace = MSBuildWorkspace.Create();
            workspace.WorkspaceFailed += (object sender, WorkspaceDiagnosticEventArgs e) =>
            {
                if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
                {
                    // NB: Ultimately an InvalidCast happens with the TypeScript FindConfigFilesTask (compiled 
                    //     against v4.0 of Microsoft.Build) trying to cast to a ITask in Microsoft.Build v15.0 
                    //     Therefore we must ignore an empty error message.
                    Debug.WriteLine(e.Diagnostic.Message);
                    if (e.Diagnostic.Message.EndsWith(".csproj'", StringComparison.OrdinalIgnoreCase) == false)
                    {
                        throw new InvalidProjectFileException(e.Diagnostic.Message);
                    }
                }
            };

            var result = workspace.OpenProjectAsync(project.ProjectFilePath).Result;

            //var workspace = new ProjectJsonWorkspace(project.ProjectDirectory);

            return new ModelTypesLocator(workspace, project);
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
