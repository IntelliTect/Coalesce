using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace IntelliTect.Coalesce.Tests.Util
{
    public static class ReflectionRepositoryFactory
    {
        public static readonly IReadOnlyCollection<SyntaxTree> ModelSyntaxTrees = GetModelSyntaxTrees();

        internal static readonly CSharpCompilation Compilation = GetCompilation(new SyntaxTree[0]);
        internal static readonly IEnumerable<ITypeSymbol> Symbols = GetAllSymbols();

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
                    !s.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat).Contains("IntelliTect.Coalesce.Tests.TargetClasses") || 
                    (s is IArrayTypeSymbol ats ? ats.ElementType : s).ContainingAssembly.MetadataName == SymbolDiscoveryAssemblyName
                )
                .Select(s => new SymbolTypeViewModel(rr, s))
            );
            return rr;
        }

        public static ReflectionRepository MakeFromReflection()
        {
            var rr = new ReflectionRepository();

            // Just picking an arbitrary class that should always be in the test assembly.
            rr.AddAssembly<ComplexModel>();
            return rr;
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

        public static CSharpCompilation GetCompilation(IEnumerable<SyntaxTree> trees)
        {
            // Essential asemblies that aren't otherwise being loaded already:
            Assembly.Load("Microsoft.EntityFrameworkCore.InMemory");
            Assembly.Load("System.Linq.Queryable");

            var current = Assembly.GetExecutingAssembly();
            var assemblies = AppDomain.CurrentDomain
                .GetAssemblies()
                // Exclude dynamic assemblies (they can't possibly be relevant here)
                .Where(a => !a.IsDynamic)
                .Select(a => MetadataReference.CreateFromFile(a.Location))
                .ToArray();

            return CSharpCompilation.Create(
                SymbolDiscoveryAssemblyName,
                trees.Concat(ModelSyntaxTrees),
                assemblies,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );
        }

        private static List<ITypeSymbol> GetAllSymbols()
        {
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
                foreach (var attr in symbol.GetAttributes())
                {
                    if (attr.AttributeClass.Name == nameof(ClassViewModelDataAttribute))
                    {
                        var value = attr.GetPropertyValue("targetClass", null);
                        if (value is ITypeSymbol type) type.Accept(this);
                    }
                }
            }
        }
    }
}
