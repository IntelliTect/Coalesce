using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using System.Reflection;

namespace IntelliTect.Coalesce.TypeDefinition
{
    internal class ReflectionParameterViewModel : ParameterViewModel
    {
        protected internal ParameterInfo Info { get; internal set; }

        public ReflectionParameterViewModel(ParameterInfo info)
        {
            Info = info;
            if (info.ParameterType.IsByRef)
                Type = new ReflectionTypeViewModel(info.ParameterType.GetElementType());
            else
                Type = new ReflectionTypeViewModel(info.ParameterType);

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
