using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Helpers.IncludeTree
{
    public static class IncludeTreeExtensions
    {
        public static IncludeTree GetIncludeTree(this IQueryable queryable, string rootName = null)
        {
            var expression = queryable.Expression;
            IncludeTree root = (queryable.Provider as IncludableQueryProvider)?.IncludeTree ?? new IncludeTree { PropertyName = rootName };
            IncludeTree currentNode = null;

            // When we get to the root of the queryable, it won't be a MethodCallExpression.
            while (expression is MethodCallExpression)
            {
                // callExpr.Arguments[0] is the entire previous query.
                // callExpr.Arguments[1] is the lambda for the property specifier
                var callExpr = expression as MethodCallExpression;

                if (callExpr.Method.Name == nameof(EntityFrameworkQueryableExtensions.Include)
                 || callExpr.Method.Name == nameof(EntityFrameworkQueryableExtensions.ThenInclude))
                {
                    // I'm like a wizard with all these casts.
                    var body = ((MemberExpression)((LambdaExpression)((UnaryExpression)callExpr.Arguments[1]).Operand).Body);

                    IncludeTree tail;
                    var head = IncludeTree.ParseMemberExpression(body, out tail);

                    // If we had a child from a ThenInclude, add it to the tail of this node.
                    if (currentNode != null)
                        tail.AddChild(currentNode);

                    // Save the head of this expression in case we're parsing a ThenInclude.
                    currentNode = head;

                    if (callExpr.Method.Name == nameof(EntityFrameworkQueryableExtensions.Include))
                    {
                        // Finally, add the current node to the root, since a .Include doesn't have parents.
                        root.AddChild(head);
                        currentNode = null;
                    }
                }

                expression = callExpr.Arguments[0];
            }

            return root;
        }


        internal static IQueryable<T> CaptureSeparateIncludes<T>(this IQueryable<T> queryable)
        {
            return new IncludableQueryProvider.WrappedProviderQueryable<T>(queryable, new IncludableQueryProvider(queryable.Provider as EntityQueryProvider));
        }

        /// <summary>
        /// Specifies additional related entities to include in serialized output of a Coalesce custom data source. 
        ///     The specified entities must have already been loaded into the context that was provided for the data source.
        ///     to be included is specified starting with the type of entity being queried (TEntity).
        ///     If you wish to include additional types based on the navigation properties of
        ///     the type being included, then chain a call to .ThenIncluded(prop => prop.Child)
        ///     after this call.
        ///     
        ///     The purpose of this is to allow loading subsets of navigation properties without neccesarily loading the entire contents of the navigation property.
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="query"></param>
        /// <param name="expr">An expression which returns the navigation property that should be included in the data source's output.</param>
        /// <returns></returns>
        public static IIncludedSeparatelyQueryable<TEntity, TProperty> IncludedSeparately<TEntity, TProperty>(
            this IQueryable<TEntity> query,
            Expression<Func<TEntity, TProperty>> expr) where TEntity : class
        {
            var provider = query.Provider as IncludableQueryProvider;
            if (provider == null)
            {
                query = query.CaptureSeparateIncludes();
                provider = query.Provider as IncludableQueryProvider;
            }

            IncludeTree tail;
            var body = ((MemberExpression)expr.Body);
            var head = IncludeTree.ParseMemberExpression(body, out tail);

            // Add the head of this expression to the root includetree for the query.
            tail = provider.IncludeTree.AddLinearChild(head);


            return new IncludedSeparatelyQueryable<TEntity, TProperty>(query) { IncludeTree = tail };
        }

        public static IIncludedSeparatelyQueryable<TEntity, TProperty> ThenIncluded<TEntity, TPreviousProperty, TProperty>(
               this IIncludedSeparatelyQueryable<TEntity, IEnumerable<TPreviousProperty>> source,
               Expression<Func<TPreviousProperty, TProperty>> navigationPropertyPath)
            where TEntity : class
        {

            IncludeTree tail;
            var body = ((MemberExpression)navigationPropertyPath.Body);
            var head = IncludeTree.ParseMemberExpression(body, out tail);

            // Merge in the head of this tree as a child of the parent tree.
            tail = source.IncludeTree.AddLinearChild(head);

            return new IncludedSeparatelyQueryable<TEntity, TProperty>(source) { IncludeTree = tail };
        }
    }
}
