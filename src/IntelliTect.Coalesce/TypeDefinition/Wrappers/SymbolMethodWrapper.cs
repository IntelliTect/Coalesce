using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.TypeDefinition.Wrappers
{
    internal class SymbolMethodWrapper : MethodWrapper
    {
        public IMethodSymbol Symbol { get; protected set; }

        public SymbolMethodWrapper(IMethodSymbol symbol)
        {
            Symbol = symbol;
        }

        public override string Name => Symbol.Name;

        public override string Comment => SymbolHelper.ExtractXmlComments(Symbol);

        public override object GetAttributeValue<TAttribute>(string valueName) =>
            Symbol.GetAttributeValue<TAttribute>(valueName);
        
        public override bool HasAttribute<TAttribute>() => Symbol.HasAttribute<TAttribute>();
        
        public override bool IsStatic => Symbol.IsStatic;

        public override TypeWrapper ReturnType => new SymbolTypeWrapper(Symbol.ReturnType);

        public override bool IsInternalUse => base.IsInternalUse || Symbol.DeclaredAccessibility != Accessibility.Public;

        public override IEnumerable<ParameterViewModel> Parameters
        {
            get
            {
                var result = new List<ParameterViewModel>();
                foreach (var parameter in Symbol.Parameters)
                {
                    result.Add(new ParameterViewModel( new SymbolParameterWrapper(parameter)));
                }
                return result;
            }
        }
    }
}
