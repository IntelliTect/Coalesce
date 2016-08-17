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

        public override string Name { get { return Symbol.Name; } }

        public override string Comment { get { return SymbolHelper.ExtractXmlComments(Symbol); } }

        public override object GetAttributeValue<TAttribute>(string valueName)
        {
            return (Symbol).GetAttributeValue<TAttribute>(valueName);
        }
        public override bool HasAttribute<TAttribute>()
        {
            return ((IPropertySymbol)Symbol).HasAttribute<TAttribute>();
        }

        public override TypeWrapper Type { get { return new SymbolTypeWrapper(Symbol.Type); } }

        public override bool CanRead { get { return !Symbol.IsWriteOnly; } }

        public override bool CanWrite { get { return !Symbol.IsReadOnly; } }

        public override bool IsReadOnly { get { return Symbol.IsReadOnly; } }

        public override PropertyInfo PropertyInfo { get { throw new NullReferenceException("Symbol based types do not have a PropertyInfo."); } }

        public override bool IsStatic
        {
            get
            {
                return Symbol.IsStatic;
            }
        }
        public override bool IsVirtual
        {
            get
            {
                return Symbol.IsVirtual;
            }
        }
    }
}
