using IntelliTect.Coalesce.Mapping.IncludeTrees;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

// Deliberately namespaced to IntelliTect.Coalesce for ease of use.
namespace IntelliTect.Coalesce
{
    public static class IncludeTreeExtensions
    {
        public static IncludeTree GetIncludeTree(this IQueryable queryable, string rootName = null)
        {
            var expression = queryable.Expression;
            IncludeTree root = new IncludeTree { PropertyName = rootName };
            IncludeTree currentNode = null;
            IncludeTree head, tail;

            // When we get to the root of the queryable, it won't be a MethodCallExpression.
            while (true)
            {
                switch (expression)
                {
                    case MethodCallExpression callExpr:
                        // callExpr.Arguments[0] is the entire previous query.
                        // callExpr.Arguments[1] is the lambda for the property specifier

                        if (callExpr.Method.Name == nameof(EntityFrameworkQueryableExtensions.Include)
                         || callExpr.Method.Name == nameof(EntityFrameworkQueryableExtensions.ThenInclude))
                        {
                            if (callExpr.Arguments[1] is UnaryExpression unary)
                            {
                                // I'm like a wizard with all these casts.
                                var body = ((MemberExpression)((LambdaExpression)unary.Operand).Body);
                                head = IncludeTree.ParseMemberExpression(body, out tail);
                            }
                            else if (callExpr.Arguments[1] is ConstantExpression constant)
                            {
                                head = IncludeTree.ParseConstantExpression(constant, out tail);
                            }
                            else
                            {
                                throw new InvalidOperationException($"Unhandled .{callExpr.Method.Name} expression type {callExpr.Arguments[1].GetType()}");
                            }
                    
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
                        break;

                    case IncludedSeparatelyExpression includeExpr:
                        head = IncludeTree.ParseMemberExpression(includeExpr.IncludedExpression, out tail);

                        // If we had a child from a ThenInclude, add it to the tail of this node.
                        if (currentNode != null)
                            tail.AddChild(currentNode);

                        // Save the head of this expression in case we're parsing a ThenInclude.
                        currentNode = head;

                        if (includeExpr.IsRoot)
                        {
                            // Finally, add the current node to the root, since a .Include doesn't have parents.
                            root.AddChild(head);
                            currentNode = null;
                        }

                        // Reduce will give us the previous expression.
                        expression = includeExpr.Reduce();
                        break;

                    default:
                        return root;
                }
            }
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
            var body = ((MemberExpression)expr.Body);

            return new IncludedSeparatelyQueryable<TEntity, TProperty>(query.Provider.CreateQuery<TEntity>(
                new IncludedSeparatelyExpression(query.Expression, body, true)
            ));

        }

        public static IIncludedSeparatelyQueryable<TEntity, TProperty> ThenIncluded<TEntity, TPreviousProperty, TProperty>(
               this IIncludedSeparatelyQueryable<TEntity, IEnumerable<TPreviousProperty>> query,
               Expression<Func<TPreviousProperty, TProperty>> navigationPropertyPath)
            where TEntity : class
        {
            var body = ((MemberExpression)navigationPropertyPath.Body);

            return new IncludedSeparatelyQueryable<TEntity, TProperty>(query.Provider.CreateQuery<TEntity>(
                new IncludedSeparatelyExpression(query.Expression, body, false)
            ));
        }
    }

    class IncludedSeparatelyExpression : Expression
    {
        private readonly Expression expression;

        public MemberExpression IncludedExpression { get; }
        public bool IsRoot { get; }

        public IncludedSeparatelyExpression(Expression expression, MemberExpression includedExpression, bool isRoot)
        {
            this.expression = expression;
            IncludedExpression = includedExpression;
            IsRoot = isRoot;
        }

        public override ExpressionType NodeType => ExpressionType.Extension;

        public override Type Type => expression.Type;

        public override bool CanReduce => true;

        public override Expression Reduce() => expression;
    }
}
