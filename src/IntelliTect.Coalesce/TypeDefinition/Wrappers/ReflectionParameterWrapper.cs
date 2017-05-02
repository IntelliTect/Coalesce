using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using System.Reflection;

namespace IntelliTect.Coalesce.TypeDefinition.Wrappers
{
    internal class ReflectionParameterWrapper : ParameterWrapper
    {
        public ReflectionParameterWrapper(ParameterInfo info)
        {
            Info = info;
            if (info.ParameterType.IsByRef)
                Type = new TypeViewModel(new ReflectionTypeWrapper(info.ParameterType.GetElementType()));
            else
                Type = new TypeViewModel(new ReflectionTypeWrapper(info.ParameterType));

        }

        public override string Name => Info.Name;

        public override object GetAttributeValue<TAttribute>(string valueName)
        {
            return Info.GetAttributeValue<TAttribute>(valueName);
        }
        public override bool HasAttribute<TAttribute>()
        {
            return Info.HasAttribute<TAttribute>();
        }
    }
}
