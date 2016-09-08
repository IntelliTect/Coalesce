using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Helpers
{
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
            return ((IReadOnlyDictionary<string, IncludeTree>)_children).ContainsKey(key);
        }

        public IEnumerator<KeyValuePair<string, IncludeTree>> GetEnumerator()
        {
            return ((IReadOnlyDictionary<string, IncludeTree>)_children).GetEnumerator();
        }

        public bool TryGetValue(string key, out IncludeTree value)
        {
            return ((IReadOnlyDictionary<string, IncludeTree>)_children).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IReadOnlyDictionary<string, IncludeTree>)_children).GetEnumerator();
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
            while (expression is MethodCallExpression)
            {
                var callExpr = expression as MethodCallExpression;

                if (callExpr.Method.Name == nameof(EntityFrameworkQueryableExtensions.Include))
                {
                    var newNode = new IncludeTree();

                    if (currentNode != null)
                        newNode.AddChild(currentNode);

                    // TODO: this is probably really fragile?
                    newNode.PropertyName = ((MemberExpression)((LambdaExpression)((UnaryExpression)callExpr.Arguments[1]).Operand).Body).Member.Name;

                    currentNode = null;
                    root.AddChild(newNode);
                }
                else if (callExpr.Method.Name == nameof(EntityFrameworkQueryableExtensions.ThenInclude))
                {
                    var newNode = new IncludeTree();

                    if (currentNode != null)
                        newNode.AddChild(currentNode);

                    // TODO: this is probably really fragile?
                    newNode.PropertyName = ((MemberExpression)((LambdaExpression)((UnaryExpression)callExpr.Arguments[1]).Operand).Body).Member.Name;
                    currentNode = newNode;
                }

                expression = callExpr.Arguments[0];
            }

            return root;
        }
    }
}
