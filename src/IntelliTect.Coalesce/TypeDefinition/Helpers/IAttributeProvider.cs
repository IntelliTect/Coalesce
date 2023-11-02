using IntelliTect.Coalesce.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public abstract class AttributeViewModel<TAttribute>
        where TAttribute : Attribute
    {
        public T? GetValue<T>(string valueName)
            where T : struct
        {
            var result = GetValue(valueName);
            if (result == null)
            {
                return null;
            }
            return new T?((T)result);
        }

        public string? GetValue(Expression<Func<TAttribute, string?>> propertyExpression)
        {
            return GetValue(propertyExpression.GetExpressedProperty().Name) as string;
        }

        public T? GetValue<T>(
            Expression<Func<TAttribute, T>> propertyExpression,
            Expression<Func<TAttribute, bool>>? hasValueExpression = null
        )
            where T : struct
        {
            // In reflection contexts, for attributes that can have "unset" as a value that means null,
            // we have to check this state with an additional property. E.g. ExecuteAttribute.ValidateAttributes.
            // In symbol contexts, the absence of syntax that assigns a value to the property implies this "unset" state.
            if (hasValueExpression is not null)
            {
                // `hasValue` will always be non-null in reflection contexts,
                // and always null in symbol contexts where the HasValue property won't ever be set.
                var hasValue = GetValue<bool>(hasValueExpression.GetExpressedProperty().Name);
                if (hasValue == false)
                {
                    return null;
                }
            }


            return GetValue<T>(propertyExpression.GetExpressedProperty().Name);
        }

        public abstract object? GetValue(string valueName);
    }

    public interface IAttributeProvider
    {
        IEnumerable<AttributeViewModel<TAttribute>> GetAttributes<TAttribute>() where TAttribute : Attribute;
    }

    public static class AttributeExtensions
    {
        public static IEnumerable<AttributeViewModel<TAttribute>> GetAttributes<TAttribute>(this IAttributeProvider obj)
            where TAttribute : Attribute
            => obj.GetAttributes<TAttribute>();

        public static bool HasAttribute<TAttribute>(this IAttributeProvider obj)
            where TAttribute : Attribute
        {
            return obj.GetAttributes<TAttribute>().Any();
        }

        public static object? GetAttributeValue<TAttribute>(this IAttributeProvider obj, string valueName)
            where TAttribute : Attribute
            => obj.GetAttributes<TAttribute>().FirstOrDefault()?.GetValue(valueName);

        public static T? GetAttributeValue<TAttribute, T>(this IAttributeProvider obj, string valueName)
            where TAttribute : Attribute
            where T : struct
            => obj.GetAttributes<TAttribute>().FirstOrDefault()?.GetValue<T>(valueName);

        public static T? GetAttributeValue<TAttribute, T>(
            this IAttributeProvider obj,
            Expression<Func<TAttribute, T>> propertyExpression,
            Expression<Func<TAttribute, bool>>? hasValueExpression = null
        )
            where TAttribute : Attribute
            where T : struct
            => obj.GetAttributes<TAttribute>().FirstOrDefault()?.GetValue<T>(propertyExpression, hasValueExpression);

        public static string? GetAttributeValue<TAttribute>(this IAttributeProvider obj, Expression<Func<TAttribute, string?>> propertyExpression)
            where TAttribute : Attribute
            => obj.GetAttributes<TAttribute>().FirstOrDefault()?.GetValue(propertyExpression);

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
