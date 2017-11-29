using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using IntelliTect.Coalesce.TypeDefinition.Wrappers;
using Microsoft.CodeAnalysis;
using IntelliTect.Coalesce.Helpers;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public class ReflectionClassViewModel : ClassViewModel
    {
        protected Type Info { get; }

        public ReflectionClassViewModel(Type type)
        {
            Info = type;
            Type = new ReflectionTypeViewModel(type);
        }

        public override string Name => Info.Name;

        public override string Comment => "";

        internal override ICollection<PropertyViewModel> RawProperties => Info.GetProperties()
            .Select((p, i) => new ReflectionPropertyViewModel(this, p){ ClassFieldOrder = i })
            .Cast<PropertyViewModel>()
            .ToList();

        internal override ICollection<MethodViewModel> RawMethods => Info.GetMethods()
            .Where(m => m.IsPublic && !m.IsSpecialName)
            .Select(m => new ReflectionMethodViewModel(m, this))
            .Cast<MethodViewModel>()
            .ToList();

        public override bool IsDto => Info.GetInterfaces().Any(f => f.Name.Contains("IClassDto"));

        public override ClassViewModel DtoBaseViewModel
        {
            get
            {
                var iDto = Info.GetInterfaces().FirstOrDefault(f => f.Name.Contains("IClassDto"));
                if (iDto != null)
                {
                    ClassViewModel baseModel = ReflectionRepository.Global.GetClassViewModel(iDto.GetGenericArguments()[0].Name);
                    return baseModel;
                }
                return null;
            }
        }

        public override object GetAttributeValue<TAttribute>(string valueName) =>
            Type.GetAttributeValue<TAttribute>(valueName);

        public override bool HasAttribute<TAttribute>() =>
            Type.HasAttribute<TAttribute>();

        protected override AttributeWrapper GetSecurityAttribute<TAttribute>() =>
            new AttributeWrapper
            {
                Attribute = Info.GetAttribute<TAttribute>()
            };
    }
}