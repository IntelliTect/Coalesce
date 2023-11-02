using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public class ReflectionAttributeViewModel<TAttribute> : AttributeViewModel<TAttribute>
        where TAttribute : Attribute
    {
        public TAttribute Instance { get; private set; }

        public ReflectionAttributeViewModel(TAttribute instance)
        {
            Instance = instance;
        }

        public override object? GetValue(string valueName)
        {
            var property = Instance.GetType().GetProperty(valueName);
            if (property == null) return null;

            // Some attributes have getters that throw if the value was never set, hence the try/catch.
            // E.g. DisplayAttribute.Order
            try
            {
                return property.GetValue(Instance, null);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }

    public static class ReflectionExtensions
    {
        public static IEnumerable<ReflectionAttributeViewModel<TAttribute>> GetAttributes<TAttribute>(
            this ICustomAttributeProvider member
        )
            where TAttribute : Attribute
            => member.GetCustomAttributes(typeof(TAttribute), true)
                .OfType<TAttribute>()
                .Select(a => new ReflectionAttributeViewModel<TAttribute>(a));

        public static TAttribute? GetAttribute<TAttribute>(
            this ICustomAttributeProvider member
        )
            where TAttribute : Attribute
            => member.GetAttributes<TAttribute>().Select(a => a.Instance).FirstOrDefault();
    }
}
