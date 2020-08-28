using System;
using System.Collections.Generic;
using System.Linq;
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
        }

        public override string Name => Symbol.Name;

        public override bool HasDefaultValue => Symbol.HasExplicitDefaultValue;

        protected override object? RawDefaultValue => Symbol.ExplicitDefaultValue;

        public override string CsDefaultValue => 
            ((ParameterSyntax)Symbol.DeclaringSyntaxReferences[0].GetSyntax()).Default?.Value.ToString()
            ?? throw new InvalidOperationException("Parameter has no default value");

        public override object? GetAttributeValue<TAttribute>(string valueName)
        {
            return Symbol.GetAttributeValue<TAttribute>(valueName);
        }

        public override bool HasAttribute<TAttribute>()
        {
            return Symbol.HasAttribute<TAttribute>();
        }
    }
}
