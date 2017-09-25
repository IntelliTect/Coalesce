using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Build.Exceptions;
using System.Diagnostics;
using System.IO;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.TypeDefinition.Wrappers;
using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;

#if NET462
using Microsoft.CodeAnalysis.MSBuild;
#endif

namespace IntelliTect.Coalesce.CodeGeneration.Analysis.Roslyn
{
    public class RoslynTypeLocator : TypeLocator
    {
        private Workspace _projectWorkspace;
        private RoslynProjectContext _projectContext;

        private Compilation _compilation;

        public RoslynTypeLocator(Workspace projectWorkspace, RoslynProjectContext projectContext)
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

            _compilation = project
                .WithMetadataReferences(_projectContext.GetMetadataReferences())
                .GetCompilationAsync().Result;

            /*
            // Commented this out because it seems to be giving us lots of nonsense errors that aren't true,
            // and that don't actually affect our output.

            var diagnostics = _compilation.GetDiagnostics();
            var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error);
            if (errors.Any())
                throw new AggregateException(errors.Select(e => new Exception(e.ToString())));
            //*/

            return _compilation;
        }

        public IEnumerable<INamedTypeSymbol> GetAllTypes()
        {
            var compilation = GetProjectCompilation();
            if (compilation == null)
            {
                throw new ArgumentNullException(nameof(compilation));
            }

            var types = new List<INamedTypeSymbol>();
            void CollectTypes(INamespaceSymbol ns)
            {
                types.AddRange(ns.GetTypeMembers());

                foreach (var nestedNs in ns.GetNamespaceMembers())
                {
                    CollectTypes(nestedNs);
                }
            }

            CollectTypes(compilation.Assembly.GlobalNamespace);
            return types;
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

        public static RoslynTypeLocator FromProjectContext(RoslynProjectContext project)
        {
#if !NET462
            throw new PlatformNotSupportedException("Roslyn-based project systems are only supported on full framework due to the need for MSBuildWorkspace");
#endif
#if NET462
            var workspace = MSBuildWorkspace.Create();
            workspace.WorkspaceFailed += (object sender, WorkspaceDiagnosticEventArgs e) =>
            {
                if (e.Diagnostic.Kind == WorkspaceDiagnosticKind.Failure)
                {
                    
                       // NB: Ultimately an InvalidCast happens with the TypeScript FindConfigFilesTask (compiled 
                       //     against v4.0 of Microsoft.Build) trying to cast to a ITask in Microsoft.Build v15.0 
                       //     Therefore we must ignore an empty error message.
                       Debug.WriteLine(e.Diagnostic.Message);
                    if (!e.Diagnostic.Message.Contains(
                        "Unable to cast object of type 'Microsoft.CodeAnalysis.BuildTasks.Csc' to type 'Microsoft.Build.Framework.ITask'."))
                    {
                        throw new InvalidProjectFileException(e.Diagnostic.Message);
                    }
                }
            };

            var result = workspace.OpenProjectAsync(project.ProjectFilePath).Result;

            return new RoslynTypeLocator(workspace, project);
#endif
        }

        public override TypeViewModel FindType(string typeName, bool throwWhenNotFound = true)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentException($"Please provide a valid {nameof(typeName)}", nameof(typeName));
            }

            var candidateModelTypes = GetType(typeName).ToList();

            int count = candidateModelTypes.Count;
            if (count == 0)
            {
                if (throwWhenNotFound)
                {
                    throw new ArgumentException(string.Format("A type with the name {0} does not exist", typeName));
                }
                return null;
            }

            if (count > 1)
            {
                throw new ArgumentException(string.Format(
                    "Multiple types matching the name {0} exist:{1}, please use a fully qualified name",
                    typeName,
                    string.Join(",", candidateModelTypes.Select(t => t.Name).ToArray())));
            }

            return new TypeViewModel(new SymbolTypeWrapper(candidateModelTypes.First()));
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
