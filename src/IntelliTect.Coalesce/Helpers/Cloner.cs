using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Helpers
{
    public static class Cloner
    {
        private static System.Collections.Concurrent.ConcurrentDictionary<Type, Type> mappedTypes = new ConcurrentDictionary<Type, Type>();

        /// <summary>
        /// Makes a shallow copy of an object. Public properties only.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T Copy<T>(this T source) where T : class, new()
        {
            Type type = typeof(T);
            var dest = new T();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(f=>f.CanWrite);
            foreach (var property in properties)
            {
                var value = property.GetValue(source);
                property.SetValue(dest, value);
            }
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (var field in fields)
            {
                var value = field.GetValue(source);
                field.SetValue(dest, value);
            }
            return dest;
        }
    }

    
}
