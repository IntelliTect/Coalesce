using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
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

        public static IEnumerable<PropertyInfo> GetExpressedProperties<T>(this Expression<Func<T, object?>> propertyLambda)
        {
            ParameterExpression parameterExpression = propertyLambda.Parameters.Single();
            if (propertyLambda.Body is NewExpression newExpression)
            {
                return newExpression.Arguments.Select((Expression x) =>
                {
                    if (x is not MemberExpression me)
                    {
                        throw new ArgumentException("Invalid expression " + propertyLambda);
                    }

                    return me.Member as PropertyInfo ?? throw new ArgumentException("Invalid member " + me.Member.ToString());
                });
            }

            return [GetExpressedProperty(propertyLambda)];
        }

        public static MemberInfo GetExpressedMember(this LambdaExpression memberLambda)
        {
            MemberExpression? member;

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

        public static PropertyInfo GetExpressedProperty(this LambdaExpression lambda, Type? paramType = null)
        {
            MemberInfo member = GetExpressedMember(lambda);
            PropertyInfo? propInfo = member as PropertyInfo;

            if (propInfo == null)
                throw new ArgumentException($"Expression '{lambda}' doesn't refer to a property.");

            // If paramType is null, don't check this. Just a safety check where it makes sense, but sometimes, its ok not to check.
            if (paramType != null
                && paramType != propInfo.ReflectedType
                && !paramType.IsSubclassOf(propInfo.ReflectedType!))
                throw new ArgumentException($"Expression '{lambda}' refers to a property that is not from type {paramType}.");

            return propInfo;
        }

        public static MethodInfo GetExpressedMethod(this LambdaExpression lambda)
            => GetExpressedMember(lambda) as MethodInfo ?? throw new ArgumentException($"Expression '{lambda}' doesn't refer to a method.");

        public static Expression Call(this Expression instanceTarget, MethodInfo method, params Expression[]? methodParams)
        {
            if (method.IsStatic)
            {
                // Call the static method as if it is an extension method,
                // passing the instanceTarget as the first param.
                return Expression.Call(
                    method,
                    [instanceTarget, .. methodParams]
                );
            }

            return Expression.Call(
                instanceTarget,
                method,
                methodParams
            );
        }

        public static Expression Call(this Expression instanceTarget, string method, params Expression[] methodParams)
        {
            var methodInfo = instanceTarget.Type.GetMethod(
                method,
                methodParams.Select(p => p.Type).ToArray()
            ) ?? throw new MissingMethodException($"Can't find method {method} on {instanceTarget.Type.FullName}");

            return instanceTarget.Call(methodInfo, methodParams);
        }

        public static Expression Prop(this Expression instance, string prop)
            => Expression.Property(instance, prop);

        public static Expression Prop(this Expression instance, PropertyViewModel prop)
            => Expression.Property(instance, prop.PropertyInfo);

        public static Expression OrAny(this IEnumerable<Expression> expressions)
            => expressions.Aggregate((prev, cur) => prev == null ? cur : Expression.OrElse(prev, cur));

        public static Expression AndAll(this IEnumerable<Expression> expressions)
            => expressions.Aggregate((prev, cur) => prev == null ? cur : Expression.AndAlso(prev, cur));

        /// <summary>
        /// Create an expression representing a constant value in a way that will
        /// cause EF's to parameterize the value and cache the query.
        /// <see href="https://github.com/dotnet/efcore/issues/8909#issuecomment-313768471" />
        /// </summary>
        public static Expression Parameterize(object? value, Type? type = null)
        {
            type ??= value?.GetType() ?? typeof(object);

            var box = Activator.CreateInstance(typeof(StrongBox<>).MakeGenericType(type));

            try
            {
                ((IStrongBox)box!).Value = value;
            }
            catch (InvalidCastException)
            {
                if (value is not IConvertible) throw;

                // If we have a known desired type, convert the value to that type.
                ((IStrongBox)box!).Value = Convert.ChangeType(value, type);
            }

            // This emulates what a variable capture from a closure looks like,
            // which EF will translate into a SQL query parameter.
            return Expression.Field(Expression.Constant(box), nameof(StrongBox<object>.Value));
        }

        internal static Expression AsQueryParam(this object value) => Parameterize(value);

        internal static Expression AsQueryParam(this object? value, TypeViewModel type) 
            => Parameterize(value, type.TypeInfo);

        public static string? GetDebugView(this Expression exp)
        {
            if (exp == null)
            {
                return null;
            }

            var propertyInfo = typeof(Expression).GetProperty("DebugView", BindingFlags.Instance | BindingFlags.NonPublic);
            return propertyInfo?.GetValue(exp) as string;
        }
    }

    internal static class MethodInfos
    {
        internal static readonly MethodInfo StringToLower
            = typeof(string).GetRuntimeMethod(nameof(string.ToLower), [])!;

        internal static readonly MethodInfo StringEquals
            = typeof(string).GetRuntimeMethod(nameof(string.Equals), [typeof(string)])!;

        internal static readonly MethodInfo StringContains
            = typeof(string).GetRuntimeMethod(nameof(string.Contains), [typeof(string)])!;

        internal static readonly MethodInfo StringStartsWith
            = typeof(string).GetRuntimeMethod(nameof(string.StartsWith), [typeof(string)])!;
    }
}
