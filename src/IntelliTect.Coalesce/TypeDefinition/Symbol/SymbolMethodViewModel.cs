using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public class SymbolMethodViewModel : MethodViewModel
    {
        internal IMethodSymbol Symbol { get; }

        public SymbolMethodViewModel(IMethodSymbol symbol, ClassViewModel parent) : base(parent)
        {
            Symbol = symbol;
        }

        public override string Name => Symbol.Name;

        public override string? Comment => Symbol.ExtractXmlComments();

        public override IEnumerable<AttributeViewModel<TAttribute>> GetAttributes<TAttribute>()
            => Symbol.GetAttributes<TAttribute>();

        public override bool IsStatic => Symbol.IsStatic;

        public override TypeViewModel ReturnType => SymbolTypeViewModel.GetOrCreate(Parent.ReflectionRepository, Symbol.ReturnType);

        public override bool IsInternalUse => base.IsInternalUse || Symbol.DeclaredAccessibility != Accessibility.Public;

        public override IEnumerable<ParameterViewModel> Parameters
            => Symbol.Parameters.Select(p => new SymbolParameterViewModel(this, p));
    }
}
