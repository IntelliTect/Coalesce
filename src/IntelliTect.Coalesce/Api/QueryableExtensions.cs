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

namespace IntelliTect.Coalesce
{
    public static class QueryableExtensions
    {
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
        /// Asynchronously finds an object based on a specific primary key value.
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
        /// Finds an object based on a specific primary key value.
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
    }
}
