using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Returns the attributed requested if it exists or null if it does not.
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <returns></returns>
        public static TAttribute GetAttribute<TAttribute>(this ICustomAttributeProvider member) where TAttribute : Attribute
        {
            var attributes = member.GetCustomAttributes(typeof(TAttribute), true);
            return attributes.FirstOrDefault() as TAttribute;
        }

        /// <summary>
        /// Returns true if the attribute exists.
        /// </summary>
        /// <typeparam name="TAttribute"></typeparam>
        /// <returns></returns>
        public static bool HasAttribute<TAttribute>(this ICustomAttributeProvider member) where TAttribute : Attribute
        {
            return member.IsDefined(typeof(TAttribute), true);
        }

        public static Object GetAttributeValue<TAttribute>(this ICustomAttributeProvider member, string valueName) where TAttribute : Attribute
        {
            var attr = member.GetAttribute<TAttribute>();
            if (attr != null)
            {
                var property = attr.GetType().GetProperty(valueName);
                if (property == null) return null;

                // TODO: Some properties throw an exception here. DisplayAttribute.Order. Not sure why.
                try 
                {
                    return property.GetValue(attr, null);
                }
                catch (Exception)
                {
                    return null;
                }
            }
            return null;
        }

    }
}
