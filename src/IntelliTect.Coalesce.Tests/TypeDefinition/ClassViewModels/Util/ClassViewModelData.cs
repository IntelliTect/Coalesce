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

        public ClassViewModelData() { }
        public ClassViewModelData(Type targetType, Type viewModelType)
        {
            TargetType = targetType;
            ViewModelType = viewModelType;
            SetupProps();
        }

        public override string ToString() =>
            $"{ViewModelType.Name.Replace("ClassViewModel", "")}: {TargetType.Name}";

        private void SetupProps()
        {
            if (ViewModelType == typeof(ReflectionClassViewModel))
            {
                ClassViewModel = new ReflectionClassViewModel(TargetType);
            }
            else if (ViewModelType == typeof(SymbolClassViewModel))
            {
                var asm = Assembly.GetExecutingAssembly();
                var streamName = TargetType.FullName + ".cs";

                SyntaxTree tree;
                using (var stream = asm.GetManifestResourceStream(streamName))
                {
                    tree = CSharpSyntaxTree.ParseText(SourceText.From(stream));
                }

                var compilation = CSharpCompilation.Create(
                    "SymbolAsm",
                    new[] { tree },
                    new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
                });
                var semanticModel = compilation.GetSemanticModel(tree);

                var node = tree
                    .GetRoot()
                    .DescendantNodes()
                    .OfType<ClassDeclarationSyntax>()
                    .Select(syntax => semanticModel.GetDeclaredSymbol(syntax))
                    .Where(symbol => symbol.Name == TargetType.Name)
                    .SingleOrDefault()
                    ?? throw new ArgumentException($"Class {TargetType} not found in resource {streamName}.");

                ClassViewModel = new SymbolClassViewModel(node);
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
    }
}
