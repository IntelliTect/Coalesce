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
        public static IncludeTree GetIncludeTree(this IQueryable queryable)
        {
            var expression = queryable.Expression;
            IncludeTree root = (queryable.Provider as IncludableQueryProvider)?.IncludeTree ?? new IncludeTree();
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
