using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using IntelliTect.Coalesce.Utilities;
using Microsoft.CodeAnalysis;

namespace IntelliTect.Coalesce.TypeDefinition;


#pragma warning disable CS0659 // Intentionally using base class's GetHashCode

public class ReflectionTypeViewModel : TypeViewModel
{
    protected internal Type Info { get; internal set; }

    public ReflectionTypeViewModel(Type type) : this(null, type)
    {
    }

    internal ReflectionTypeViewModel(ReflectionRepository? reflectionRepository, Type type) : base(reflectionRepository)
    {
        ReflectionRepository = reflectionRepository;
        Info = type;

        FirstTypeArgument = IsGeneric && Info.IsConstructedGenericType
            ? ReflectionTypeViewModel.GetOrCreate(reflectionRepository, Info.GenericTypeArguments[0])
            : null;

        ArrayType = IsArray 
            ? ReflectionTypeViewModel.GetOrCreate(reflectionRepository, Info.GetElementType()!) 
            : null;

        IEnumerable<Type> GetBaseClassesAndInterfaces(Type t)
        {
            return (t.BaseType == null || t.BaseType == typeof(object) ? [] : GetBaseClassesAndInterfaces(t.BaseType))
                .Append(t)
                .Concat(t.GetInterfaces());
        }

        BaseClassesAndInterfaces = GetBaseClassesAndInterfaces(Info).Distinct().Reverse().ToList();

        ClassViewModel = ShouldCreateClassViewModel
            ? new ReflectionClassViewModel(this)
            : null;

        // This is precomputed because it is used for .Equals() and the == operator.
        FullyQualifiedName = GetTypeName(Info);
        VerboseFullyQualifiedName = GetVerboseTypeName(Info);
    }

    internal static ReflectionTypeViewModel GetOrCreate(ReflectionRepository? reflectionRepository, Type type)
    {
        return reflectionRepository?.GetOrAddType(type) ?? new ReflectionTypeViewModel(reflectionRepository, type);
    }

    public override TypeViewModel? BaseType => Info.BaseType is null ? null : GetOrCreate(ReflectionRepository, Info.BaseType);

    public override IAttributeProvider Assembly
        => new ReflectionAttributeProvider(Info.Assembly);

    // TODO: why is an arity of 1 removed from the name? Seems to be an oversight
    // - If we're removing arity, we should remove any arity.
    public override string Name => Info.Name.Replace("`1", "");

    public override IEnumerable<AttributeViewModel<TAttribute>> GetAttributes<TAttribute>()
        => Info.GetAttributes<TAttribute>();

    private ICollection<Type> BaseClassesAndInterfaces { get; }

    private Type? GetSatisfyingBaseType(Type type)
    {
        return BaseClassesAndInterfaces
            .FirstOrDefault(x =>
                   x.Equals(type)
                || x.IsSubclassOf(type)
                || type.IsAssignableFrom(x)
                || (x.IsGenericType && x.GetGenericTypeDefinition() == type)
            );
    }

    public override TypeViewModel[]? GenericArgumentsFor(Type type) =>
        GetSatisfyingBaseType(type)?
            .GenericTypeArguments
            .Select(t => ReflectionTypeViewModel.GetOrCreate(ReflectionRepository, t))
            .ToArray();

    public override bool IsA(Type type) => GetSatisfyingBaseType(type) != null;

    public override bool IsGeneric => Info.IsGenericType;

    public override bool IsAbstract => Info.IsAbstract;

    public override bool IsArray => Info.IsArray;

    public override bool IsReferenceOrNullableValue => Info.IsClass || IsNullableValueType;

    public override bool IsReferenceType => !Info.IsValueType;

    public override bool IsClass => Info.IsClass;

    public override bool IsInterface => Info.IsInterface;

    public override bool IsInternalUse => !Info.IsVisible || base.IsInternalUse;

    public override bool IsVoid => Info == typeof(void);

    private IReadOnlyList<EnumMember>? _enumValues;
    public override IReadOnlyList<EnumMember> EnumValues
    {
        get
        {
            if (_enumValues != null) return _enumValues;

            if (IsNullableValueType) return NullableValueUnderlyingType.EnumValues;

            var result = new List<EnumMember>();
            if (!IsEnum) return result;

            var integralType = Enum.GetUnderlyingType(Info);

            foreach (var value in Enum.GetValues(Info))
            {
                var name = value!.ToString()!;
                var member = Info.GetMember(name)[0];

                result.Add(new EnumMember(
                    name, 
                    Convert.ChangeType(value, integralType),
                    member.GetAttribute<DisplayAttribute>()?.Name ??
                        member.Name.ToProperCase(),
                    member.GetAttribute<DisplayAttribute>()?.Description ??
                        member.GetAttribute<DescriptionAttribute>()?.Description
                ));
            }
            return _enumValues = result;
        }
    }

    public override bool IsEnum =>
        IsNullableValueType ? NullableValueUnderlyingType.IsEnum : Info.IsEnum;

    public override string FullNamespace => Info.Namespace ?? "";

    private static string GetTypeName(Type type)
    {
        if (type.IsGenericParameter)
        {
            return type.Name;
        }

        if (type.IsArray)
        {
            return GetTypeName(type.GetElementType()!) + "[" + new string(',', type.GetArrayRank() - 1) + "]";
        }

        if (Nullable.GetUnderlyingType(type) is Type nullableUnderlying && nullableUnderlying != null)
        {
            return GetTypeName(nullableUnderlying) + "?";
        }

        if (type.IsEnum)
        {
            return type.FullName?.Replace('+', '.') ?? "";
        }

        if (!type.IsGenericType)
        {
            if (type == typeof(void)) return "void";
            return Type.GetTypeCode(type) switch
            {
                TypeCode.Boolean => "bool",
                TypeCode.Int16 => "short",
                TypeCode.Int32 => "int",
                TypeCode.Int64 => "long",
                TypeCode.UInt16 => "ushort",
                TypeCode.UInt32 => "uint",
                TypeCode.UInt64 => "ulong",
                TypeCode.Single => "float",
                TypeCode.Double => "double",
                TypeCode.Decimal => "decimal",
                TypeCode.Byte => "byte",
                TypeCode.SByte => "sbyte",
                TypeCode.String => "string",
                _ => type.FullName?.Replace('+', '.') ?? ""
            };
        }

        var builder = new System.Text.StringBuilder();
        var name = type.Name;
        var index = name.IndexOf("`");
        builder.AppendFormat("{0}.{1}", type.Namespace, name.Substring(0, index));
        builder.Append('<');
        var first = true;
        foreach (var arg in type.GetGenericArguments())
        {
            if (!first)
            {
                builder.Append(", ");
            }
            builder.Append(GetTypeName(arg));
            first = false;
        }
        builder.Append('>');
        return builder.ToString();
    }

    private static string GetVerboseTypeName(Type type)
    {
        // From https://stackoverflow.com/questions/401681

        if (type.IsGenericParameter)
        {
            return type.Name;
        }

        if (type.IsArray)
        {
            return GetVerboseTypeName(type.GetElementType()!) + "[" + new string(',', type.GetArrayRank() - 1) + "]";
        }

        if (!type.IsGenericType)
        {
            return type.FullName!.Replace('+', '.') ?? "";
        }

        var builder = new System.Text.StringBuilder();
        var name = type.Name;
        var index = name.IndexOf("`");
        builder.AppendFormat("{0}.{1}", type.Namespace, name.Substring(0, index));
        builder.Append('<');
        var first = true;
        foreach (var arg in type.GetGenericArguments())
        {
            if (!first)
            {
                builder.Append(", ");
            }
            builder.Append(GetVerboseTypeName(arg));
            first = false;
        }
        builder.Append('>');
        return builder.ToString();
    }

    public override string FullyQualifiedName { get; }

    public override string VerboseFullyQualifiedName { get; }

    public override TypeViewModel? FirstTypeArgument { get; }

    public override TypeViewModel? ArrayType { get; }

    public override ClassViewModel? ClassViewModel { get; }

    public override Type TypeInfo => Info;

    public override bool Equals(object? obj)
    {
        if (!(obj is ReflectionTypeViewModel that)) return base.Equals(obj);

        return Info == that.Info;
    }
}
