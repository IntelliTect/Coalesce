using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Reflection;
using IntelliTect.Coalesce.Data;
using System.Reflection.Emit;
using IntelliTect.Coalesce.TypeDefinition;

namespace IntelliTect.Coalesce.Data
{
    public static class IncludeExternalExtension
    {
        private static Dictionary<string, IEnumerable<object>> _repositories = new Dictionary<string, IEnumerable<object>>();


        /// <summary>
        /// Includes sub objects from the graph based on IIncludeExternal method on class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="include"></param>
        /// <returns></returns>
        public static IEnumerable<T> IncludesExternal<T>(this IEnumerable<T> query, string includes = null) where T : class, new()
        {
            T obj = new T();
            var objT = obj as IIncludeExternal<T>;
            if (objT != null)
            {
                return objT.IncludeExternal(query, includes);
            }
            else
            {
                var model = ReflectionRepository.Global.GetClassViewModel<T>();
                foreach (var prop in model.ClientExposedProperties.Where(f => f.IsExternal))
                {
                    // TODO: need to figure out how to do this without a <T>
                    //query = query.IncludeExternal(prop);
                }

            }
            return query;
        }

        /// <summary>
        /// Includes sub objects from the graph based on IIncludeExternal method on class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="include"></param>
        /// <returns></returns>
        public static T IncludeExternal<T>(this T item, string includes = null) where T : class, new()
        {
            // Note: If this is named IncludesExternal, C# doesn't call the right one. It gets in a loop.
            if (item != null)
            {
                var list = new List<T> { item };
                list.IncludesExternal(includes);
            }
            return item;
        }


        public static IEnumerable<T> IncludeExternal<T, TProperty>(this IEnumerable<T> col, Expression<Func<T, TProperty>> propertySelector) where T : class
        {
            foreach (var obj in col)
            {
                obj.IncludeExternal(propertySelector);
            }

            return col;
        }

        public static T IncludeExternal<T, TProperty>(this T obj, Expression<Func<T, TProperty>> propertySelector) where T : class
        {
            var objViewModel = ReflectionRepository.Global.GetClassViewModel<T>();
            var propViewModel = objViewModel.PropertyBySelector(propertySelector);
            string type = propViewModel.Type.Name;
            if (_repositories.ContainsKey(type))
            {
                var list = _repositories[type] as IEnumerable<TProperty>;
                // Get item from the list
                // Get ID field
                var idProperty = propViewModel.ObjectIdProperty.PropertyInfo;
                // get ID value
                object idValue = idProperty.GetValue(obj);
                if (idValue != null)
                {
                    // Look up object by ID field and ID value.
                    //object objectValue = list.Where($"{propViewModel.ObjectIdProperty.Name} = {idValue}").FirstOrDefault();
                    object objectValue = list
                        .AsQueryable()
                        .Where($"{propViewModel.PureType.ClassViewModel.PrimaryKey.Name} = {idValue}")
                        .FirstOrDefault();
                    // Set the value.
                    propViewModel.PropertyInfo.SetValue(obj, objectValue);
                }
            }

            return obj;
        }


        /// <summary>
        /// Registers a collection as being 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void Register<T>(IEnumerable<T> list) where T : class
        {
            var type = typeof(T).Name;
            if (!_repositories.ContainsKey(type))
            {
                _repositories.Add(type, list);
            }
        }



    }

}
