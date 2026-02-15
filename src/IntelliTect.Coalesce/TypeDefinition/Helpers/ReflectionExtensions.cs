using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace IntelliTect.Coalesce.TypeDefinition;

public class ReflectionAttributeViewModel<TAttribute> : AttributeViewModel<TAttribute>
    where TAttribute : Attribute
{
    public TAttribute Instance { get; }

    public ReflectionAttributeViewModel(TAttribute instance, ReflectionRepository? rr) : base(rr)
    {
        Instance = instance;
    }

    public override TypeViewModel Type => ReflectionRepository.GetOrAddType(Instance.GetType());

    public override object? GetValue(string valueName)
    {
        var property = Instance.GetType().GetProperty(valueName);
        if (property == null) return null;

        // Some attributes have getters that throw if the value was never set, hence the try/catch.
        // E.g. DisplayAttribute.Order
        try
        {
            var value = property.GetValue(Instance, null);
            if (value is Type reflectionValue) return ReflectionRepository.Global.GetOrAddType(reflectionValue);
            return value;
        }
        catch (Exception)
        {
            return null;
        }
    }
}

internal class ReflectionAttributeProvider(ICustomAttributeProvider symbol) : IAttributeProvider
{
    public IEnumerable<AttributeViewModel<TAttribute>> GetAttributes<TAttribute>()
        where TAttribute : Attribute
        => symbol.GetAttributes<TAttribute>();
}

public static class ReflectionExtensions
{
    public static IEnumerable<ReflectionAttributeViewModel<TAttribute>> GetAttributes<TAttribute>(
        this ICustomAttributeProvider member,
        ReflectionRepository? rr = null
    )
        where TAttribute : Attribute
        => member.GetCustomAttributes(typeof(TAttribute), true)
            .OfType<TAttribute>()
            .Select(a => new ReflectionAttributeViewModel<TAttribute>(a, rr));

    public static TAttribute? GetAttribute<TAttribute>(
        this ICustomAttributeProvider member
    )
        where TAttribute : Attribute
        => member.GetAttributes<TAttribute>().Select(a => a.Instance).FirstOrDefault();
}
