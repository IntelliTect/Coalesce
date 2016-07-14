using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.TypeDefinition.Wrappers
{
    internal class SymbolMethodWrapper : MethodWrapper
    {
        protected IMethodSymbol Symbol;

        public SymbolMethodWrapper(IMethodSymbol symbol)
        {
            Symbol = symbol;
        }

        public override string Name
        {
            get
            {
                return Symbol.Name;
            }
        }

        public override string Comment { get { return SymbolHelper.ExtractXmlComments(Symbol) ; } }

        public override object GetAttributeValue<TAttribute>(string valueName) 
        {
            return Symbol.GetAttributeValue<TAttribute>(valueName);
        }
        public override bool HasAttribute<TAttribute>()
        {
            return Symbol.HasAttribute<TAttribute>();
        }

        public override bool IsStatic
        {
            get
            {
                return Symbol.IsStatic;
            }
        }

        public override TypeWrapper ReturnType
        {
            get
            {
                return new SymbolTypeWrapper(Symbol.ReturnType);
            }
        }

        public override IEnumerable<ParameterViewModel> Parameters
        {
            get
            {
                var result = new List<ParameterViewModel>();
                foreach (var parameter in Symbol.Parameters)
                {
                    result.Add(new ParameterViewModel(parameter.Name, new TypeViewModel(new SymbolTypeWrapper(parameter.Type))));
                }
                return result;
            }
        }
    }
}
