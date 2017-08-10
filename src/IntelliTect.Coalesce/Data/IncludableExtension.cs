using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Dynamic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Reflection;
using IntelliTect.Coalesce.Data;
using System.Reflection.Emit;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Threading;
using IntelliTect.Coalesce.Helpers;
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
        /// </summary>
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
        public static IQueryable<T> IncludeChildren<T>(this IQueryable<T> query) where T : class, new()
        {
            var model = ReflectionRepository.GetClassViewModel<T>();
            foreach (var prop in model.Properties.Where(f => !f.IsStatic && !f.IsInternalUse && f.PureType.HasClassViewModel && f.PureType.ClassViewModel.HasDbSet && !f.HasNotMapped))
            {
                if (prop.IsManytoManyCollection)
                {
                    query = query.IncludeString(prop.Name + "." + prop.ManyToManyCollectionProperty.Name);
                }
                else
                {
                    query = query.IncludeString(prop.Name);
                }
            }
            return query;
        }


        /// <summary>
        /// Asynchronously finds an object based on key after an include has been done.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async static Task<T> FindItemAsync<T>(this IQueryable<T> query, object id)
        {
            var classViewModel = ReflectionRepository.GetClassViewModel<T>();
            if (classViewModel.PrimaryKey.Type.IsString)
            {
                return await query.Where($@"{classViewModel.PrimaryKey.Name} = ""{id}""").FirstAsync();
            }
            else
            {
                return await query.Where(string.Format("{0} = {1}", classViewModel.PrimaryKey.Name, id)).FirstAsync();
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
            var classViewModel = ReflectionRepository.GetClassViewModel<T>();
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

        // to get this to work, code was referenced and borrowed from https://github.com/aspnet/EntityFramework/blob/dev/src/EntityFramework.Core/EntityFrameworkQueryableExtensions.cs
        public static IQueryable<T> IncludeString<T>(this IQueryable<T> query, string include) where T : class, new()
        {
            // need to get the first include, and then append the thenincludes
            Type childType, parentType = typeof(T);
            MethodInfo includeMethodInfo;
            MethodInfo thenIncludeMethodInfo;
            MethodCallExpression methodCallExpression;
            IIncludableQueryable<T, object> resultQuery;
            string[] includeProperties = include.Split('.');

            childType = parentType.GetProperty(includeProperties[0], BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).PropertyType;

            includeMethodInfo = IncludeMethodInfo.MakeGenericMethod(parentType, childType);
            methodCallExpression = Expression.Call(
                    includeMethodInfo,
                    query.Expression,
                    CreateExpression(parentType, includeProperties[0]));

            resultQuery = new IncludableQueryable<T, object>(query.Provider.CreateQuery<T>(methodCallExpression));

            for (int i = 1; i < includeProperties.Length; i++)
            {
                parentType = childType;
                if (parentType.IsGenericType)
                {
                    parentType = parentType.GetGenericArguments().First();
                    childType = parentType.GetProperty(includeProperties[i], BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance).PropertyType;
                    thenIncludeMethodInfo = ThenIncludeAfterCollectionMethodInfo.MakeGenericMethod(typeof(T), parentType, childType);
                    methodCallExpression = Expression.Call(
                       thenIncludeMethodInfo,
                       resultQuery.Expression,
                       CreateExpression(parentType, includeProperties[i]));
                }

                resultQuery = new IncludableQueryable<T, object>(resultQuery.Provider.CreateQuery<T>(methodCallExpression));
            }

            return resultQuery;
        }

    }
}
