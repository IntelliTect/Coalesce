using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using IntelliTect.Coalesce.Helpers;
using IntelliTect.Coalesce.Interfaces;

namespace IntelliTect.Coalesce.TypeDefinition.Wrappers
{
    internal class ReflectionClassWrapper : ClassWrapper
    {

        public override string Name
        {
            get { return Info.Name; }
        }

        public override string Namespace
        {
            get
            {
                return Info.Namespace;
            }
        }

        public override string Comment { get { return ""; } }

        public override List<PropertyWrapper> Properties
        {
            get
            {
                var result = new List<PropertyWrapper>();
                foreach (PropertyInfo dtoPropertyInfo in Info.GetProperties())
                {
                    result.Add(new ReflectionPropertyWrapper(dtoPropertyInfo));
                }
                return result;
            }
        }

        public override List<MethodWrapper> RawMethods
        {
            get
            {
                var result = new List<MethodWrapper>();
                foreach (MethodInfo method in Info.GetMethods().Where(f => f.IsPublic && !f.IsSpecialName))
                {
                    result.Add(new ReflectionMethodWrapper(method));
                }
                return result;
            }
        }

        public override bool IsComplexType
        {
            get
            {
                return Info.HasAttribute<ComplexTypeAttribute>();
            }
        }

        public override bool IsDto
        {
            get
            {
                return Info.GetInterfaces().Any(f => f.Name.Contains("IClassDto"));
            }
        }

        public override ClassViewModel DtoBaseType
        {
            get
            {
                var iDto = Info.GetInterfaces().FirstOrDefault(f => f.Name.Contains("IClassDto"));
                if (iDto != null)
                {
                    ClassViewModel baseModel = ReflectionRepository.GetClassViewModel(iDto.GetGenericArguments()[0].Name);
                    return baseModel;
                }
                return null;
            }
        }

        public ReflectionClassWrapper(Type classType)
        {
            Info = classType;
        }

        public override object GetAttributeValue<TAttribute>(string valueName)
        {
            return Info.GetAttributeValue<TAttribute>(valueName);
        }
        public override bool HasAttribute<TAttribute>()
        {
            return Info.HasAttribute<TAttribute>();
        }
        public override AttributeWrapper GetSecurityAttribute<TAttribute>()
        {
            return new AttributeWrapper
            {
                Attribute = Info.GetAttribute<TAttribute>()
            };
        }
    }
}
