using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.Build.Exceptions;
using System.Diagnostics;
using System.IO;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.CodeGeneration.Analysis.Base;
using Microsoft.VisualStudio.Web.CodeGeneration.Utils;
using System.Threading.Tasks;
using System.Collections.Concurrent;
using Microsoft.CodeAnalysis.CSharp;

namespace IntelliTect.Coalesce.CodeGeneration.Analysis.Roslyn
{
    public class RoslynTypeLocator : TypeLocator
    {
        private readonly Workspace _projectWorkspace;
        private readonly RoslynProjectContext _projectContext;

        private Compilation _compilation;

        public RoslynTypeLocator(Workspace projectWorkspace, RoslynProjectContext projectContext)
        {
            _projectWorkspace = projectWorkspace ?? throw new ArgumentNullException(nameof(projectWorkspace));
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

            var parseOptions = ((CSharpParseOptions)project.ParseOptions)
                .WithPreprocessorSymbols(_projectContext.MsBuildProjectContext.DefineConstants.Split(new[] { ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries));

            if (_projectContext.LangVersion != null)
            {
                parseOptions = parseOptions.WithLanguageVersion(_projectContext.LangVersion.Value);
            }

            _compilation = project
                .WithParseOptions(parseOptions)
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


        private class SymbolDiscoveryVisitor : SymbolVisitor
        {
            public List<INamedTypeSymbol> Discovered { get; } = new List<INamedTypeSymbol>();

            public override void VisitNamespace(INamespaceSymbol symbol)
            {
                foreach (var member in symbol.GetMembers()) member.Accept(this);
            }

            public override void VisitNamedType(INamedTypeSymbol symbol)
            {
                Discovered.Add(symbol);
                foreach (var childSymbol in symbol.GetTypeMembers()) childSymbol.Accept(this);
            }
        }

        private List<INamedTypeSymbol> _allTypes;

        public List<INamedTypeSymbol> GetAllTypes()
        {
            if (_allTypes != null) return _allTypes;
            
            var compilation = GetProjectCompilation();
            if (compilation == null)
            {
                throw new ArgumentNullException(nameof(compilation));
            }

            var visitor = new SymbolDiscoveryVisitor();
            compilation.Assembly.GlobalNamespace.Accept(visitor);
            return _allTypes = visitor.Discovered;
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

        public static RoslynTypeLocator FromProjectContext(RoslynProjectContext project)
        {
            var workspace = new RoslynWorkspace(project.MsBuildProjectContext, project.MsBuildProjectContext.Configuration);

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

            return new RoslynTypeLocator(workspace, project);
        }

        public override TypeViewModel FindType(string typeName, bool throwWhenNotFound = true)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentNullException(nameof(typeName));
            }

            var candidateModelTypes = GetAllTypes()
                .Where(type => string.Equals(type.Name, typeName, StringComparison.Ordinal))
                .ToList();

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

            return new SymbolTypeViewModel(candidateModelTypes[0]);
        }

        public override IEnumerable<TypeViewModel> FindDerivedTypes(string typeName, bool throwWhenNotFound = true)
        {
            bool HasBaseType(INamedTypeSymbol type) =>
                type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).EndsWith(typeName) || (type.BaseType != null && HasBaseType(type.BaseType));

            return GetAllTypes()
                .Where(type => HasBaseType(type))
                .Select(t => new SymbolTypeViewModel(t));
        }

        private class TypeSymbolEqualityComparer : IEqualityComparer<ITypeSymbol>
        {
            public bool Equals(ITypeSymbol x, ITypeSymbol y)
            {
                if (Object.ReferenceEquals(x, y))
                {
                    return true;
                }

                if (x is null || y is null)
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
                if (obj is null)
                {
                    return 0;
                }
                var hashName = obj.Name?.GetHashCode() ?? 0;
                var hashNamespace = obj.ContainingNamespace?.Name == null ? 0 : obj.ContainingNamespace.Name.GetHashCode();
                var hashAssembly = obj.ContainingAssembly?.Name == null ? 0 : obj.ContainingAssembly.Name.GetHashCode();

                return hashName ^ hashNamespace ^ hashAssembly;
            }
        }
    }
}
