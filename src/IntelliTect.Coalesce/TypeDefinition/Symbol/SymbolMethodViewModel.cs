using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.TypeDefinition.Wrappers
{
    public class SymbolMethodViewModel : MethodViewModel
    {
        internal IMethodSymbol Symbol { get; private set; }

        public SymbolMethodViewModel(IMethodSymbol symbol, ClassViewModel parent) : base(parent)
        {
            Symbol = symbol;
        }

        public override string Name => Symbol.Name;

        public override string Comment => SymbolExtensions.ExtractXmlComments(Symbol);

        public override object GetAttributeValue<TAttribute>(string valueName) =>
            Symbol.GetAttributeValue<TAttribute>(valueName);
        
        public override bool HasAttribute<TAttribute>() => Symbol.HasAttribute<TAttribute>();
        
        public override bool IsStatic => Symbol.IsStatic;

        public override TypeViewModel ReturnType => new SymbolTypeViewModel(Symbol.ReturnType);

        public override bool IsInternalUse => base.IsInternalUse || Symbol.DeclaredAccessibility != Accessibility.Public;

        public override IEnumerable<ParameterViewModel> Parameters
            => Symbol.Parameters.Select(p => new SymbolParameterViewModel(p));
    }
}
