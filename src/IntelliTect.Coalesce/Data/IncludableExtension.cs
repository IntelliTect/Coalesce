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
using IntelliTect.Coalesce.TypeDefinition;
using System.Threading;
using System.Collections;

namespace IntelliTect.Coalesce.Data
{
    public static class IncludableExtension
    {
        internal static MethodInfo IncludeMethodInfo { get; }
        internal static MethodInfo ThenIncludeAfterCollectionMethodInfo { get; }

        static IncludableExtension()
        {
            IncludeMethodInfo = typeof(EntityFrameworkQueryableExtensions)
                .GetTypeInfo().GetDeclaredMethods(nameof(EntityFrameworkQueryableExtensions.Include))
                .Single(mi => mi.GetParameters().Any(
                    pi => pi.Name == "navigationPropertyPath" && pi.ParameterType != typeof(string)));

            ThenIncludeAfterCollectionMethodInfo = typeof(EntityFrameworkQueryableExtensions)
                .GetTypeInfo().GetDeclaredMethods(nameof(EntityFrameworkQueryableExtensions.ThenInclude))
                .Single(mi =>
                {
                    var typeInfo = mi.GetParameters()[0].ParameterType.GenericTypeArguments[1].GetTypeInfo();
                    return typeInfo.IsGenericType
                           && typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>);
                });

        }






        /// <summary>
        /// Includes sub objects from the graph based on IIncludable method on class.
        /// </summary>------
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="include"></param>
        /// <returns></returns>
        public static IQueryable<T> Includes<T>(this IQueryable<T> query, string includes = null) where T : class, new()
        {
            T obj = new T();
            var objT = obj as IIncludable<T>;
            if (objT != null)
            {
                return objT.Include(query, includes);
            }
            else
            {
                query = query.IncludeChildren();
            }
            return query;
        }


        /// <summary>
        /// Includes immediate children, as well as the other side of many-to-many relationships.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <returns></returns>
        public static IQueryable<T> IncludeChildren<T>(this IQueryable<T> query) where T : class
        {
            var model = ReflectionRepository.Global.GetClassViewModel<T>();
            foreach (var prop in model.ClientProperties.Where(f => !f.IsStatic && f.PureType.HasClassViewModel && f.PureType.ClassViewModel.HasDbSet && !f.HasNotMapped))
            {
                if (prop.IsManytoManyCollection)
                {
                    query = query.Include(prop.Name + "." + prop.ManyToManyCollectionProperty.Name);
                }
                else
                {
                    query = query.Include(prop.Name);
                }
            }
            return query;
        }


        // Async disabled because of https://github.com/aspnet/EntityFrameworkCore/issues/9038.
        // Renable once microsoft releases the fix and we upgrade our references.
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        /// <summary>
        /// Asynchronously finds an object based on key after an include has been done.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async static Task<T> FindItemAsync<T>(this IQueryable<T> query, object id)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {

            var classViewModel = ReflectionRepository.Global.GetClassViewModel<T>();
            if (classViewModel.PrimaryKey.Type.IsString)
            {
                // return await query.Where($@"{classViewModel.PrimaryKey.Name} = ""{id}""").FirstAsync();
                return query.Where($@"{classViewModel.PrimaryKey.Name} = ""{id}""").First();
            }
            else
            {
                // return await query.Where(string.Format("{0} = {1}", classViewModel.PrimaryKey.Name, id)).FirstAsync();
                return query.Where(string.Format("{0} = {1}", classViewModel.PrimaryKey.Name, id)).First();
            }
        }

        /// <summary>
        /// Finds an object based on key after an include has been done.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static T FindItem<T>(this IQueryable<T> query, object id)
        {
            var classViewModel = ReflectionRepository.Global.GetClassViewModel<T>();
            if (classViewModel.PrimaryKey.Type.IsString)
            {
                return query.Where(string.Format("{0} = \"{1}\"", classViewModel.PrimaryKey.Name, id)).First();
            }
            else
            {
                return query.Where(string.Format("{0} = {1}", classViewModel.PrimaryKey.Name, id)).First();
            }
        }

        static Expression CreateExpression(Type type, string propertyName)
        {
            var param = Expression.Parameter(type, "x");
            Expression body = param;
            body = Expression.PropertyOrField(body, propertyName);
            return Expression.Lambda(body, param);
        }
    }
}
