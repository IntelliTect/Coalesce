using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Helpers
{
    /// <summary>
    ///    Represents a hierarchy of entity properties which should be included in results sent to clients.
    /// </summary>
    public class IncludeTree : IReadOnlyDictionary<string, IncludeTree>
    {
        private Dictionary<string, IncludeTree> _children = new Dictionary<string, IncludeTree>();

        public string PropertyName { get; set; }

        public void AddChild(IncludeTree tree)
        {
            if (_children.ContainsKey(tree.PropertyName))
            {
                // Recursively merge into the existing tree.
                foreach (IncludeTree child in tree.Values)
                {
                    _children[tree.PropertyName].AddChild(child);
                }
            }
            else
            {
                _children[tree.PropertyName] = tree;
            }
        }

        #region IReadOnlyDictionary

        public bool ContainsKey(string key)
        {
            return _children.ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<string, IncludeTree>> GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        public bool TryGetValue(string key, out IncludeTree value)
        {
            return _children.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        public IncludeTree this[string key]
        {
            get
            {
                if (!_children.ContainsKey(key)) return null;
                return _children[key];
            }
        }

        public int Count => _children.Count;
        public IEnumerable<string> Keys => _children.Keys;
        public IEnumerable<IncludeTree> Values => _children.Values;

        #endregion
    }

    public static class IncludeTreeExtensions
    {
        public static IncludeTree GetIncludeTree(this IQueryable queryable)
        {
            var expression = queryable.Expression;
            IncludeTree root = new IncludeTree();
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
                    var newNode = new IncludeTree();

                    // If we have a child from a ThenInclude, add it to this node.
                    if (currentNode != null)
                        newNode.AddChild(currentNode);

                    // I'm like a wizard with all these casts.
                    var body = ((MemberExpression)((LambdaExpression)((UnaryExpression)callExpr.Arguments[1]).Operand).Body);

                    newNode.PropertyName = body.Member.Name;
                    currentNode = newNode;

                    // If this lambda was a multi-level property specifier, walk up the chain and add each property as the parent of currentNode.
                    // For example, .Include(e => e.Application.ApplicationType)
                    while (body.Expression.NodeType != ExpressionType.Parameter)
                    {
                        newNode = new IncludeTree();

                        newNode.AddChild(currentNode);

                        body = ((MemberExpression)body.Expression);
                        newNode.PropertyName = body.Member.Name;
                        currentNode = newNode;
                    }

                    if (callExpr.Method.Name == nameof(EntityFrameworkQueryableExtensions.Include))
                    {
                        // Finally, add the current node to the root, since a .Include doesn't have parents.
                        root.AddChild(currentNode);
                        currentNode = null;
                    }
                }

                expression = callExpr.Arguments[0];
            }

            return root;
        }
    }
}
