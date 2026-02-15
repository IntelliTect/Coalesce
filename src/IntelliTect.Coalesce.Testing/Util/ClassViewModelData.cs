using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace IntelliTect.Coalesce.Testing.Util;

public class ClassViewModelData
{
    public Type TargetType { get; private set; }
    public Type ViewModelType { get; private set; }

    public TypeViewModel TypeViewModel { get; private set; }
    public ClassViewModel ClassViewModel => TypeViewModel.ClassViewModel;

    public ClassViewModelData()
    {
    }

    public ClassViewModelData(Type targetType, Type viewModelType) : this()
    {
        TargetType = targetType;
        ViewModelType = viewModelType;
        SetupProps();
    }

    protected void SetupProps()
    {
        if (ViewModelType == typeof(ReflectionClassViewModel))
        {
            TypeViewModel = ReflectionRepositoryFactory.Reflection.GetOrAddType(TargetType);
        }
        else if (ViewModelType == typeof(SymbolClassViewModel))
        {
            var fqn = new ReflectionTypeViewModel(TargetType).VerboseFullyQualifiedName;
            var symbolFormat = SymbolTypeViewModel.VerboseDisplayFormat;

            var locatedSymbol = ReflectionRepositoryFactory.Symbols
                .Where(symbol => symbol.ToDisplayString(symbolFormat) == fqn)
                .FirstOrDefault();

            if (locatedSymbol == null)
            {
                throw new ArgumentException($"Class {TargetType} ({fqn}) not found in any C# embedded resources.");
            }

            TypeViewModel = ReflectionRepositoryFactory.Symbol.GetOrAddType(locatedSymbol);
        }
    }



    public static implicit operator ClassViewModel(ClassViewModelData self)
        => self.ClassViewModel;

    public static implicit operator TypeViewModel(ClassViewModelData self)
        => self.TypeViewModel;

    public override string ToString() =>
        $"({(ViewModelType.Name.StartsWith("Sym") ? "Symbol" : "Reflect")}) {new ReflectionTypeViewModel(TargetType).FullyQualifiedName}";
}
