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

        public static MemberInfo GetExpressedMember(this LambdaExpression memberLambda)
        {
            MemberExpression member;

            // Check to see if the node type is a Convert type (this is the case with enums)
            if (memberLambda.Body.NodeType == ExpressionType.Convert)
            {
                member = ((UnaryExpression)memberLambda.Body).Operand as MemberExpression;
            }
            else
            {
                member = memberLambda.Body as MemberExpression;
            }
            if (member == null)
            {
                throw new ArgumentException($"Expression '{memberLambda}' isn't a member expression.");
            }

            return member.Member;
        }

        public static PropertyInfo GetExpressedProperty(this LambdaExpression lambda, Type paramType)
        {
            MemberInfo member = GetExpressedMember(lambda);
            PropertyInfo propInfo = member as PropertyInfo;

            if (propInfo == null)
                throw new ArgumentException($"Expression '{lambda}' doesn't refer to a property.");

            // If paramType is null, don't check this. Just a safety check where it makes sense, but sometimes, its ok not to check.
            if (   paramType != null 
                && paramType != propInfo.ReflectedType 
                && !paramType.IsSubclassOf(propInfo.ReflectedType))
                throw new ArgumentException($"Expression '{lambda}' refers to a property that is not from type {paramType}.");

            return propInfo;
        }

        public static MethodInfo GetExpressedMethod(this LambdaExpression lambda)
            => GetExpressedMember(lambda) as MethodInfo ?? throw new ArgumentException($"Expression '{lambda}' doesn't refer to a method.");
    }
}
