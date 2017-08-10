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
using System.IO;
using System.Reflection;

namespace IntelliTect.Coalesce.CodeGeneration.Scripts
{
    public class ModelTypesLocator
    {
        private Workspace _projectWorkspace;
        private ProjectContext _projectContext;

        private Compilation _compilation;

        public ModelTypesLocator(Workspace projectWorkspace, ProjectContext projectContext)
        {
            if (projectWorkspace == null)
            {
                throw new ArgumentNullException(nameof(projectWorkspace));
            }

            _projectWorkspace = projectWorkspace;
            _projectContext = projectContext;
        }

        private Compilation GetProjectCompilation()
        {
            if (_compilation != null) return _compilation;

            var projectFileName = Path.GetFileName(_projectContext.ProjectFilePath);
            var project = _projectWorkspace.CurrentSolution.Projects
                .SingleOrDefault(p => Path.GetFileName(p.FilePath) == projectFileName);

            if (project == null)
            {
                throw new FileNotFoundException($"Couldn't find project in workspace with project file name {projectFileName}");
            }

            return _compilation = project
                .AddMetadataReferences(_projectContext.CompilationAssemblies.Select(rr => MetadataReference.CreateFromFile(rr.ResolvedPath)))
                .GetCompilationAsync().Result;
        }

        public IEnumerable<INamedTypeSymbol> GetAllTypes()
        {
            return RoslynUtilities.GetDirectTypesInCompilation(GetProjectCompilation());
        }

        /*
         * This is currently quite flawed - getting missing method exceptions for methods that clearly exist
         * when trying to instantiate types in the loaded assembly.
         * The intent behind this was to instantiate the DbContext to get EF's model metadata and use that
         * for code generation, since its going to be more correct & consistent than our guesses about the data model.
         
        public Assembly GetAssembly()
        {
            //return null;
            var projectFileName = Path.GetFileName(_projectContext.ProjectFilePath);
            var project = _projectWorkspace.CurrentSolution.Projects
                .SingleOrDefault(p => Path.GetFileName(p.FilePath) == projectFileName);
            
            using (var assemblyStream = new MemoryStream())
            {
                using (var pdbStream = new MemoryStream())
                {
                    var result = GetProjectCompilation().Emit(
                        assemblyStream,
                        pdbStream);

                    if (!result.Success)
                    {
                        throw new TypeLoadException($"Couldn't emit assembly for project {_projectContext.ProjectFilePath}");
                    }

                    assemblyStream.Seek(0, SeekOrigin.Begin);
                    pdbStream.Seek(0, SeekOrigin.Begin);

                    //var domain = AppDomain.CreateDomain($"{_projectContext.ProjectFilePath}-compilation");
                    //domain.ExecuteAssembly(project.OutputFilePath);
                    //foreach (var file in _projectContext.CompilationAssemblies)
                    //{
                    //    try
                    //    {
                    //        domain.Load(AssemblyName.GetAssemblyName(file.ResolvedPath));
                    //    }
                    //    catch { }
                    //}

                    AppDomain.CurrentDomain.AssemblyLoad += (sender, args) => Console.WriteLine(args.LoadedAssembly.ToString());

                    AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                    {
                        var name = new AssemblyName(args.Name);
                        var match = _projectContext.CompilationAssemblies.FirstOrDefault(a => AssemblyName.GetAssemblyName(a.ResolvedPath).Name == name.Name);
                        if (match != null)
                        {
                            return Assembly.LoadFrom(match.ResolvedPath);
                        }
                        return null;
                    };

                    var assembly = Assembly.Load(assemblyStream.ToArray(), pdbStream.ToArray());
                    //var assembly = Assembly.LoadFrom(project.OutputFilePath);
                    var contextType = assembly.GetType("Intellitect.Myriad.Data.AppDbContext");
                    var instance = Activator.CreateInstance(contextType);

                    return assembly;
                }
            }
        }
        */

        public IEnumerable<INamedTypeSymbol> GetType(string typeName)
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
