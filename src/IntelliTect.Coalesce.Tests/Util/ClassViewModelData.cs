using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace IntelliTect.Coalesce.Tests.Util;

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

    public virtual void Deserialize(IXunitSerializationInfo info)
    {
        var targetType = info.GetValue<string>(nameof(TargetType));
        var viewModelType = info.GetValue<string>(nameof(ViewModelType));

        TargetType = Type.GetType(targetType) ??
            typeof(ApiResult).Assembly.GetType(targetType) ??
            throw new Exception($"Unable to locate type {targetType}");

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

    public virtual void Serialize(IXunitSerializationInfo info)
    {
        info.AddValue(nameof(TargetType), TargetType.FullName);
        info.AddValue(nameof(ViewModelType), ViewModelType.Name);
    }
    


    public static implicit operator ClassViewModel(ClassViewModelData self)
        => self.ClassViewModel;

    public static implicit operator TypeViewModel(ClassViewModelData self)
        => self.TypeViewModel;

    public override string ToString() =>
        $"({(ViewModelType.Name.StartsWith("Sym") ? "Symbol" : "Reflect")}) {new ReflectionTypeViewModel(TargetType).FullyQualifiedName}";
}