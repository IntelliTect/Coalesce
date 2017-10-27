using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Helpers;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Linq.Expressions;

namespace IntelliTect.Coalesce.TypeDefinition.Wrappers
{
    internal abstract class Wrapper
    {
        public abstract string Name { get; }

        public abstract object GetAttributeValue<TAttribute>(string valueName)
            where TAttribute : Attribute;

        public abstract bool HasAttribute<TAttribute>()
            where TAttribute : Attribute;

        public T? GetAttributeValue<TAttribute, T>(string valueName)
            where TAttribute : Attribute
            where T : struct
        {
            var result = GetAttributeValue<TAttribute>(valueName);
            if (result == null)
            {
                return null;
            }
            return new Nullable<T>((T)result);
        }
        
        public T? GetAttributeValue<TAttribute, T>(Expression<Func<TAttribute, T>> propertyExpression)
            where TAttribute : Attribute
            where T : struct
        {
            return GetAttributeValue<TAttribute, T>(propertyExpression.GetExpressedProperty().Name);
        }

        
        public T GetAttributeObject<TAttribute, T>(string valueName) where TAttribute : Attribute where T : class
        {
            return GetAttributeValue<TAttribute>(valueName) as T;
        }

        public T GetAttributeObject<TAttribute, T>(Expression<Func<TAttribute, T>> propertyExpression) where TAttribute : Attribute where T : class
        {
            return GetAttributeObject<TAttribute, T>(propertyExpression.GetExpressedProperty().Name);
        }

        public virtual AttributeWrapper GetSecurityAttribute<TAttribute>() where TAttribute : SecurityAttribute
        {
            throw new NotImplementedException();
        }
    }
}
