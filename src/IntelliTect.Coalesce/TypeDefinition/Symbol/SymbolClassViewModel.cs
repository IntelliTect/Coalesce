﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using IntelliTect.Coalesce.Helpers;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public class SymbolClassViewModel : ClassViewModel
    {
        protected ITypeSymbol Symbol { get; }

        public SymbolClassViewModel(INamedTypeSymbol symbol) : this(new SymbolTypeViewModel(symbol))
        {
        }

        public SymbolClassViewModel(SymbolTypeViewModel typeViewModel) : base(typeViewModel)
        {
            Symbol = typeViewModel.Symbol;
        }

        internal static SymbolClassViewModel? GetOrCreate(ReflectionRepository? reflectionRepository, INamedTypeSymbol symbol)
        {
            return (reflectionRepository ?? ReflectionRepository.Global).GetClassViewModel(symbol) as SymbolClassViewModel;
        }

        public override string Name => Symbol.Name;

        public override string? Comment => Symbol.ExtractXmlComments();

        public override bool IsStatic => Symbol.IsStatic;

        public override bool IsRecord => Symbol.IsRecord;

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
                var parentSymbol = GetOrCreate(ReflectionRepository, Symbol.BaseType);
                if (parentSymbol != null)
                {
                    result.AddRange(parentSymbol.RawProperties(effectiveParent));
                }
            }
            return result.AsReadOnly();
        }

        protected override IReadOnlyCollection<MethodViewModel> RawMethods(ClassViewModel effectiveParent)
        {
            var result = Symbol.GetMembers()
                .Where(f => f.Kind == SymbolKind.Method && f.DeclaredAccessibility == Accessibility.Public)
                .Cast<IMethodSymbol>()
                .Select((s, i) => new SymbolMethodViewModel(s, this, effectiveParent))
                .Cast<MethodViewModel>()
                .ToList();

            // Add methods from the base class
            if (Symbol.BaseType != null && Symbol.BaseType.Name != "Object")
            {
                var parentSymbol = GetOrCreate(ReflectionRepository, Symbol.BaseType);
                if (parentSymbol != null)
                {
                    result.AddRange(parentSymbol.RawMethods(effectiveParent));
                }
            }

            // If this type is itself an interface, add inherited interfaces.
            // This is used for interface-based Service interfaces.
            if (Symbol.TypeKind == TypeKind.Interface)
            {
                foreach (var iface in Symbol.AllInterfaces)
                {
                    var parentSymbol = GetOrCreate(ReflectionRepository, iface);
                    if (parentSymbol != null)
                    {
                        result.AddRange(parentSymbol.RawMethods(effectiveParent));
                    }
                }
            }

            return result.AsReadOnly();
        }

        public override IReadOnlyCollection<MethodViewModel> Constructors => Symbol.GetMembers()
            .Where(m => m.Kind == SymbolKind.Method && m.DeclaredAccessibility == Accessibility.Public)
            .Cast<IMethodSymbol>()
            .Where(m => m.MethodKind == MethodKind.Constructor)
            .Select(m => new SymbolMethodViewModel(m, this, this))
            .Where(m => !m.IsInternalUse)
            .ToList();

        protected override IReadOnlyCollection<TypeViewModel> RawNestedTypes => Symbol
            .GetTypeMembers()
            .Select(t => SymbolTypeViewModel.GetOrCreate(ReflectionRepository, t))
            .ToList().AsReadOnly();

        public override IEnumerable<LiteralViewModel> ClientConsts => Symbol.GetMembers()
            .OfType<IFieldSymbol>()
            .Where(m => 
                m.IsConst &&
                m.HasConstantValue &&
                m.GetAttributeProvider().HasAttribute<CoalesceAttribute>()
            )
            .Select(m => new LiteralViewModel(SymbolTypeViewModel.GetOrCreate(ReflectionRepository, m.Type), m.ConstantValue!, m.Name));
    }
}