using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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

            var nullable = new NullabilityInfoContext().Create(Info);
            ReadNullability = nullable.ReadState;
            WriteNullability = nullable.WriteState;
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
            => Info.SetMethod?.ReturnParameter.GetRequiredCustomModifiers().Contains(typeof(System.Runtime.CompilerServices.IsExternalInit)) == true;

        public override bool HasRequiredKeyword =>
#if NET7_0_OR_GREATER
            this.HasAttribute<System.Runtime.CompilerServices.RequiredMemberAttribute>();
#else
            false;
#endif

        public override bool IsInternalUse => base.IsInternalUse || Info.GetGetMethod(true)?.IsPublic != true;

        public override IEnumerable<AttributeViewModel<TAttribute>> GetAttributes<TAttribute>()
            => Info.GetAttributes<TAttribute>();

        private IReadOnlyList<ValidationAttribute>? _validationAttributes;
        internal IReadOnlyList<ValidationAttribute> GetValidationAttributes()
        {
            if (_validationAttributes is not null) return _validationAttributes;

            var attrs = Info
                .GetCustomAttributes(typeof(ValidationAttribute), true)
                .OfType<ValidationAttribute>();
            if (IsRequired && !attrs.Any(a => a is RequiredAttribute))
            {
                // Implicitly add required validation to any property that is required by other means,
                // but doesn't explicitly have a RequiredAttribute.
                // This mirrors the behavior of aspnetcore:
                // https://github.com/dotnet/aspnetcore/blob/3a6acd95c769bbbd2e5288d5844c81dee45fc958/src/Mvc/Mvc.Abstractions/src/ModelBinding/ModelMetadata.cs#L344-L347
                attrs = attrs.Append(new RequiredAttribute());
            }

            return _validationAttributes = attrs
                // RequiredAttribute first (descending: true first)
                .OrderByDescending(a => a is RequiredAttribute)
                .ToList();
        }
    }
}
