using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;


namespace IntelliTect.Coalesce.TypeDefinition.Wrappers
{
    internal abstract class TypeWrapper: Wrapper
    {
        protected internal Type Info { get; internal set; }
        protected internal ITypeSymbol Symbol { get; internal set; }


        public string NameWithTypeParams
        {
            get
            {
                if (IsArray) return $"{PureType.Name}[]";
                if (IsGeneric) return $"{Name}<{PureType.Name}>";
                return Name;
            }
        }
        public string FullyQualifiedNameWithTypeParams
        {
            get
            {
                if (IsArray) return $"{PureType.FullNamespace}.{PureType.Name}[]";
                if (IsGeneric) return $"{FullNamespace}.{Name}<{PureType.FullNamespace}.{PureType.Name}>";
                return $"{PureType.FullNamespace}.{Name}";
            }
        }

        public abstract string Namespace { get; }

        public abstract string FullNamespace { get; }
        
        public abstract bool IsGeneric { get; }

        public abstract bool IsCollection { get; }

        public abstract bool IsArray { get; }

        public abstract bool IsNullable { get; }

        public abstract bool IsNullableType { get; }

        public abstract bool IsClass { get; }

        public abstract Dictionary<int, string> EnumValues { get; }
        public abstract bool IsEnum { get; }



        public abstract TypeWrapper PureType { get; }

        public abstract TypeWrapper FirstTypeArgument { get; }

        public abstract TypeWrapper ArrayType { get; }


        public abstract bool IsA<T>();



        public bool IsNumber
        {
            get
            {
                switch (PureType.Name)
                {
                    case "Byte":
                    case "byte":
                    case "Int":
                    case "Single":
                    case "Double":
                    case "Decimal":
                        return true;
                }
                return false;
            }
        }


        /// <summary>
        /// True if this is a DateTime or DateTimeOffset.
        /// </summary>
        public bool IsDate
        {
            get { return IsDateTime || IsDateTimeOffset; }
        }

        public bool IsDateTime { get { return PureType.Name == "DateTime"; } }

        public bool IsDateTimeOffset { get { return PureType.Name == "DateTimeOffset"; } }
        public bool IsTimeZoneInfo { get { return PureType.Name == "TimeZoneInfo"; } }

        public bool IsString { get { return Name == "String"; } }
        public bool IsBool { get { return Name == "Boolean"; } }

        public bool IsPrimitive
        {
            get
            {
                return IsString || IsNumber || IsBool || IsEnum;
            }
        }


        /// <summary>
        /// Creates or gets the ClassViewModel. Only for external classes.
        /// </summary>
        public ClassViewModel ClassViewModel
        {
            get
            {
                if (Symbol != null && Symbol is INamedTypeSymbol) return ReflectionRepository.GetClassViewModel(Symbol as INamedTypeSymbol);
                if (Info != null) return ReflectionRepository.GetClassViewModel(Info);
                return null;
            }
        }


    }
}
