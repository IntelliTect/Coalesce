using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace IntelliTect.Coalesce.TypeDefinition.Wrappers
{
    internal class SymbolPropertyWrapper : PropertyWrapper
    {
        protected IPropertySymbol Symbol;

        public SymbolPropertyWrapper(IPropertySymbol symbol)
        {
            Symbol = symbol;
        }

        public override string Name => Symbol.Name;

        public override string Comment => SymbolHelper.ExtractXmlComments(Symbol);

        public override object GetAttributeValue<TAttribute>(string valueName) =>
            Symbol.GetAttributeValue<TAttribute>(valueName);
        
        public override bool HasAttribute<TAttribute>() => Symbol.HasAttribute<TAttribute>();
        

        public override TypeWrapper Type => new SymbolTypeWrapper(Symbol.Type);

        public override bool HasGetter => !Symbol.IsWriteOnly;

        public override bool HasSetter => !Symbol.IsReadOnly;

        public override PropertyInfo PropertyInfo => throw new NullReferenceException("Symbol based types do not have a PropertyInfo.");

        public override bool IsStatic => Symbol.IsStatic;

        public override bool IsVirtual => Symbol.IsVirtual;

        public override bool IsInternalUse => base.IsInternalUse || Symbol.DeclaredAccessibility != Accessibility.Public;

    }
}
