using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Xunit.Abstractions;

namespace IntelliTect.Coalesce.Tests.TypeDefinition.ClassViewModels
{
    public class ClassViewModelData : IXunitSerializable
    {
        public Type TargetType { get; private set; }
        public Type ViewModelType { get; private set; }

        public ClassViewModel ClassViewModel { get; private set; }

        public ClassViewModelData()
        {
            allSymbols = new Lazy<List<INamedTypeSymbol>>(GetAllSymbols);
        }

        public ClassViewModelData(Type targetType, Type viewModelType) : this()
        {
            TargetType = targetType;
            ViewModelType = viewModelType;
            SetupProps();
        }

        private static Lazy<List<INamedTypeSymbol>> allSymbols;

        private List<INamedTypeSymbol> GetAllSymbols()
        {
            var asm = Assembly.GetExecutingAssembly();
            var streamName = TargetType.FullName + ".cs";

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

            var compilation = CSharpCompilation.Create(
                "SymbolAsm",
                trees,
                new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
            });

            return compilation.Assembly.GlobalNamespace
                .Accept(new SymbolDiscoveryVisitor())
                .ToList();
        }

        private void SetupProps()
        {
            if (ViewModelType == typeof(ReflectionClassViewModel))
            {
                ClassViewModel = new ReflectionClassViewModel(TargetType);
            }
            else if (ViewModelType == typeof(SymbolClassViewModel))
            {
                var node = allSymbols.Value
                    .Where(symbol => symbol.Name == TargetType.Name)
                    .SingleOrDefault()
                    ?? throw new ArgumentException($"Class {TargetType} not found in any C# embedded resources.");

                ClassViewModel = new SymbolClassViewModel(node);
            }
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

                foreach (var childSymbol in symbol.GetTypeMembers())
                {
                    //Once againt we must accept the children to visit 
                    //all of their children
                    foreach (var result in childSymbol.Accept(this)) yield return result;
                }
            }
        }




        public void Deserialize(IXunitSerializationInfo info)
        {
            var targetType = info.GetValue<string>(nameof(TargetType));
            var viewModelType = info.GetValue<string>(nameof(ViewModelType));

            TargetType = Type.GetType(targetType);

            switch (viewModelType)
            {
                case nameof(ReflectionClassViewModel):
                    ViewModelType = typeof(ReflectionClassViewModel);
                    break;

                case nameof(SymbolClassViewModel):
                    ViewModelType = typeof(SymbolClassViewModel);
                    break;

                default:
                    throw new InvalidOperationException($"Unknown type {viewModelType}");
            }

            SetupProps();
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            info.AddValue(nameof(TargetType), TargetType.FullName);
            info.AddValue(nameof(ViewModelType), ViewModelType.Name);
        }
        


        public static implicit operator ClassViewModel(ClassViewModelData self)
            => self.ClassViewModel;

        public override string ToString() =>
            $"({(ViewModelType.Name.StartsWith("Sym") ? "Symbol" : "Reflect")}) {TargetType.Name}";
    }
}
