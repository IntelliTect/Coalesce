using IntelliTect.Coalesce.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public abstract class TypeViewModel : IAttributeProvider
    {
        public abstract string Name { get; }

        public abstract string FullyQualifiedName { get; }

        public abstract string FullNamespace { get; }

        public abstract bool IsGeneric { get; }

        public abstract bool IsCollection { get; }

        public abstract bool IsArray { get; }

        public abstract bool IsNullable { get; }

        public abstract bool IsNullableType { get; }

        public abstract bool IsClass { get; }

        public abstract bool IsInterface { get; }

        /// <summary>
        /// Returns true if this TypeViewModel represents void.
        /// </summary>
        public abstract bool IsVoid { get; }

        public abstract Dictionary<int, string> EnumValues { get; }
        public abstract bool IsEnum { get; }

        public abstract TypeViewModel FirstTypeArgument { get; }

        public abstract TypeViewModel ArrayType { get; }

        public abstract bool IsA(Type type);
        public abstract TypeViewModel[] GenericArgumentsFor(Type type);

        public bool IsA<T>() => IsA(typeof(T));

        /// <summary>
        /// Convenient accessor for the represented System.Type when in reflection-based contexts.
        /// </summary>
        public virtual Type TypeInfo => throw new InvalidOperationException("TypeInfo not available in the current context");

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
                return $"default({FullyQualifiedName})";
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

        /// <summary>
        /// True if this is a boolean.
        /// </summary>
        public bool IsBool => NullableUnderlyingType.Name == nameof(Boolean);

        public bool IsPrimitive => IsString || IsNumber || IsBool || IsEnum;

        /// <summary>
        /// Best approximation of a TypeScript type definition for the type.
        /// </summary>
        public string TsType
        {
            get
            {
                if (IsByteArray) return "string";
                if (IsCollection && IsNumber) return "number[]";
                if (IsCollection) return PureType.TsTypePlain + "[]";
                return TsTypePlain;
            }
        }

        /// <summary>
        /// Exrepssion that will convert from a string to the data's actual type.
        /// </summary>
        public string TsConvertFromString(string expression)
        {
            if (IsBool) return $"({expression}.toUpperCase() == 'TRUE')";
            if (IsEnum || IsIntegral) return $"parseInt({expression})";
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
                if (IsString) return "string";
                if (IsBool) return "boolean";
                if (IsDate) return "moment.Moment";
                if (IsEnum) return "number";
                if (IsNumber) return "number";
                if (IsPOCO) return $"ViewModels.{PureType.Name}";
                if (IsClass) return PureType.Name;
                return "any";
            }
        }

        public virtual bool IsInternalUse => HasAttribute<InternalUseAttribute>();

        public bool HasClassViewModel => !IsPrimitive && IsPOCO;

        public abstract ClassViewModel ClassViewModel { get; }


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
        public bool IsDateTime => NullableUnderlyingType.Name == "DateTime";

        /// <summary>
        /// True if the property is a DateTimeOffset or Nullable DateTimeOffset
        /// </summary>
        public bool IsDateTimeOffset => NullableUnderlyingType.Name == "DateTimeOffset";

        /// <summary>
        /// Returns true if class is a Byte[]
        /// </summary>
        public bool IsByteArray => PureType.Name == nameof(Byte) && IsArray;
        
        /// <summary>
        /// Returns true if the type is any integral type, except <see cref="char"/>
        /// </summary>
        public bool IsIntegral
        {
            get 
            {
                switch (NullableUnderlyingType.Name)
                {
                    case nameof(SByte):
                    case nameof(Byte):
                    case nameof(Int16):
                    case nameof(UInt16):
                    case nameof(Int32):
                    case nameof(UInt32):
                    case nameof(Int64):
                    case nameof(UInt64):
                        return true;
                    default:
                        return false;
                }
            }
        }

        /// <summary>
        /// Returns true if the class is a number.
        /// </summary>
        public bool IsNumber
        {
            get
            {
                if (IsIntegral) return true;

                switch (NullableUnderlyingType.Name)
                {
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
        /// If this represents a nullable type, returns the underlying type that is nullable.
        /// Otherwise, returns the current instance.
        /// </summary>
        public TypeViewModel NullableUnderlyingType => (IsNullable && IsA(typeof(Nullable<>))) ? FirstTypeArgument : this;

        /// <summary>
        /// Gets the type name without any collection or nullable around it.
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
        /// Returns true if the property is class outside the System namespace, and is not a string or array
        /// </summary>
        public bool IsPOCO => !IsArray && !IsCollection && !FullNamespace.StartsWith("System") && IsClass;

        public string TsDeclaration => $"{Name}: {TsType}";

        public string TsDeclarationPlain(string parameterName) => $"{parameterName}: {TsTypePlain}";
        
        public abstract object GetAttributeValue<TAttribute>(string valueName) where TAttribute : Attribute;
        public abstract bool HasAttribute<TAttribute>() where TAttribute : Attribute;


        public string NullableTypeForDto(string dtoNamespace)
        {
            var model = this.PureType.ClassViewModel;
            if (model != null)
            {
                string typeName = "";

                if (IsNullable || IsArray || IsCollection)
                    typeName = FullyQualifiedName;
                else
                    typeName = Name + "?";

                var regex = new Regex($@"({model.Name}(?!DtoGen))(>|$)");
                typeName = regex.Replace(typeName, $@"{model.Name}DtoGen$2");
                typeName = typeName.Replace(model.Type.FullNamespace, dtoNamespace);

                return typeName;
            }
            else
            {
                if (IsNullable || IsArray)
                    return FullyQualifiedName;
                else
                    return FullyQualifiedName + "?";
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is TypeViewModel that)) return false;

            return this.FullyQualifiedName == that.FullyQualifiedName;
        }

        public override int GetHashCode() => this.FullyQualifiedName.GetHashCode();

        // TODO: maybe retire this in favor of plain .Equals? Make sure that ReflectionTypeViewModel.FullyQualifiedName is correct, first.
        public abstract bool EqualsType(TypeViewModel b);

        public override string ToString() => FullyQualifiedName;
    }
}
