using IntelliTect.Coalesce.TypeDefinition;
using System;

namespace IntelliTect.Coalesce.Tests.Util;

public class PropertyViewModelData : ClassViewModelData
{
    public string PropName { get; private set; }

    public PropertyViewModel PropertyViewModel => ClassViewModel.PropertyByName(PropName);

    public PropertyViewModelData()
    {
    }

    public PropertyViewModelData(Type targetType, string propName, Type viewModelType) : base(targetType, viewModelType)
    {
        PropName = propName;
    }

    public static implicit operator PropertyViewModel(PropertyViewModelData self)
        => self.PropertyViewModel;

    public override string ToString() =>
        $"({(ViewModelType.Name.StartsWith("Sym") ? "Symbol" : "Reflect")}) {new ReflectionTypeViewModel(TargetType).FullyQualifiedName}.{PropName}";
}