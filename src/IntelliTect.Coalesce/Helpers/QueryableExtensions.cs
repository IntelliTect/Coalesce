using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce;

public static class QueryableExtensions
{
    /// <summary>
    /// <para>Includes immediate children, as well as the other side of many-to-many relationships.</para>
    /// <para>Does not include navigations or classes that have <see cref="ReadAttribute.NoAutoInclude"/> or <see cref="CoalesceConfigurationAttribute.NoAutoInclude"/> set.</para>
    /// </summary>
    public static IQueryable<T> IncludeChildren<T>(this IQueryable<T> query, ReflectionRepository? reflectionRepository = null) where T : class
    {
        var model = (reflectionRepository ?? ReflectionRepository.Global).GetClassViewModel<T>()
            ?? throw new ArgumentException("Queried type is not a class");

        foreach (var prop in model.ClientProperties.Where(f => f.CanAutoInclude))
        {
            if (prop.IsManyToManyCollection && prop.ManyToManyFarNavigationProperty.CanAutoInclude)
            {
                query = query.Include(prop.Name + "." + prop.ManyToManyFarNavigationProperty!.Name);
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
        var classViewModel = (reflectionRepository ?? ReflectionRepository.Global).GetClassViewModel<T>()
            ?? throw new ArgumentException("Queried type is not a class");

        var pkProp = classViewModel.PrimaryKey
            ?? throw new ArgumentException("Unable to determine primary key of the queried type");

        return query.WhereExpression(it => Expression.Equal(it.Prop(pkProp), id.AsQueryParam(pkProp.Type)));
    }

    /// <summary>
    /// Asynchronously finds an object based on a specific primary key value.
    /// </summary>
    /// <returns>The desired item, or null if it was not found.</returns>
    public static Task<T?> FindItemAsync<T>(this IQueryable<T> query, object id, ReflectionRepository? reflectionRepository = null, CancellationToken cancellationToken = default)
        where T : class
    {
        return query.WherePrimaryKeyIs(id, reflectionRepository).FirstOrDefaultAsync(cancellationToken)!;
    }

    /// <summary>
    /// Finds an object based on a specific primary key value.
    /// </summary>
    /// <returns>The desired item, or null if it was not found.</returns>
    public static T? FindItem<T>(this IQueryable<T> query, object id, ReflectionRepository? reflectionRepository = null)
        where T : class
    {
        return query.WherePrimaryKeyIs(id, reflectionRepository).FirstOrDefault();
    }

    public static IQueryable<T> OrderBy<T>(
        this IQueryable<T> query,
        IEnumerable<OrderByInformation> orderings)
        where T : class
    {
        bool isFirst = true;

        foreach (var ordering in orderings)
        {
            var expression = ordering!.LambdaExpression<T>();
            query = (IQueryable<T>)ordering.OrderByMethod<T>(isFirst).Invoke(null, [query, expression])!;
            isFirst = false;
        }

        return query;
    }

    internal static IQueryable<T> WhereExpression<T>(
        this IQueryable<T> query,
        Func<ParameterExpression, Expression> predicateBuilder
    )
    {
        var param = Expression.Parameter(typeof(T));
        return query.Where(Expression.Lambda<Func<T, bool>>(predicateBuilder(param), param));
    }
}
