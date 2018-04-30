using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace IntelliTect.Coalesce.TypeDefinition
{
    internal class ReflectionPropertyViewModel : PropertyViewModel
    {
        protected PropertyInfo Info { get; }

        public ReflectionPropertyViewModel(ClassViewModel effectiveParent, ClassViewModel declaringParent, PropertyInfo propetyInfo)
        {
            Parent = declaringParent;
            EffectiveParent = effectiveParent;
            Info = propetyInfo;
            Type = new ReflectionTypeViewModel(Info.PropertyType);
        }

        public override string Name => Info.Name;

        public override string Comment => "";

        public override bool HasGetter => Info.CanRead;

        public override bool HasSetter => Info.CanWrite;

        public override PropertyInfo PropertyInfo => Info;
        
        public override bool IsVirtual => Info.GetGetMethod()?.IsVirtual ?? Info.GetSetMethod()?.IsVirtual ?? false;

        public override bool IsStatic => Info.GetGetMethod()?.IsStatic ?? Info.GetSetMethod()?.IsStatic ?? false;

        public override bool IsInternalUse => base.IsInternalUse || !Info.GetGetMethod(true).IsPublic;

        public override object GetAttributeValue<TAttribute>(string valueName)
            => Info.GetAttributeValue<TAttribute>(valueName);
        
        public override bool HasAttribute<TAttribute>() =>
            Info.HasAttribute<TAttribute>();
        
    }
}
