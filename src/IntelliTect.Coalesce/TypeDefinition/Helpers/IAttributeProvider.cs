using IntelliTect.Coalesce.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public interface IAttributeProvider
    {
        object? GetAttributeValue<TAttribute>(string valueName) where TAttribute : Attribute;

        bool HasAttribute<TAttribute>() where TAttribute : Attribute;
    }

    public static class AttributeExtensions
    {
        public static T? GetAttributeValue<TAttribute, T>(this IAttributeProvider obj, string valueName)
            where TAttribute : Attribute
            where T : struct
        {
            var result = obj.GetAttributeValue<TAttribute>(valueName);
            if (result == null)
            {
                return null;
            }
            return new T?((T)result);
        }

        public static T? GetAttributeValue<TAttribute, T>(this IAttributeProvider obj, Expression<Func<TAttribute, T?>> propertyExpression)
            where TAttribute : Attribute
            where T : struct
        {
            return obj.GetAttributeValue<TAttribute, T>(propertyExpression.GetExpressedProperty().Name);
        }

        public static T? GetAttributeValue<TAttribute, T>(this IAttributeProvider obj, Expression<Func<TAttribute, T>> propertyExpression)
            where TAttribute : Attribute
            where T : struct
        {
            return obj.GetAttributeValue<TAttribute, T>(propertyExpression.GetExpressedProperty().Name);
        }

        public static string? GetAttributeValue<TAttribute>(this IAttributeProvider obj, Expression<Func<TAttribute, string?>> propertyExpression)
            where TAttribute : Attribute
        {
            return obj.GetAttributeValue<TAttribute>(propertyExpression.GetExpressedProperty().Name) as string;
        }

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

        public static TypeViewModel? GetAttributeValue<TAttribute>(this IAttributeProvider obj, Expression<Func<TAttribute, Type>> propertyExpression)
            where TAttribute : Attribute
        {
            var value = obj.GetAttributeValue<TAttribute>(propertyExpression.GetExpressedProperty().Name);
            if (value is Type reflectionValue) return ReflectionRepository.Global.GetOrAddType(reflectionValue);
            if (value is ITypeSymbol symbolValue) return ReflectionRepository.Global.GetOrAddType(symbolValue);
            return null;
        }
    }
}
