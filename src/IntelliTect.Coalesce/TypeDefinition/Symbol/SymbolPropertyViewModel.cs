using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System.Text.RegularExpressions;

namespace IntelliTect.Coalesce.TypeDefinition
{
    internal class SymbolPropertyViewModel : PropertyViewModel
    {
        protected IPropertySymbol Symbol;

        public SymbolPropertyViewModel(ClassViewModel effectiveParent, ClassViewModel declaringParent, IPropertySymbol symbol)
            : base(effectiveParent, declaringParent, SymbolTypeViewModel.GetOrCreate(declaringParent.ReflectionRepository, symbol.Type))
        {
            Symbol = symbol;
#if NET6_0_OR_GREATER
            if (symbol.Type.IsReferenceType)
            {
                // This is naive and doesn't capture the full nullable behavior and nuances of attributes.
                // But its probably good enough for most use cases.
                ReadNullability = WriteNullability = symbol.NullableAnnotation switch
                {
                    NullableAnnotation.Annotated => NullabilityState.Nullable,
                    NullableAnnotation.NotAnnotated => NullabilityState.NotNull,
                    _ => NullabilityState.Unknown
                };
            }
#endif
        }

        public override string Name => Symbol.Name;

        public override string? Comment => Symbol.ExtractXmlComments();

        public override object? GetAttributeValue<TAttribute>(string valueName) =>
            Symbol.GetAttributeValue<TAttribute>(valueName);
        
        public override bool HasAttribute<TAttribute>() => Symbol.HasAttribute<TAttribute>();
        
        public override bool HasGetter => !Symbol.IsWriteOnly;

        public override bool HasSetter => !Symbol.IsReadOnly;

        public override bool HasPublicSetter => HasSetter && Symbol.SetMethod?.DeclaredAccessibility == Accessibility.Public;

        public override bool IsStatic => Symbol.IsStatic;

        public override bool IsInitOnly => Symbol.SetMethod?.IsInitOnly == true;

        public override bool HasRequiredKeyword =>
#if NETCOREAPP3_1_OR_GREATER
            Symbol.IsRequired;
#else
            false;
#endif

        public override bool IsVirtual => Symbol.IsVirtual;

        public override bool IsInternalUse => base.IsInternalUse || Symbol.DeclaredAccessibility != Accessibility.Public;
    }
}
