using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace IntelliTect.Coalesce.TypeDefinition.Wrappers
{
    internal class SymbolParameterWrapper : ParameterWrapper
    {
        public SymbolParameterWrapper(IParameterSymbol symbol)
        {
            Symbol = symbol;
            Type = new TypeViewModel(new SymbolTypeWrapper(symbol.Type));
        }

        public override string Name => Symbol.Name;

        public override object GetAttributeValue<TAttribute>(string valueName)
        {
            return Symbol.GetAttributeValue<TAttribute>(valueName);
        }

        public override bool HasAttribute<TAttribute>()
        {
            return Symbol.HasAttribute<TAttribute>();
        }
    }
}
