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
    internal static class ReflectionRepositoryFactory
    {
        internal static readonly CSharpCompilation Compilation = GetCompilation();
        internal static readonly IEnumerable<INamedTypeSymbol> Symbols = GetAllSymbols();

        public static readonly ReflectionRepository Symbol = MakeFromSymbols();
        public static readonly ReflectionRepository Reflection = MakeFromReflection();
        
        public static ReflectionRepository MakeFromSymbols()
        {
            var rr = new ReflectionRepository();
            rr.DiscoverCoalescedTypes(Symbols.Select(s => new SymbolTypeViewModel(s)));
            return rr;
        }

        public static ReflectionRepository MakeFromReflection()
        {
            var rr = new ReflectionRepository();

            // Just picking an arbitrary class that should always be in the test assembly.
            rr.AddAssembly<ComplexModel>();
            return rr;
        }

        private static CSharpCompilation GetCompilation()
        {
            var asm = Assembly.GetExecutingAssembly();

            var trees = asm.GetManifestResourceNames()
                .AsParallel()
                .Where(name => name.EndsWith(".cs"))
                .Select(name =>
                {
                    using (var stream = asm.GetManifestResourceStream(name))
                    {
                        return CSharpSyntaxTree.ParseText(SourceText.From(stream));
                    }
                })
                .ToList();

            return CSharpCompilation.Create(
                "SymbolAsm",
                trees,
                AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Where(a => !a.IsDynamic)
                    .Select(a => MetadataReference.CreateFromFile(a.Location)).ToArray()
            );
        }

        private static List<INamedTypeSymbol> GetAllSymbols()
        {
            // Assembly.GlobalNamespace restricts us to only types declared in the C# files 
            // that are being included.
            // compilation.GlobalNamespace gives us ALL CLR types
            // that are accessible, which is way more useful.
            //return Compilation.Assembly.GlobalNamespace
            return Compilation.GlobalNamespace
                .Accept(new SymbolDiscoveryVisitor())
                .ToList();
        }


        // TODO: this is duplicated from RoslynTypeLocator.cs in Coalesce.CodeGeneration. 
        // can we avoid this duplication?
        private class SymbolDiscoveryVisitor : SymbolVisitor<IEnumerable<INamedTypeSymbol>>
        {
            public override IEnumerable<INamedTypeSymbol> VisitNamespace(INamespaceSymbol symbol)
            {
                foreach (var childSymbol in symbol.GetMembers())
                {
                    //We must implement the visitor pattern ourselves and 
                    //accept the child symbols in order to visit their children
                    foreach (var result in childSymbol.Accept(this)) yield return result;
                }
            }

            public override IEnumerable<INamedTypeSymbol> VisitNamedType(INamedTypeSymbol symbol)
            {
                yield return symbol;

                foreach (var childSymbol in symbol.GetMembers())
                {
                    var accept = childSymbol.Accept(this);
                    if (accept != null)
                    {
                        foreach (var result in accept) yield return result;
                    }
                }
            }

            public override IEnumerable<INamedTypeSymbol> VisitMethod(IMethodSymbol symbol)
            {
                // Hack to pick up any arbitrary constructed generic types
                // that were specified as parameters to [ClassViewModelDataAttribute] in tests.
                foreach (var attr in symbol.GetAttributes())
                {
                    if (attr.AttributeClass.Name == nameof(ClassViewModelDataAttribute))
                    {
                        var value = attr.GetPropertyValue("targetClass", null);
                        if (value is INamedTypeSymbol namedType) yield return namedType;
                    }
                }
            }
        }
    }
}
