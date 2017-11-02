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
        protected Type Type { get; }

        public ReflectionClassViewModel(Type type)
        {
            Type = type;
        }

        public override string Name => Type.Name;

        public override string Namespace => Type.Namespace;

        public override string Comment => "";

        internal override ICollection<PropertyWrapper> RawProperties => Type.GetProperties()
            .Select(p => new ReflectionPropertyWrapper(p))
            .Cast<PropertyWrapper>()
            .ToList();

        internal override ICollection<MethodWrapper> RawMethods => Type.GetMethods()
            .Where(m => m.IsPublic && !m.IsSpecialName)
            .Select(m => new ReflectionMethodWrapper(m))
            .Cast<MethodWrapper>()
            .ToList();

        public override bool IsDto => Type.GetInterfaces().Any(f => f.Name.Contains("IClassDto"));

        public override ClassViewModel DtoBaseViewModel
        {
            get
            {
                var iDto = Type.GetInterfaces().FirstOrDefault(f => f.Name.Contains("IClassDto"));
                if (iDto != null)
                {
                    ClassViewModel baseModel = ReflectionRepository.GetClassViewModel(iDto.GetGenericArguments()[0].Name);
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
                Attribute = Type.GetAttribute<TAttribute>()
            };
    }
}