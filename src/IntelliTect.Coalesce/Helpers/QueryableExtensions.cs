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
        public static IQueryable<T> IncludeChildren<T>(this IQueryable<T> query, ReflectionRepository? reflectionRepository = null) where T : class
        {
            var model = (reflectionRepository ?? ReflectionRepository.Global).GetClassViewModel<T>() ?? throw new ArgumentException("Queried type is not a class");
            foreach (var prop in model.ClientProperties.Where(f => !f.IsStatic && f.Object?.HasDbSet == true && !f.HasNotMapped))
            {
                if (prop.IsManytoManyCollection)
                {
                    query = query.Include(prop.Name + "." + prop.ManyToManyCollectionProperty!.Name);
                }
                else
                {
                    query = query.Include(prop.Name);
                }
            }
            return query;
        }

        /// <summary>
        /// Filters a query by a given primary key value.
        /// </summary>
        /// <returns>The filtered query.</returns>
        public static IQueryable<T> WherePrimaryKeyIs<T>(this IQueryable<T> query, object id, ReflectionRepository? reflectionRepository = null)
        {
            var classViewModel = (reflectionRepository ?? ReflectionRepository.Global).GetClassViewModel<T>() ?? throw new ArgumentException("Queried type is not a class");
            var pkProp = classViewModel.PrimaryKey.PropertyInfo;
            return query.Where($"{pkProp.Name} == @0", id);
        }

        /// <summary>
        /// Asynchronously finds an object based on a specific primary key value.
        /// </summary>
        /// <returns>The desired item, or null if it was not found.</returns>
        public static Task<T> FindItemAsync<T>(this IQueryable<T> query, object id, ReflectionRepository? reflectionRepository = null)
        {
            return query.WherePrimaryKeyIs(id, reflectionRepository).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Finds an object based on a specific primary key value.
        /// </summary>
        /// <returns>The desired item, or null if it was not found.</returns>
        public static T FindItem<T>(this IQueryable<T> query, object id, ReflectionRepository? reflectionRepository = null)
        {
            return query.WherePrimaryKeyIs(id, reflectionRepository).FirstOrDefault();
        }
    }
}
