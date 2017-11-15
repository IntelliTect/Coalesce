using IntelliTect.Coalesce.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using IntelliTect.Coalesce.TypeDefinition.Wrappers;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public abstract class TypeViewModel : IAttributeProvider
    {
        public abstract string Name { get; }

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

        public abstract TypeViewModel FirstTypeArgument { get; }

        public abstract TypeViewModel ArrayType { get; }

        public abstract bool IsA<T>();

        public string CsDefaultValue
        {
            get
            {
                if (IsString) { return "\"\""; }
                if (IsPOCO) { return "null"; }
                if (IsEnum) { return "0"; }
                if (IsNumber) { return "0"; }
                if (IsDateTime) { return "DateTime.MinValue"; }
                if (IsDateTimeOffset) { return "DateTimeOffset.MinValue"; }
                if (IsBool) { return "false"; }
                return "null";
            }
        }

        public string CsConvertFromString
        {
            get
            {
                if (IsString) return "";
                if (IsPOCO) return "(object)";
                if (IsEnum) return "Convert.ToInt32";
                if (IsNumber) return "Convert.To" + Name;
                if (IsDateTime) return "DateTime.Parse";
                if (IsDateTimeOffset) return "DateTimeOffset.Parse";
                if (IsBool) return "Convert.ToBoolean";
                return "(object)";
            }
        }

        public string FullName => $"{Namespace}.{Name}";

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

        /// <summary>
        /// True if this is a boolean.
        /// </summary>
        public bool IsBool => Name == "Boolean";

        /// <summary>
        /// Returns true if the property returns void.
        /// </summary>
        public bool IsVoid => Name == "Void";

        public bool IsPrimitive => IsString || IsNumber || IsBool || IsEnum;

        /// <summary>
        /// Best approximation of a TypeScript type definition for the type.
        /// </summary>
        public string TsType
        {
            get
            {
                if (IsCollection && IsNumber) return "number[]";
                if (IsCollection) return PureType + "[]";
                return TsTypePlain;
            }
        }

        /// <summary>
        /// Exrepssion that will convert from a string to the data's actual type.
        /// </summary>
        public string TsConvertFromString(string expression)
        {
            if (IsBool) return $"({expression}.toUpperCase() == 'TRUE')";
            if (IsEnum) return $"parseInt({expression})";
            if (IsNumber) return $"parseFloat({expression})";
            if (IsDate) return $"moment({expression})";
            return expression;
        }


        /// <summary>
        /// Best approximation of a TypeScript type definition for the type, not accounting for arrays.
        /// Collection types will be typed as "any". Use TsType to get correct collection types.
        /// </summary>
        public string TsTypePlain
        {
            get
            {
                if (IsBool) return "boolean";
                if (IsDate) return "moment.Moment";
                if (IsPOCO) return $"ViewModels.{PureType.Name}";
                if (IsClass) return PureType.Name;
                if (IsEnum) return "number";
                if (IsNumber) return "number";
                return "any";
            }
        }


        public bool HasClassViewModel => !IsPrimitive && PureType.IsPOCO;

        public abstract ClassViewModel ClassViewModel { get; }


        /// <summary>
        /// Returns true if this is a complex type.
        /// </summary>
        public bool IsComplexType => false;

        /// <summary>
        /// True if this is a DateTime or DateTimeOffset.
        /// </summary>
        public bool IsDate => IsDateTime || IsDateTimeOffset;



        /// <summary>
        /// True if the property is a string.
        /// </summary>
        public bool IsString => Name == "String";

        /// <summary>
        /// True if the property is a DateTime or Nullable DateTime
        /// </summary>
        public bool IsDateTime => PureType.Name == "DateTime";

        /// <summary>
        /// True if the property is a DateTimeOffset or Nullable DateTimeOffset
        /// </summary>
        public bool IsDateTimeOffset => PureType.Name == "DateTimeOffset";

        /// <summary>
        /// Returns true if class is a Byte[]
        /// </summary>
        public bool IsByteArray => PureType.Name == nameof(Byte) && IsArray;

        /// <summary>
        /// Returns true if the class is a number.
        /// </summary>
        public bool IsNumber
        {
            get
            {
                switch (PureType.Name)
                {
                    case nameof(Byte):
                    case nameof(Int16):
                    case nameof(UInt16):
                    case nameof(Int32):
                    case nameof(UInt32):
                    case nameof(Int64):
                    case nameof(UInt64):
                    case nameof(Single):
                    case nameof(Double):
                    case nameof(Decimal):
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Gets the type name without any collection around it.
        /// </summary>
        public TypeViewModel PureType
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

        /// <summary>
        /// Returns true if the property is class outside the System namespac, and is not a string, array, or filedownload
        /// </summary>
        public bool IsPOCO => !IsArray && !IsCollection && !FullName.StartsWith("System") && IsClass;

        public string TsDeclaration => $"{Name}: {TsType}";

        public string TsDeclarationPlain(string parameterName) => $"{parameterName}: {TsTypePlain}";

        public string CsDeclaration(string parameterName) => $"{NameWithTypeParams} {parameterName.ToCamelCase()}";
        public abstract object GetAttributeValue<TAttribute>(string valueName) where TAttribute : Attribute;
        public abstract bool HasAttribute<TAttribute>() where TAttribute : Attribute;


        public string NullableTypeForDto
        {
            get
            {
                var model = this.ClassViewModel;
                if (model != null)
                {
                    string typeName = "";

                    if (IsNullable || IsArray || IsCollection)
                        typeName = NameWithTypeParams;
                    else
                        typeName = Name + "?";

                    typeName = (new Regex($"({model.Name}(?!(DtoGen)))")).Replace(typeName, $"{model.Name}DtoGen");

                    return typeName;
                }
                else
                {
                    if (IsNullable || IsArray)
                        return FullyQualifiedNameWithTypeParams;
                    else
                        return Name + "?";
                }

            }
        }

        public string ExplicitConversionType
        {
            get
            {
                if (IsNullable || IsArray) return "";
                else return $"({Name})";
            }
        }
    }
}
