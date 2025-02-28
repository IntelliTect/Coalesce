using IntelliTect.Coalesce.Mapping.IncludeTrees;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

// Deliberately namespaced to IntelliTect.Coalesce for ease of use.
namespace IntelliTect.Coalesce
{
    public static class IncludeTreeExtensions
    {
        public static IncludeTree GetIncludeTree(this IQueryable queryable, string? rootName = null)
        {
            var expression = queryable.Expression;
            IncludeTree root = new IncludeTree { PropertyName = rootName };

            var visitor = new IncludeTreeVisitor();
            visitor.Visit(expression);
            foreach (var tree in visitor.Trees) root.AddChild(tree);

            return root;
        }

        internal class IncludeTreeVisitor : ExpressionVisitor
        {
            public List<IncludeTree> Trees { get; } = [];

            IncludeTree? tail;

            protected override Expression VisitMemberInit(MemberInitExpression node)
            {
                foreach (var binding in node.Bindings)
                {
                    if (binding is not MemberAssignment assignment) continue;

                    var member = assignment.Member;
                    var arg = assignment.Expression;
                    VisitMemberAssignment(member, arg);
                }

                return node;
            }

            protected override Expression VisitNew(NewExpression node)
            {
                if (node.Members is null) return node;

                for (var i = 0; i < node.Arguments.Count; i++)
                {
                    var member = node.Members[i];
                    var arg = node.Arguments[i];
                    VisitMemberAssignment(member, arg);
                }

                return node;
            }

            private void VisitMemberAssignment(MemberInfo member, Expression arg)
            {
                if (
                    member is not PropertyInfo pi ||
                    !ReflectionRepository.Global.GetOrAddType(pi.PropertyType).PureType.IsPOCO
                )
                {
                    return;
                }

                var memberTree = new IncludeTree { PropertyName = member.Name };
                Trees.Add(memberTree);

                var subVisitor = new IncludeTreeVisitor();
                subVisitor.Visit(arg);
                foreach (var childTree in subVisitor.Trees)
                {
                    memberTree.AddChild(childTree);
                }
            }

            protected override Expression VisitExtension(Expression node)
            {
                if (node is not IncludedSeparatelyExpression includeExpr) return base.VisitExtension(node);

                // visit everyting up until this point
                Visit(includeExpr.Reduce());

                IncludeTree? oldTail = tail;
                var head = IncludeTree.ParseMemberExpression(includeExpr.IncludedExpression, out tail);

                if (includeExpr.IsRoot)
                {
                    // Finally, add the current node as a root, since a .Include doesn't have parents.
                    Trees.Add(head);
                } 
                else
                {
                    oldTail!.AddChild(head);
                }

                // Reduce will give us the previous expression.
                return node;
            }

            protected override Expression VisitMethodCall(MethodCallExpression node)
            {
                if (node.Method.Name == nameof(Queryable.Select))
                {
                    // Only the body of the select is important.
                    // Don't visit the target of the select, because it is either:
                    // - A collection navigation or similar, which doesn't have anything useful in it.
                    // - An earlier part of the query, which doesn't affect the ultimate projection.
                    //   NOTE: This isn't strictly true. If we're re-projecting something that was projected
                    //   with a more detailed definition earlier in the query, we should see about preserving the original.

                    Visit(node.Arguments[1]);
                }
                else if (node.Method.Name == nameof(EntityFrameworkQueryableExtensions.Include)
                     || node.Method.Name == nameof(EntityFrameworkQueryableExtensions.ThenInclude)
                )
                {
                    Visit(node.Arguments[0]);

                    IncludeTree head;
                    IncludeTree? oldTail = tail;
                    if (node.Arguments[1] is UnaryExpression unary)
                    {
                        // Handles lambda includes
                        var body = FindMemberAccessExpression(unary.Operand);
                        head = IncludeTree.ParseMemberExpression(body, out tail);
                    }
                    else if (node.Arguments[1] is ConstantExpression constant)
                    {
                        // Handles string includes
                        head = IncludeTree.ParseConstantExpression(constant, out tail);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Unhandled .{node.Method.Name} expression type {node.Arguments[1].GetType()}");
                    }

                    if (node.Method.Name == nameof(EntityFrameworkQueryableExtensions.Include))
                    {
                        // Finally, add the current node as a root, since a .Include doesn't have parents.
                        Trees.Add(head);
                    }
                    else
                    {
                        oldTail!.AddChild(head);
                    }
                }
                else
                {
                    // Not the method we're looking for. Climb up the method call chain.
                    Visit(node.Arguments[0]);
                }

                return node;
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
            var body = FindMemberAccessExpression(expr);

            return new IncludedSeparatelyQueryable<TEntity, TProperty>(query.Provider.CreateQuery<TEntity>(
                new IncludedSeparatelyExpression(query.Expression, body, true)
            ));

        }

        public static IIncludedSeparatelyQueryable<TEntity, TProperty> ThenIncluded<TEntity, TPreviousProperty, TProperty>(
               this IIncludedSeparatelyQueryable<TEntity, IEnumerable<TPreviousProperty>> query,
               Expression<Func<TPreviousProperty, TProperty>> navigationPropertyPath)
            where TEntity : class
        {
            var body = FindMemberAccessExpression(navigationPropertyPath);

            return new IncludedSeparatelyQueryable<TEntity, TProperty>(query.Provider.CreateQuery<TEntity>(
                new IncludedSeparatelyExpression(query.Expression, body, false)
            ));
        }

        /// <summary>
        /// Walks down an method chain expression tree, returning the original member
        /// that the first chained method was called upon.
        /// </summary>
        private static MemberExpression FindMemberAccessExpression(Expression expr)
        {
            return expr switch
            {
                // This is what we're after:
                MemberExpression member => member,
                // The root expression
                LambdaExpression lambda => FindMemberAccessExpression(lambda.Body),
                // For chained LINQ methods, the previous method call is the first argument.
                MethodCallExpression methodCall => FindMemberAccessExpression(methodCall.Arguments.First()),
                _ => throw new ArgumentException($"Unsupported expression type {expr.NodeType} encounted while building IncludeTree.")
            };
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
