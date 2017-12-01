using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public class SymbolTypeViewModel : TypeViewModel
    {
        protected internal ITypeSymbol Symbol { get; internal set; }

        public SymbolTypeViewModel(ITypeSymbol symbol)
        {
            Symbol = symbol;
        }

        public INamedTypeSymbol NamedSymbol
        {
            get
            {
                if (Symbol is INamedTypeSymbol)
                {
                    return ((INamedTypeSymbol)Symbol);
                }
                else
                {
                    throw new InvalidCastException("Cannot cast to INamedTypeSymbol");
                }
            }
        }


        public override bool IsGeneric
        {
            get
            {
                try
                {
                    return NamedSymbol.IsGenericType;
                }
                catch (Exception)
                {
                    return false;
                }
            }

        }

        public override string Name => Symbol.Name;

        public override bool IsCollection =>
            !IsArray &&!IsString && Symbol.Interfaces.Any(f => f.Name.Contains("IEnumerable"));

        public override bool IsArray => Symbol.TypeKind == TypeKind.Array;

        /// <summary>
        /// Returns true if the property is nullable.
        /// </summary>
        public override bool IsNullable => Symbol.IsReferenceType || IsNullableType;

        public override bool IsNullableType => Symbol.Name.Contains(nameof(Nullable));

        public override bool IsClass => IsArray || Symbol.IsReferenceType;

        public override Dictionary<int, string> EnumValues
        {
            get
            {
                var result = new Dictionary<int, string>();
                // TODO: This needs to be fixed
                if (!IsArray)
                {
                    var enumType = NamedSymbol.EnumUnderlyingType;
                    var symbol = Symbol;
                    if (IsNullableType)
                    {
                        enumType = (PureType as SymbolTypeViewModel).NamedSymbol.EnumUnderlyingType;
                        symbol = (PureType as SymbolTypeViewModel).Symbol;
                    }
                    if (enumType != null)
                    {
                        foreach (var member in symbol.GetMembers())
                        {
                            if (member is IFieldSymbol)
                            {
                                result.Add((int)((IFieldSymbol)member).ConstantValue, member.Name);
                            }
                        }
                    }
                }
                return result;
            }
        }

        public override bool IsEnum => Symbol.TypeKind == TypeKind.Enum;

        public static readonly SymbolDisplayFormat DefaultDisplayFormat = SymbolDisplayFormat
            .FullyQualifiedFormat
            .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining)
            .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.RemoveAttributeSuffix);

        public override string FullyQualifiedName => Symbol.ToDisplayString(DefaultDisplayFormat);

        public override string FullNamespace =>
            (Symbol is IArrayTypeSymbol array ? array.ElementType : Symbol.ContainingSymbol).ToDisplayString(DefaultDisplayFormat);

        public override TypeViewModel FirstTypeArgument =>
            new SymbolTypeViewModel(NamedSymbol.TypeArguments.First());

        public override TypeViewModel ArrayType =>
            new SymbolTypeViewModel(((IArrayTypeSymbol)Symbol).ElementType);
        
        public override ClassViewModel ClassViewModel
        {
            get
            {
                if (!HasClassViewModel) return null;
                if (PureType != this) return PureType.ClassViewModel;
                if (Symbol != null && Symbol is INamedTypeSymbol) return ReflectionRepository.Global.GetClassViewModel(Symbol);
                return null;
            }
        }

        public override object GetAttributeValue<TAttribute>(string valueName) =>
            Symbol.GetAttributeValue<TAttribute>(valueName);

        public override bool HasAttribute<TAttribute>() =>
            Symbol.HasAttribute<TAttribute>();

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

            INamedTypeSymbol IsBase(INamedTypeSymbol symbol)
            {
                if (symbol == null) return null;
                if (symbol.MetadataName == typeToCheck.Name) return symbol;
                else if (symbol.BaseType != null) return IsBase(symbol.BaseType);
                return null;
            }

            // Check self & base classes.
            var baseSymbol = IsBase(Symbol as INamedTypeSymbol);
            if (baseSymbol != null) return baseSymbol;

            // Check interfaces.
            foreach (var symbol in Symbol.AllInterfaces)
            {
                if (symbol.MetadataName == typeToCheck.Name) return symbol;
            }

            return null;
        }


        /// <summary>
        /// Get the generic parameters used to satisfy the inheritance relationship with the given type.
        /// </summary>
        public override TypeViewModel[] GenericArgumentsFor(Type type)
        {
            var baseTypeSymbol = GetSatisfyingBaseTypeSymbol(type);
            if (baseTypeSymbol == null)
            {
                throw new ArgumentException($"{this} does not inherit from {type}");
            }

            return baseTypeSymbol.TypeArguments.Select(t => new SymbolTypeViewModel(t)).ToArray();
        }

        public override bool IsA(Type typeToCheck) => GetSatisfyingBaseTypeSymbol(typeToCheck) != null;

        public override bool IsA<T>() => IsA(typeof(T));

        public override bool EqualsType(TypeViewModel b) =>
            b is SymbolTypeViewModel s ? FullyQualifiedName == s.FullyQualifiedName : false;
    }
}
