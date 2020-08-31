using Microsoft.CodeAnalysis;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.TypeDefinition
{

#pragma warning disable CS0659 // Intentionally using base class's GetHashCode

    public class SymbolTypeViewModel : TypeViewModel
    {
        protected internal ITypeSymbol Symbol { get; internal set; }

        public SymbolTypeViewModel(ITypeSymbol symbol) : this(null, symbol)
        {
        }

        internal SymbolTypeViewModel(ReflectionRepository? reflectionRepository, ITypeSymbol symbol) : base(reflectionRepository)
        {
            Symbol = symbol;

            FirstTypeArgument = IsGeneric && NamedSymbol.Arity > 0
                ? SymbolTypeViewModel.GetOrCreate(reflectionRepository, NamedSymbol.TypeArguments.First())
                : null;

            ArrayType = IsArray
                ? SymbolTypeViewModel.GetOrCreate(reflectionRepository, ((IArrayTypeSymbol)Symbol).ElementType) 
                : null;

            var types = new List<INamedTypeSymbol>();
            var target = Symbol as INamedTypeSymbol;
            while (target != null)
            {
                types.Add(target);
                target = target.BaseType;
            }
            foreach (var iface in Symbol.AllInterfaces)
            {
                types.Add(iface);
            }
            AssignableToLookup = types.ToLookup(t => t.MetadataName);

            ClassViewModel = ShouldCreateClassViewModel && Symbol is INamedTypeSymbol nts
                ? new SymbolClassViewModel(this)
                : null;

            // These are precomputed because they are used for .Equals() and the == operator.
            FullyQualifiedName = Symbol.ToDisplayString(DefaultDisplayFormat);
            // TODO: write tests that assert that this will format types the same 
            // way as ReflectionTypeViewModel.VerboseFullyQualifiedName. Adjust either one as needed.
            VerboseFullyQualifiedName = Symbol.ToDisplayString(VerboseDisplayFormat);
    }

        internal static SymbolTypeViewModel GetOrCreate(ReflectionRepository? reflectionRepository, ITypeSymbol symbol)
        {
            return reflectionRepository?.GetOrAddType(symbol) ?? new SymbolTypeViewModel(reflectionRepository, symbol);
        }

        public INamedTypeSymbol NamedSymbol => Symbol as INamedTypeSymbol ?? throw new InvalidCastException("Cannot cast to INamedTypeSymbol");

        public override bool IsGeneric => (Symbol as INamedTypeSymbol)?.IsGenericType ?? false;

        public override string Name => Symbol.Name;

        public override bool IsArray => Symbol.TypeKind == TypeKind.Array;

        /// <summary>
        /// Returns true if the property is nullable.
        /// </summary>
        public override bool IsNullable => Symbol.IsReferenceType || IsNullableType;

        public override bool IsClass => IsArray || Symbol.IsReferenceType;

        public override bool IsInterface => Symbol.TypeKind == TypeKind.Interface;

        public override bool IsInternalUse => base.IsInternalUse || Symbol.DeclaredAccessibility != Accessibility.Public;

        public override bool IsVoid => Symbol.SpecialType == SpecialType.System_Void;

        public override Dictionary<int, string> EnumValues
        {
            get
            {
                if (IsNullableType) return NullableUnderlyingType.EnumValues;

                var result = new Dictionary<int, string>();

                if (!IsEnum) return result;

                var enumType = NamedSymbol.EnumUnderlyingType;
                var symbol = Symbol;

                if (enumType != null)
                {
                    foreach (var member in symbol.GetMembers().OfType<IFieldSymbol>())
                    {
                        result.Add(Convert.ToInt32(member.ConstantValue!), member.Name);
                    }
                }

                return result;
            }
        }

        public override bool IsEnum =>
            IsNullableType ? NullableUnderlyingType.IsEnum : Symbol.TypeKind == TypeKind.Enum;

        public static readonly SymbolDisplayFormat DefaultDisplayFormat = SymbolDisplayFormat
            .FullyQualifiedFormat
            .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining)
            .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.RemoveAttributeSuffix);

        public override string FullyQualifiedName { get; }

        public static readonly SymbolDisplayFormat VerboseDisplayFormat = DefaultDisplayFormat
            .RemoveMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.UseSpecialTypes)
            .RemoveMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.RemoveAttributeSuffix)
            .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.ExpandNullable);

        public override string VerboseFullyQualifiedName { get; }

        public override string FullNamespace =>
            (Symbol is IArrayTypeSymbol array ? array.ElementType : Symbol.ContainingSymbol).ToDisplayString(DefaultDisplayFormat);

        public override TypeViewModel? FirstTypeArgument { get; }

        public override TypeViewModel? ArrayType { get; }

        public override ClassViewModel? ClassViewModel { get; }

        public override object? GetAttributeValue<TAttribute>(string valueName) =>
            Symbol.GetAttributeValue<TAttribute>(valueName);

        public override bool HasAttribute<TAttribute>() =>
            Symbol.HasAttribute<TAttribute>();

        /// <summary>
        /// The set of types that an object of the current represented type is assignable to, 
        /// keyed by a base type's <see cref="ISymbol.MetadataName"/>.
        /// </summary>
        private ILookup<string, INamedTypeSymbol> AssignableToLookup { get; }

        /// <summary>
        /// Find the ITypeSymbol that satisfies the inheritance relationship "this : typeToCheck"
        /// </summary>
        /// <param name="typeToCheck"></param>
        /// <returns></returns>
        private INamedTypeSymbol GetSatisfyingBaseTypeSymbol(Type typeToCheck)
        {
            if (typeToCheck.IsConstructedGenericType)
            {
                throw new ArgumentException(
                    "SymbolTypeViewModels can't currently check if a symbol is assignable to a specific constructed generic. " +
                    "It can only check against non-constructed generics.", nameof(typeToCheck));
            }

            // KNOWN SHORTCOMING: This method only checks the name of the type, and not its namespace.
            // For now, this is OK, but should probably be improved in the future so that MyNamespace.String != System.String.

            return AssignableToLookup[typeToCheck.Name].FirstOrDefault();
        }


        /// <summary>
        /// Get the generic parameters used to satisfy the inheritance relationship with the given type.
        /// </summary>
        public override TypeViewModel[]? GenericArgumentsFor(Type type) =>
            GetSatisfyingBaseTypeSymbol(type)?
            .TypeArguments
            .Select(t => SymbolTypeViewModel.GetOrCreate(ReflectionRepository, t))
            .ToArray();

        public override bool IsA(Type type) => GetSatisfyingBaseTypeSymbol(type) != null;

        public override bool Equals(object? obj)
        {
            if (!(obj is SymbolTypeViewModel that)) return base.Equals(obj);

            return SymbolEqualityComparer.IncludeNullability.Equals(Symbol, that.Symbol);
        }
    }
}
