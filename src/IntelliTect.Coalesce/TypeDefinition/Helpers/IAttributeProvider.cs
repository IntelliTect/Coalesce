using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public interface IAttributeProvider
    {
        object GetAttributeValue<TAttribute>(string valueName) where TAttribute : Attribute;

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
            return new Nullable<T>((T)result);
        }

        public static T? GetAttributeValue<TAttribute, T>(this IAttributeProvider obj, Expression<Func<TAttribute, T>> propertyExpression)
            where TAttribute : Attribute
            where T : struct
        {
            return obj.GetAttributeValue<TAttribute, T>(propertyExpression.GetExpressedProperty().Name);
        }

        public static string GetAttributeValue<TAttribute>(this IAttributeProvider obj, Expression<Func<TAttribute, string>> propertyExpression)
            where TAttribute : Attribute
        {
            return obj.GetAttributeValue<TAttribute>(propertyExpression.GetExpressedProperty().Name) as string;
        }
    }
}
