using IntelliTect.Coalesce.TypeDefinition;
using System;
using Xunit.Abstractions;

namespace IntelliTect.Coalesce.Tests.Util
{
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

        public override void Deserialize(IXunitSerializationInfo info)
        {
            base.Deserialize(info);
            PropName = info.GetValue<string>(nameof(PropName));
        }

        public override void Serialize(IXunitSerializationInfo info)
        {
            base.Serialize(info);
            info.AddValue(nameof(PropName), PropName);
        }

        public static implicit operator PropertyViewModel(PropertyViewModelData self)
            => self.PropertyViewModel;

        public override string ToString() =>
            $"({(ViewModelType.Name.StartsWith("Sym") ? "Symbol" : "Reflect")}) {new ReflectionTypeViewModel(TargetType).FullyQualifiedName}.{PropName}";
    }
}
