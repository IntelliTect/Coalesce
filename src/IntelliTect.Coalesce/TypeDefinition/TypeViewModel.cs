using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace IntelliTect.Coalesce.TypeDefinition
{

    public abstract class TypeViewModel : IAttributeProvider
    {
        public TypeViewModel()
        {

        }

        internal TypeViewModel(ReflectionRepository? reflectionRepository) : this()
        {
            ReflectionRepository = reflectionRepository;
        }

        public ReflectionRepository? ReflectionRepository { get; internal set; }

        public abstract string Name { get; }

        /// <summary>
        /// Returns the name of the type to be used by the client.
        /// </summary>
        public string ClientTypeName => IsNullableValueType
            ? NullableValueUnderlyingType.ClientTypeName
            : this.GetAttributeValue<CoalesceAttribute>(a => a.ClientTypeName) ?? Name;

        public abstract string FullyQualifiedName { get; }

        public abstract string VerboseFullyQualifiedName { get; }

        public abstract string FullNamespace { get; }

        public abstract bool IsGeneric { get; }

        public abstract bool IsAbstract { get; }

        /// <summary>
        /// True if the type is <see cref="IEnumerable"/> and behaves like a collection.
        /// Does not necessarily mean the type is an <see cref="ICollection"/> or <see cref="ICollection{T}"/>.
        /// <para>
        /// Excludes <see cref="string"/> and <see cref="T:byte[]"/>
        /// </para>
        /// </summary>
        public bool IsCollection => IsA(typeof(IEnumerable<>)) && !IsString && !IsByteArray;

        public bool IsDictionary => IsA(typeof(IDictionary<,>));

        public abstract bool IsArray { get; }

        /// <summary>
        /// Returns true if the type can be assigned `null`. Does NOT factor in C# 8 non-nullable reference types.
        /// </summary>
        public abstract bool IsReferenceOrNullableValue { get; }

        /// <summary>
        /// Returns true if the type is a reference type (i.e. not a value type).
        /// </summary>
        public abstract bool IsReferenceType { get; }

        public bool IsNullableValueType => !IsReferenceType && IsA(typeof(Nullable<>));

        public abstract bool IsClass { get; }

        public abstract bool IsInterface { get; }

        /// <summary>
        /// Returns true if this TypeViewModel represents void.
        /// </summary>
        public abstract bool IsVoid { get; }

        public abstract IReadOnlyList<EnumMember> EnumValues { get; }

        public abstract bool IsEnum { get; }

        public abstract TypeViewModel? FirstTypeArgument { get; }

        public abstract TypeViewModel? ArrayType { get; }

        public abstract bool IsA(Type type);

        public abstract TypeViewModel[]? GenericArgumentsFor(Type type);

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
            IsByteArray ? TypeDiscriminator.Binary :
            IsNumber ? TypeDiscriminator.Number :
            IsBool ? TypeDiscriminator.Boolean :
            IsDateOrTime ? TypeDiscriminator.Date :
            IsEnum ? TypeDiscriminator.Enum :
            IsVoid ? TypeDiscriminator.Void :
            IsFile ? TypeDiscriminator.File :
            IsCollection ? TypeDiscriminator.Collection :
            ClassViewModel != null 
                ? (ClassViewModel.IsDbMappedType ? TypeDiscriminator.Model : TypeDiscriminator.Object) 
                : TypeDiscriminator.Unknown;

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

        [Obsolete("Deprecated")]
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
        public bool IsValidKeyType => IsString || IsIntegral || IsEnum || IsGuid;

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
                switch (TsTypeKind)
                {
                    case TypeDiscriminator.String: return "string";
                    case TypeDiscriminator.Boolean: return "boolean";
                    case TypeDiscriminator.Date: return "moment.Moment";
                    case TypeDiscriminator.Enum: return "number";
                    case TypeDiscriminator.Number: return "number";
                    case TypeDiscriminator.Void: return "void";
                    case TypeDiscriminator.File: return "File";
                    case TypeDiscriminator.Unknown: return "any";
                }

                if (IsPOCO) return $"ViewModels.{PureType.Name}";
                if (IsClass) return PureType.Name;
                return "any";
            }
        }

        public virtual bool IsInternalUse => HasAttribute<InternalUseAttribute>() || (PureType != this && PureType.IsInternalUse);

        protected virtual bool ShouldCreateClassViewModel => !IsPrimitive && IsPOCO;

        public bool HasClassViewModel => ClassViewModel != null;

        public abstract ClassViewModel? ClassViewModel { get; }


        /// <summary>
        /// True if this is a boolean.
        /// </summary>
        public bool IsBool => NullableValueUnderlyingType.IsA<bool>();

        public bool IsPrimitive => IsString || IsNumber || IsBool || IsEnum;

        /// <summary>
        /// True if this is a DateTime or DateTimeOffset.
        /// </summary>
        public bool IsDate => IsDateTime || IsDateTimeOffset || NullableValueUnderlyingType.IsA<DateOnly>();

        /// <summary>
        /// True if this type holds temporal data.
        /// </summary>
        public bool IsDateOrTime => IsDate || NullableValueUnderlyingType.IsA<TimeOnly>();

        /// <summary>
        /// True if the property is a string.
        /// </summary>
        public bool IsString => IsA<string>();

        public bool IsGuid => NullableValueUnderlyingType.IsA<Guid>();

        /// <summary>
        /// True if the property is a DateTime or Nullable DateTime
        /// </summary>
        public bool IsDateTime => NullableValueUnderlyingType.IsA<DateTime>();

        /// <summary>
        /// True if the property is a DateTimeOffset or Nullable DateTimeOffset
        /// </summary>
        public bool IsDateTimeOffset => NullableValueUnderlyingType.IsA<DateTimeOffset>();

        public DateTypeAttribute.DateTypes? DateType =>
            NullableValueUnderlyingType.IsA<DateOnly>() ? DateTypeAttribute.DateTypes.DateOnly :
            NullableValueUnderlyingType.IsA<TimeOnly>() ? DateTypeAttribute.DateTypes.TimeOnly :
            IsDate ? DateTypeAttribute.DateTypes.DateTime :
            null;

        /// <summary>
        /// Returns true if class is a Byte[]
        /// </summary>
        public bool IsByteArray => IsArray && ArrayType!.IsA<Byte>();

        /// <summary>
        /// Returns true if the type is an <see cref="IntelliTect.Coalesce.Models.IFile"/>
        /// </summary>
        public bool IsFile => IsA(typeof(IntelliTect.Coalesce.Models.IFile));

        /// <summary>
        /// Returns true if the type is any integral type or a nullable version of such a type, except <see cref="char"/>
        /// </summary>
        public bool IsIntegral
        {
            get
            {
                var underlying = NullableValueUnderlyingType;
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

                var underlying = NullableValueUnderlyingType;
                return underlying.IsA<Single>()
                    || underlying.IsA<Double>()
                    || underlying.IsA<Decimal>();
            }
        }

        /// <summary>
        /// If this represents a nullable type, returns the underlying type that is nullable.
        /// Otherwise, returns the current instance.
        /// </summary>
        public TypeViewModel NullableValueUnderlyingType => IsNullableValueType ? FirstTypeArgument! : this;

        /// <summary>
        /// Gets the type name without any collection or nullable around it.
        /// </summary>
        public TypeViewModel PureType
        {
            get
            {
                if (IsArray)
                {
                    return ArrayType!;
                }

                if (IsGeneric)
                {
                    if (IsCollection)
                    {
                        return (
                            GenericArgumentsFor(typeof(ICollection<>))
                            ?? GenericArgumentsFor(typeof(IEnumerable<>))
                        )?[0]
                        ?? FirstTypeArgument!;
                    }
                    if (IsReferenceOrNullableValue)
                    {
                        return FirstTypeArgument!;
                    }
                }

                return this;
            }
        }

        /// <summary>
        /// Returns true if the type is a reference type outside the System namespace.
        /// </summary>
        public bool IsPOCO => 
            IsReferenceType && 
            !IsArray && 
            !IsCollection && 
            !FullNamespace.StartsWith("System") && 
            !IsFile;

        public string TsDeclaration => $"{Name}: {TsType}";

        public string TsDeclarationPlain(string parameterName) => $"{parameterName}: {TsTypePlain}";
        
        public abstract object? GetAttributeValue<TAttribute>(string valueName) where TAttribute : Attribute;

        public abstract bool HasAttribute<TAttribute>() where TAttribute : Attribute;

        public string DtoFullyQualifiedName => NullableTypeForDto(null, true);

        public string NullableTypeForDto(string? dtoNamespace, bool dontEmitNullable = false)
        {
            if (IsDictionary)
            {
                var args = GenericArgumentsFor(typeof(IDictionary<,>))!;
                return $"System.Collections.Generic.IDictionary<{args[0].NullableTypeForDto(dtoNamespace, true)}, {args[1].NullableTypeForDto(dtoNamespace, true)}>";
            }

            if (IsCollection)
            {
                var innerType = PureType.NullableTypeForDto(dtoNamespace, true);
                if (IsArray)
                {
                    return $"{innerType}[]";
                }

                if (IsA(typeof(System.Collections.Generic.ICollection<>)))
                {
                    return $"System.Collections.Generic.ICollection<{innerType}>";
                }

                if (IsA(typeof(System.Collections.Generic.IReadOnlyCollection<>)))
                {
                    return $"System.Collections.Generic.IReadOnlyCollection<{innerType}>";
                }

                // ¯\_(ツ)_/¯
                return $"System.Collections.Generic.ICollection<{innerType}>";
            }

            var model = this.PureType.ClassViewModel;
            if (model != null)
            {
                if (model.IsDto) return FullyQualifiedName;

                string typeName;

                if (IsReferenceOrNullableValue || IsArray || IsCollection)
                    typeName = FullyQualifiedName;
                else
                    typeName = Name + "?";

                var regex = new Regex($@"({model.Name}(?!DtoGen))(>|$)");
                typeName = regex.Replace(typeName, $@"{model.Name}DtoGen$2");
                typeName = typeName.Replace(model.Type.FullNamespace + ".", string.IsNullOrWhiteSpace(dtoNamespace) ? "" : dtoNamespace + ".");

                return typeName;
            }
            else
            {
                if (dontEmitNullable || IsReferenceOrNullableValue || IsArray)
                    return FullyQualifiedName;
                else
                    return FullyQualifiedName + "?";
            }
        }

        public override bool Equals(object? obj)
        {
            if (!(obj is TypeViewModel that)) return false;

            return this.VerboseFullyQualifiedName == that.VerboseFullyQualifiedName;
        }

        public override int GetHashCode() => VerboseFullyQualifiedName.GetHashCode();

        public override string ToString() => FullyQualifiedName;

        public static bool operator ==(TypeViewModel? lhs, TypeViewModel? rhs)
        {
            if (Object.ReferenceEquals(lhs, null))
            {
                return Object.ReferenceEquals(rhs, null);
            }

            return lhs.Equals(rhs);
        }

        public static bool operator !=(TypeViewModel? o1, TypeViewModel? o2)
        {
            return !(o1 == o2);
        }
    }
}
