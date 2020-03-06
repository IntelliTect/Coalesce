using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using IntelliTect.Coalesce.Helpers;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public class SymbolClassViewModel : ClassViewModel
    {
        protected ITypeSymbol Symbol { get; }

        public SymbolClassViewModel(INamedTypeSymbol symbol)
        {
            Symbol = symbol;
            Type = new SymbolTypeViewModel(Symbol);
        }

        public override string Name => Symbol.Name;

        public override string Comment => Symbol.ExtractXmlComments();

        protected override IReadOnlyCollection<PropertyViewModel> RawProperties(ClassViewModel effectiveParent)
        {
            var result = Symbol.GetMembers()
                .Where(s => s.Kind == SymbolKind.Property)
                .Cast<IPropertySymbol>()
                .Select((s, i) => new SymbolPropertyViewModel(effectiveParent, this, s) { ClassFieldOrder = i })
                .Cast<PropertyViewModel>()
                .ToList();

            // Add properties from the base class
            if (Symbol.BaseType != null && Symbol.BaseType.Name != "Object")
            {
                var parentSymbol = new SymbolClassViewModel(Symbol.BaseType);
                result.AddRange(parentSymbol.RawProperties(effectiveParent));
            }
            return result.AsReadOnly();
        }

        protected override IReadOnlyCollection<MethodViewModel> RawMethods
        {
            get
            {
                var result = Symbol.GetMembers()
                    .Where(f => f.Kind == SymbolKind.Method && f.DeclaredAccessibility == Accessibility.Public)
                    .Cast<IMethodSymbol>()
                    .Where(f => f.MethodKind == MethodKind.Ordinary)
                    .Select(s => new SymbolMethodViewModel(s, this))
                    .ToList();

                void AddSymbolMethods(INamedTypeSymbol symbol)
                {
                    var parentSymbol = new SymbolClassViewModel(symbol);
                    result.AddRange(parentSymbol.Methods
                        .Cast<SymbolMethodViewModel>()
                        // Don't add overriden methods
                        .Where(baseMethod => !result.Any(method => SymbolEqualityComparer.Default.Equals(method.Symbol.OverriddenMethod, baseMethod.Symbol))
                    ));
                }

                // Add methods from the base class
                if (Symbol.BaseType != null && Symbol.BaseType.Name != "Object")
                {
                    AddSymbolMethods(Symbol.BaseType);
                }

                // If this type is itself an interface, add inherited interfaces.
                // This is used for interface-based Service interfaces.
                if (Symbol.TypeKind == TypeKind.Interface)
                {
                    foreach (var iface in Symbol.AllInterfaces)
                    {
                        AddSymbolMethods(iface);

                    }
                }

                return result.Distinct().ToList().AsReadOnly();
            }
        }

        protected override IReadOnlyCollection<TypeViewModel> RawNestedTypes => Symbol
            .GetTypeMembers()
            .Select(t => new SymbolTypeViewModel(t))
            .ToList().AsReadOnly();
    }
}