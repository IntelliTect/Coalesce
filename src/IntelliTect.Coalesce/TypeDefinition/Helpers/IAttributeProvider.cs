using IntelliTect.Coalesce.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;

namespace IntelliTect.Coalesce.TypeDefinition;

public interface IAttributeProvider
{
    IEnumerable<AttributeViewModel<TAttribute>> GetAttributes<TAttribute>() where TAttribute : Attribute;
}

public static class AttributeExtensions
{
    public static IEnumerable<AttributeViewModel<TAttribute>> GetAttributes<TAttribute>(this IAttributeProvider obj)
        where TAttribute : Attribute
        => obj.GetAttributes<TAttribute>();

    public static AttributeViewModel<TAttribute>? GetAttribute<TAttribute>(this IAttributeProvider obj)
        where TAttribute : Attribute
        => obj.GetAttributes<TAttribute>().FirstOrDefault();

    public static bool HasAttribute<TAttribute>(this IAttributeProvider obj)
        where TAttribute : Attribute
    {
        return obj.GetAttributes<TAttribute>().Any();
    }

    public static object? GetAttributeValue<TAttribute>(this IAttributeProvider obj, string valueName)
        where TAttribute : Attribute
        => obj.GetAttribute<TAttribute>()?.GetValue(valueName);

    public static T? GetAttributeValue<TAttribute, T>(this IAttributeProvider obj, string valueName)
        where TAttribute : Attribute
        where T : struct
        => obj.GetAttribute<TAttribute>()?.GetValue<T>(valueName);

    public static T? GetAttributeValue<TAttribute, T>(
        this IAttributeProvider obj,
        Expression<Func<TAttribute, T>> propertyExpression,
        Expression<Func<TAttribute, bool>>? hasValueExpression = null
    )
        where TAttribute : Attribute
        where T : struct
        => obj.GetAttribute<TAttribute>()?.GetValue<T>(propertyExpression, hasValueExpression);

    public static string? GetAttributeValue<TAttribute>(this IAttributeProvider obj, Expression<Func<TAttribute, string?>> propertyExpression)
        where TAttribute : Attribute
        => obj.GetAttribute<TAttribute>()?.GetValue(propertyExpression);

    public static (bool Exists, string? Message) GetValidationAttribute<TAttribute>(this IAttributeProvider obj)
        where TAttribute : ValidationAttribute
    {
        if (!obj.HasAttribute<TAttribute>())
        {
            return (false, null);
        }

        return (true, obj.GetAttributeValue<TAttribute>(a => a.ErrorMessage) ?? "");
    }

    public static (bool Exists, T? Value, string? Message) GetValidationAttribute<TAttribute, T>(
        this IAttributeProvider obj,
        Expression<Func<TAttribute, T>> propertyExpression
    )
        where TAttribute : ValidationAttribute
        where T : struct
    {
        if (obj.GetValidationAttribute<TAttribute>() is not (true, string message))
        {
            return (false, null, null);
        }

        return (true, obj.GetAttributeValue(propertyExpression), message);
    }

    public static TypeViewModel? GetAttributeValue<TAttribute>(this IAttributeProvider obj, Expression<Func<TAttribute, Type?>> propertyExpression)
        where TAttribute : Attribute
    {
        var value = obj.GetAttributeValue<TAttribute>(propertyExpression.GetExpressedProperty().Name);
        return value as TypeViewModel;
    }
}
