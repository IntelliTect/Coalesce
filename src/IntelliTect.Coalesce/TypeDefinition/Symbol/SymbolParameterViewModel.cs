using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace IntelliTect.Coalesce.TypeDefinition
{
    internal class SymbolParameterViewModel : ParameterViewModel
    {
        protected internal IParameterSymbol Symbol { get; internal set; }

        public SymbolParameterViewModel(IParameterSymbol symbol)
        {
            Symbol = symbol;
            Type = new SymbolTypeViewModel(symbol.Type);
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
