using Intellitect.ComponentModel.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Linq.Dynamic;
using Intellitect.ComponentModel.TypeDefinition;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Reflection;
using Intellitect.ComponentModel.Data;
using System.Reflection.Emit;

namespace Intellitect.ComponentModel.Data
{
    public static class IncludableExtension
    {
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
                var model = ReflectionRepository.GetClassViewModel<T>();
                foreach (var prop in model.Properties.Where(f => !f.IsInternalUse && f.PureType.HasClassViewModel && f.PureType.ClassViewModel.HasDbSet))
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
        public async static Task<T> FindItemAsync<T>(this IQueryable<T> query, string id)
        {
            var classViewModel = ReflectionRepository.GetClassViewModel(typeof(T));
            if (classViewModel.PrimaryKey.Type.IsString)
            {
                return await query.Where($@"{classViewModel.PrimaryKey.Name} = ""{id}""").FirstAsync();
            }
            else {
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
            var classViewModel = ReflectionRepository.GetClassViewModel(typeof(T));
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

        internal static readonly MethodInfo IncludeMethodInfo
            = typeof(EntityFrameworkQueryableExtensions)
                .GetTypeInfo().GetDeclaredMethods("Include")
                .Single(mi => mi.GetParameters().Any(pi => pi.Name == "navigationPropertyPath"));

        internal static readonly MethodInfo ThenIncludeAfterCollectionMethodInfo
            = typeof(EntityFrameworkQueryableExtensions)
                .GetTypeInfo().GetDeclaredMethods(nameof(EntityFrameworkQueryableExtensions.ThenInclude))
                .Single(mi => !mi.GetParameters()[0].ParameterType.GenericTypeArguments[1].IsGenericParameter);

        // to get this to work, code was referenced and borrowed from https://github.com/aspnet/EntityFramework/blob/dev/src/EntityFramework.Core/EntityFrameworkQueryableExtensions.cs
        public static IQueryable<T> IncludeString<T>(this IQueryable<T> query, string include) where T: class, new()
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

            for(int i = 1; i < includeProperties.Length; i++)
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
