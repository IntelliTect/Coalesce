using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Utilities
{
    public static class ExpressionExtensions
    {
        public static PropertyInfo GetExpressedProperty<T, TProp>(this Expression<Func<T, TProp>> propertyLambda)
        {
            return propertyLambda.GetExpressedProperty(typeof(T));
        }

        public static PropertyInfo GetExpressedProperty(this LambdaExpression propertyLambda, Type paramType)
        {
            Type type = paramType;
            MemberExpression member;

            // Check to see if the node type is a Convert type (this is the case with enums)
            if (propertyLambda.Body.NodeType == ExpressionType.Convert)
            {
                member = ((UnaryExpression)propertyLambda.Body).Operand as MemberExpression;
            }
            else
            {
                member = propertyLambda.Body as MemberExpression;
            }
            if (member == null)
            {
                // Handle the case of a nullable.
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda.ToString()));
            }

            PropertyInfo propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda.ToString()));

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a property that is not from type {1}.",
                    propertyLambda.ToString(),
                    type));

            return propInfo;
        }
    }
}
