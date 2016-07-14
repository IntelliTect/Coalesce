using AutoMapper;
using AutoMapper.Mappers;
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
        private static IMapper ObjMapper;
        private static MapperConfiguration ObjConfiguration;
        private static System.Collections.Concurrent.ConcurrentDictionary<Type, Type> mappedTypes = new ConcurrentDictionary<Type, Type>();

        /// <summary>
        /// Broken: Makes a deep copy of the object using AutoMapper.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T Clone<T>(this T source) where T : class, new()
        {
            // Create the local mapping configuration for the clones. 
            if (ObjMapper == null)
            {
                ObjConfiguration = new MapperConfiguration(cfg => cfg.CreateMap<T, T>());
                ObjMapper = ObjConfiguration.CreateMapper();
            }

            Type type = typeof(T);

            // Create a new one if the map doesn't exist.
            if (!mappedTypes.ContainsKey(type))
            {
                lock (ObjMapper)
                {
                    // Check again, now that we are locked. This way we don't create two.
                    if (!mappedTypes.ContainsKey(type))
                    {
                        // TODO: Fix this line to make the cloner work.
                        //ObjConfiguration.CreateMap<T, T>();
                        mappedTypes.GetOrAdd(type, type);
                    }
                }
            }

            // Create a new one.
            T dest = new T();
            // Map the fields.
            ObjMapper.Map(source, dest);

            return dest;
        }


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
