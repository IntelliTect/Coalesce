using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.TypeDefinition.Wrappers
{
    internal class SymbolTypeViewModel : TypeViewModel
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

        public override bool IsA<T>()
        {
            // Check base classes.
            if (IsA<T>(Symbol)) return true;
            // Check interfaces.
            foreach (var symbol in Symbol.AllInterfaces)
            {
                if (symbol.Name == typeof(T).Name) return true;
            }
            return false;
        }

        /// <summary>
        /// Checks inheritance tree.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="symbol"></param>
        /// <returns></returns>
        private bool IsA<T>(ITypeSymbol symbol)
        {
            if (symbol.Name == typeof(T).Name) return true;
            else if (symbol.BaseType != null) return IsA<T>(symbol.BaseType);
            return false;
        }

        public override bool EqualsType(TypeViewModel b) =>
            b is SymbolTypeViewModel s ? FullyQualifiedName == s.FullyQualifiedName : false;
    }
}
