using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace IntelliTect.Coalesce.TypeDefinition
{

    public abstract class TypeViewModel : IAttributeProvider
    {
        public abstract string Name { get; }

        /// <summary>
        /// Returns the name of the type to be used by the client.
        /// </summary>
        public string ClientTypeName => IsNullableType
            ? NullableUnderlyingType.ClientTypeName
            : this.GetAttributeValue<CoalesceAttribute>(a => a.ClientTypeName) ?? Name;

        public abstract string FullyQualifiedName { get; }

        public abstract string VerboseFullyQualifiedName { get; }

        public abstract string FullNamespace { get; }

        public abstract bool IsGeneric { get; }

        public bool IsCollection => IsA<IEnumerable>() && !IsArray && !IsString;

        public abstract bool IsArray { get; }

        public abstract bool IsNullable { get; }

        public bool IsNullableType => IsA(typeof(Nullable<>));

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

        /// <summary>
        /// Returns a human-readable string that represents the name of this type to the client.
        /// </summary>
        public string DisplayName => ClientTypeName.ToProperCase();

        /// <summary>
        /// Get a value indicating what kind of type this <see cref="TypeViewModel"/> will be represented by on the client.
        /// </summary>
        public TypeDiscriminator TsTypeKind =>
            IsString ? TypeDiscriminator.String :
            IsGuid ? TypeDiscriminator.String :
            IsByteArray ? TypeDiscriminator.String :
            IsNumber ? TypeDiscriminator.Number :
            IsBool ? TypeDiscriminator.Boolean :
            IsDate ? TypeDiscriminator.Date :
            IsEnum ? TypeDiscriminator.Enum :
            IsVoid ? TypeDiscriminator.Void :
            IsCollection ? TypeDiscriminator.Collection :
            HasClassViewModel ? (
                ClassViewModel.IsDbMappedType ? TypeDiscriminator.Model : TypeDiscriminator.Object
            ) : TypeDiscriminator.Unknown;

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
                if (IsGuid) return "Guid.Parse";
                return "(object)";
            }
        }

        /// <summary>
        /// True if the type is supported by Coalesce as a key type.
        /// </summary>
        public bool IsValidKeyType => IsString || IsIntegral || IsGuid;

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
                if (IsGuid) return "string";
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
        private string TsTypePlain
        {
            get
            {
                if (IsString) return "string";
                if (IsBool) return "boolean";
                if (IsDate) return "moment.Moment";
                if (IsEnum) return "number";
                if (IsNumber) return "number";
                if (IsVoid) return "void";
                if (IsPOCO) return $"ViewModels.{PureType.Name}";
                if (IsClass) return PureType.Name;
                return "any";
            }
        }

        public virtual bool IsInternalUse => HasAttribute<InternalUseAttribute>();

        public bool HasClassViewModel => !IsPrimitive && IsPOCO;

        public abstract ClassViewModel ClassViewModel { get; }


        /// <summary>
        /// True if this is a boolean.
        /// </summary>
        public bool IsBool => NullableUnderlyingType.IsA<bool>();

        public bool IsPrimitive => IsString || IsNumber || IsBool || IsEnum;

        /// <summary>
        /// True if this is a DateTime or DateTimeOffset.
        /// </summary>
        public bool IsDate => IsDateTime || IsDateTimeOffset;

        /// <summary>
        /// True if the property is a string.
        /// </summary>
        public bool IsString => IsA<string>();

        public bool IsGuid => NullableUnderlyingType.IsA<Guid>();

        /// <summary>
        /// True if the property is a DateTime or Nullable DateTime
        /// </summary>
        public bool IsDateTime => NullableUnderlyingType.IsA<DateTime>();

        /// <summary>
        /// True if the property is a DateTimeOffset or Nullable DateTimeOffset
        /// </summary>
        public bool IsDateTimeOffset => NullableUnderlyingType.IsA<DateTimeOffset>();

        /// <summary>
        /// Returns true if class is a Byte[]
        /// </summary>
        public bool IsByteArray => IsArray && PureType.IsA<Byte>();

        /// <summary>
        /// Returns true if the type is any integral type or a nullable version of such a type, except <see cref="char"/>
        /// </summary>
        public bool IsIntegral
        {
            get
            {
                var underlying = NullableUnderlyingType;
                return underlying.IsA<SByte>()
                    || underlying.IsA<Byte>()
                    || underlying.IsA<Int16>()
                    || underlying.IsA<UInt16>()
                    || underlying.IsA<Int32>()
                    || underlying.IsA<UInt32>()
                    || underlying.IsA<Int64>()
                    || underlying.IsA<UInt64>();
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

                var underlying = NullableUnderlyingType;
                return underlying.IsA<Single>()
                    || underlying.IsA<Double>()
                    || underlying.IsA<Decimal>();
            }
        }

        /// <summary>
        /// If this represents a nullable type, returns the underlying type that is nullable.
        /// Otherwise, returns the current instance.
        /// </summary>
        public TypeViewModel NullableUnderlyingType => IsNullableType ? FirstTypeArgument : this;

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

                if (IsGeneric && (IsCollection || IsNullable))
                {
                    return FirstTypeArgument;
                }

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

        public string DtoFullyQualifiedName => IsCollection
            // We assume ICollection for all collections. If this doesn't work in a particular context,
            // consider that whatever you're assigning to this type should probably be assignable to ICollection if it is indeed a collection.
            ? $"ICollection<{PureType.DtoFullyQualifiedName}>" 
            : (HasClassViewModel ? ClassViewModel.DtoName : FullyQualifiedName);

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

            return this.VerboseFullyQualifiedName == that.VerboseFullyQualifiedName;
        }

        public override int GetHashCode() => VerboseFullyQualifiedName.GetHashCode();

        // TODO: maybe retire this in favor of plain .Equals? Make sure that ReflectionTypeViewModel.FullyQualifiedName is correct, first.
        public abstract bool EqualsType(TypeViewModel b);

        public override string ToString() => FullyQualifiedName;
    }
}
