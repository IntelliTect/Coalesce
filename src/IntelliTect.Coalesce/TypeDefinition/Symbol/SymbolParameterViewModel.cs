using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace IntelliTect.Coalesce.TypeDefinition
{
    internal class SymbolParameterViewModel : ParameterViewModel
    {
        protected internal IParameterSymbol Symbol { get; internal set; }

        public SymbolParameterViewModel(MethodViewModel parent, IParameterSymbol symbol)
            : base(parent, SymbolTypeViewModel.GetOrCreate(parent.Parent.ReflectionRepository, symbol.Type))
        {
            Symbol = symbol;
            if (symbol.Type.IsReferenceType)
            {
                // This is naive and doesn't capture the full nullable behavior and nuances of attributes.
                // But its probably good enough for most use cases.
                Nullability = symbol.NullableAnnotation switch
                {
                    NullableAnnotation.Annotated => NullabilityState.Nullable,
                    NullableAnnotation.NotAnnotated => NullabilityState.NotNull,
                    _ => NullabilityState.Unknown
                };
            }
        }

        public override string Name => Symbol.Name;

        public override bool HasDefaultValue => Symbol.HasExplicitDefaultValue;

        protected override object? RawDefaultValue => Symbol.ExplicitDefaultValue;

        public override IEnumerable<AttributeViewModel<TAttribute>> GetAttributes<TAttribute>()
            => Symbol.GetAttributes<TAttribute>();
    }
}
