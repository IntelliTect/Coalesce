using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.TypeDefinition.Wrappers
{
    internal class SymbolTypeWrapper : TypeWrapper
    {
        public SymbolTypeWrapper(ITypeSymbol symbol)
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

        public override string Name { get { return Symbol.Name; } }

        public override bool IsCollection
        {
            get
            {
                return !IsArray &&
                    !IsString &&
                    Symbol.Interfaces.Any(f => f.Name.Contains("IEnumerable"));
            }
        }

        public override bool IsArray { get { return Symbol.TypeKind == TypeKind.Array; } }

        /// <summary>
        /// Returns true if the property is nullable.
        /// </summary>
        public override bool IsNullable
        {
            get
            {
                if (!IsArray) return Symbol.IsReferenceType || IsNullableType;
                return false;
            }
        }

        public override bool IsNullableType
        {
            get
            {
                return Symbol.Name.Contains(nameof(Nullable));
            }
        }

        public override bool IsClass
        {
            get
            {
                if (IsArray) return true;
                return Symbol.IsReferenceType;
            }
        }

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
                        enumType = (PureType as SymbolTypeWrapper).NamedSymbol.EnumUnderlyingType;
                        symbol = PureType.Symbol;
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

        public override bool IsEnum
        {
            get
            {
                if (IsArray) { return false; }
                else { return Symbol.TypeKind == TypeKind.Enum; }
            }
        }

        public override string Namespace
        {
            get
            {
                if (Symbol.ContainingNamespace == null) return Symbol.BaseType.ContainingNamespace.Name;
                return Symbol.ContainingNamespace.Name;
            }
        }

        public override string FullNamespace
        {
            get
            {
                INamespaceSymbol currentNamespace = Symbol.ContainingNamespace ?? Symbol.BaseType.ContainingNamespace;
                var fullNamespace = currentNamespace.Name;
                while (currentNamespace != null)
                {
                    currentNamespace = currentNamespace.ContainingNamespace;
                    if (currentNamespace != null && !string.IsNullOrEmpty(currentNamespace.Name)) fullNamespace = currentNamespace.Name + "." + fullNamespace;
                }

                return fullNamespace;
            }
        }

        public override TypeWrapper PureType
        {
            get
            {
                if (IsArray)
                {
                    return ArrayType;
                }
                if (IsGeneric && (IsCollection || IsNullable)) { return FirstTypeArgument; }
                return this;
            }
        }

        public override TypeWrapper FirstTypeArgument
        {
            get
            {
                return new SymbolTypeWrapper(NamedSymbol.TypeArguments.First());
            }
        }

        public override TypeWrapper ArrayType
        {
            get
            { return new SymbolTypeWrapper(((IArrayTypeSymbol)Symbol).ElementType); }
        }

        public override object GetAttributeValue<TAttribute>(string valueName)
        {
            return Symbol.GetAttributeValue<TAttribute>(valueName);
        }
        public override bool HasAttribute<TAttribute>()
        {
            return Symbol.HasAttribute<TAttribute>();
        }

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
    }
}
