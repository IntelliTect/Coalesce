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
        }

        public override string Name => Symbol.Name;

        public override string Comment => Symbol.ExtractXmlComments();

        public override object? GetAttributeValue<TAttribute>(string valueName) =>
            Symbol.GetAttributeValue<TAttribute>(valueName);
        
        public override bool HasAttribute<TAttribute>() => Symbol.HasAttribute<TAttribute>();
        
        public override bool HasGetter => !Symbol.IsWriteOnly;

        public override bool HasSetter => !Symbol.IsReadOnly;

        public override bool HasPublicSetter => HasSetter && Symbol.SetMethod?.DeclaredAccessibility == Accessibility.Public;

        public override bool IsStatic => Symbol.IsStatic;

        public override bool IsVirtual => Symbol.IsVirtual;

        public override bool IsInternalUse => base.IsInternalUse || Symbol.DeclaredAccessibility != Accessibility.Public;
    }
}
