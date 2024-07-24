using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.AspNetCore.Authorization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit;

namespace IntelliTect.Coalesce.Tests.Util
{
    public static class ReflectionRepositoryFactory
    {
        static ReflectionRepositoryFactory()
        {
            MakeFromReflection(ReflectionRepository.Global);
        }

        public static readonly IReadOnlyCollection<SyntaxTree> ModelSyntaxTrees = GetModelSyntaxTrees();

        internal static readonly CSharpCompilation Compilation = GetCompilation(new SyntaxTree[0]);
        internal static readonly List<ITypeSymbol> Symbols = GetAllSymbols();

        public static readonly ReflectionRepository Symbol = MakeFromSymbols();
        public static readonly ReflectionRepository Reflection = MakeFromReflection();

        public const string SymbolDiscoveryAssemblyName = "SymbolAsm";
        
        public static ReflectionRepository MakeFromSymbols()
        {
            var rr = new ReflectionRepository();
            rr.DiscoverCoalescedTypes(Symbols
                .Where(s =>
                    // For classes inside the TargetClasses namespace, only include those from
                    // the symbol discovery assembly, not the copies that were included from the 
                    // assembly metadata of this very test assembly (IntelliTect.Coalesce.Tests),
                    // as the versions derived from assembly metadata behave slightly differently
                    // and are also just otherwise redundant.
                    s.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) is string fqn &&
                    (
                        (
                            !fqn.Contains("IntelliTect.Coalesce.Tests.TargetClasses") //&&
                           // !fqn.StartsWith("System.")
                        ) || 
                        (s is IArrayTypeSymbol ats ? ats.ElementType : s).ContainingAssembly?.MetadataName == SymbolDiscoveryAssemblyName
                    )
                )
                .Select(s => new SymbolTypeViewModel(rr, s))
            );
            return rr;
        }

        public static ReflectionRepository MakeFromReflection()
        {
            var rr = new ReflectionRepository();
            MakeFromReflection(rr);
            return rr;
        }

        public static void Initialize()
        {
            // Do nothing - this is a dummy method to ensure the static constructor was hit,
            // which will in turn initialize ReflectionRepository.Global.
        }

        private static void MakeFromReflection(ReflectionRepository rr)
        {
            // Just picking an arbitrary class that should always be in the test assembly.
            rr.AddAssembly<TargetClasses.TestDbContext.AppDbContext>();
            rr.AddAssembly<ComplexModel>();
        }

        private static IReadOnlyCollection<SyntaxTree> GetModelSyntaxTrees()
        {
            var asm = Assembly.GetExecutingAssembly();

            var parseOptions = CSharpParseOptions.Default
                // We inject the current preprocessor constants via
                // IntelliTect.Coalesce.Tests.csproj target "AddDefinesAsAttribute". 
                // Grab them and add them to the compilation.
                .WithPreprocessorSymbols(Assembly
                    .GetExecutingAssembly()
                    .GetCustomAttributes<AssemblyMetadataAttribute>()
                    .Where(m => m.Key == "DefineConstants")
                    .First()
                    .Value
                    .Split(";", StringSplitOptions.RemoveEmptyEntries));

            return asm.GetManifestResourceNames()
                .AsParallel()
                .Where(name => name.EndsWith(".cs"))
                .Select(name =>
                {
                    using var stream = asm.GetManifestResourceStream(name);
                    return CSharpSyntaxTree.ParseText(
                        SourceText.From(stream), parseOptions
                    );
                })
                .ToList()
                .AsReadOnly();
        }

        public static CSharpCompilation GetCompilation(IEnumerable<SyntaxTree> trees, bool assertSuccess = true)
        {
            var loaded = new HashSet<Assembly>();
            void Load(Assembly asm)
            {
                foreach (var reference in asm.GetReferencedAssemblies())
                {
                    var dep = Assembly.Load(reference);
                    if (loaded.Add(dep)) Load(dep);
                }
            }

            // Eagerly and recursively load all dependencies of the current assembly,
            // which takes care of almost everything that can be a dependency of the code gen output.
            Load(Assembly.GetExecutingAssembly());

            // Other assemblies that are ONLY introduced by generated code:
            Load(typeof(Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider).Assembly);

            // Now that we've eagerly loaded all possible dependencies,
            // gather their locations so we can feed those locations to Roslyn.
            var assemblies = AppDomain.CurrentDomain
                .GetAssemblies()
                // Exclude dynamic assemblies (they can't possibly be relevant here)
                .Where(a => !a.IsDynamic)
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .ToArray();

            var compilation = CSharpCompilation.Create(
                SymbolDiscoveryAssemblyName,
                trees.Concat(ModelSyntaxTrees),
                assemblies,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            if (assertSuccess)
            {
                AssertCompilationSuccess(compilation);
            }

            return compilation;
        }

        public static void AssertCompilationSuccess(CSharpCompilation comp)
        {
            var errors = comp
                .GetDiagnostics()
                .Where(d => d.Severity >= Microsoft.CodeAnalysis.DiagnosticSeverity.Error);

            if (!errors.Any()) return;

            Assert.False(true, string.Join("\n", errors.Select(error =>
                error.ToString() +
                $" near `" +
                error.Location.SourceTree.ToString().Substring(error.Location.SourceSpan.Start, error.Location.SourceSpan.Length) +
                "`"
            )));
        }

        private static List<ITypeSymbol> GetAllSymbols()
        {
            AssertCompilationSuccess(Compilation);

            // Assembly.GlobalNamespace restricts us to only types declared in the C# files 
            // that are being included.
            // compilation.GlobalNamespace gives us ALL CLR types
            // that are accessible, which is way more useful.
            //return Compilation.Assembly.GlobalNamespace
            var visitor = new SymbolDiscoveryVisitor();
            Compilation.GlobalNamespace.Accept(visitor);
            return visitor.Symbols.ToList();
        }


        private class SymbolDiscoveryVisitor : SymbolVisitor
        {
            public HashSet<ITypeSymbol> Symbols = new HashSet<ITypeSymbol>(SymbolEqualityComparer.IncludeNullability);

            public override void VisitNamespace(INamespaceSymbol symbol)
            {
                foreach (var childSymbol in symbol.GetMembers())
                {
                    childSymbol.Accept(this);
                }
            }

            public override void VisitNamedType(INamedTypeSymbol symbol)
            {
                if (!Symbols.Add(symbol)) return;

                foreach (var childSymbol in symbol.GetMembers())
                {
                    childSymbol.Accept(this);
                }
            }

            public override void VisitArrayType(IArrayTypeSymbol symbol)
            {
                if (!Symbols.Add(symbol)) return;
                symbol.ElementType.Accept(this);
            }

            public override void VisitProperty(IPropertySymbol symbol)
            {
                if (symbol.Type.OriginalDefinition != symbol.Type)
                {

                    symbol.Type.Accept(this);
                }
                else
                {

                    symbol.Type.Accept(this);
                }
            }

            public override void VisitMethod(IMethodSymbol symbol)
            {
                // Hack to pick up any arbitrary constructed generic types
                // that were specified as parameters to [ClassViewModelDataAttribute] in tests.
                foreach (var attr in symbol.GetAttributeProvider().GetAttributes<ClassViewModelDataAttribute>())
                {
                    var value = attr.GetValue("targetClass");
                    if (value is SymbolTypeViewModel type) type.Symbol.Accept(this);
                }
            }
        }
    }
}
