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

        public ReflectionPropertyViewModel(ClassViewModel effectiveParent, ClassViewModel declaringParent, PropertyInfo propertyInfo)
            : base (effectiveParent, declaringParent, ReflectionTypeViewModel.GetOrCreate(declaringParent.ReflectionRepository, propertyInfo.PropertyType))
        {
            Info = propertyInfo;
        }

        public override string Name => Info.Name;

        public override string Comment => "";

        public override bool HasGetter => Info.CanRead;

        public override bool HasSetter => Info.CanWrite;

        public override bool HasPublicSetter => HasSetter && Info.SetMethod?.IsPublic == true;

        public override PropertyInfo PropertyInfo => Info;
        
        public override bool IsVirtual => Info.GetGetMethod()?.IsVirtual ?? Info.GetSetMethod()?.IsVirtual ?? false;

        public override bool IsStatic => Info.GetGetMethod()?.IsStatic ?? Info.GetSetMethod()?.IsStatic ?? false;

        public override bool IsInitOnly
#if NET5_0_OR_GREATER
            => Info.SetMethod?.ReturnParameter.GetRequiredCustomModifiers().Contains(typeof(System.Runtime.CompilerServices.IsExternalInit)) == true;
#else
            => false;
#endif

        public override bool IsInternalUse => base.IsInternalUse || Info.GetGetMethod(true)?.IsPublic != true;

        public override object? GetAttributeValue<TAttribute>(string valueName)
            => Info.GetAttributeValue<TAttribute>(valueName);
        
        public override bool HasAttribute<TAttribute>() =>
            Info.HasAttribute<TAttribute>();
        
    }
}
