﻿using IntelliTect.Coalesce.Mapping.IncludeTrees;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce
{
    /// <summary>
    ///    Represents a hierarchy of entity properties which should be included in results sent to clients.
    /// </summary>
    public class IncludeTree : IReadOnlyDictionary<string, IncludeTree>
    {
        private readonly Dictionary<string, IncludeTree> _children = new Dictionary<string, IncludeTree>();

        public string? PropertyName { get; set; }

        public void AddChild(IncludeTree tree)
        {
            if (string.IsNullOrWhiteSpace(tree.PropertyName))
            {
                throw new ArgumentException("New child IncludeTree must have a PropertyName");
            }

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

        /// <summary>
        /// Merges in a linearly-structured IncludeTree, and returns the tail of that tree.
        /// </summary>
        /// <param name="tree"></param>
        /// <returns></returns>
        public IncludeTree AddLinearChild(IncludeTree tree)
        {
            if (string.IsNullOrWhiteSpace(tree.PropertyName))
            {
                throw new ArgumentException("New child IncludeTree must have a PropertyName");
            }

            if (!_children.ContainsKey(tree.PropertyName))
            {
                _children[tree.PropertyName] = new IncludeTree { PropertyName = tree.PropertyName };
            }

            if (tree.Count > 0)
            {
                // Recursively merge into the existing tree. Returns the tail.
                return _children[tree.PropertyName].AddLinearChild(tree.Single().Value);
            }
            else
            {
                // Nothing underneath - the new node is the tail. Return it up the call stack.
                return _children[tree.PropertyName];
            }
        }

        /// <summary>
        /// Shorthand for <code>Enumerable.Empty&lt;T&gt;().AsQueryable()</code>,
        /// which can be used to build up a query upon which <code>GetIncludeTree</code> can be called.
        /// </summary>
        public static IQueryable<T> QueryFor<T>()
        {
            return Enumerable.Empty<T>().AsQueryable();
        }

        /// <summary>
        /// Build an include tree for the specified type by providing a function
        /// that will build up the query by calling <code>IncludedSeprately</code> 
        /// and <code>ThenIncludedSeprately</code> on the query.
        /// </summary>
        public static IncludeTree For<T>(Func<IQueryable<T>, IIncludedSeparatelyQueryable<T, object>> builder)
        {
            return builder(Enumerable.Empty<T>().AsQueryable()).GetIncludeTree();
        }

        internal static IncludeTree ParseMemberExpression(MemberExpression expr, out IncludeTree tail)
        {
            var newNode = tail = new IncludeTree();

            newNode.PropertyName = expr.Member.Name;
            var head = newNode;

            // If this lambda was a multi-level property specifier, walk up the chain and add each property as the parent of currentNode.
            // For example, .Include(e => e.Application.ApplicationType)
            while (expr.Expression.NodeType != ExpressionType.Parameter)
            {
                newNode = new IncludeTree();

                newNode.AddChild(head);

                expr = (MemberExpression)expr.Expression;
                newNode.PropertyName = expr.Member.Name;
                head = newNode;
            }

            return head;
        }

        internal static IncludeTree ParseConstantExpression(ConstantExpression expr, out IncludeTree tail)
        {
            if (!(expr.Value is string stringValue))
            {
                throw new ArgumentException("Cannot parse constant expression with non-string values.");
            }

            var members = stringValue.Split('.');

            IncludeTree? first = null, last = null;

            foreach (var member in members)
            {
                var newNode = new IncludeTree
                {
                    PropertyName = member
                };
                first ??= newNode;

                last?.AddChild(newNode);
                last = newNode;
            }

            if (first == null || last == null)
            {
                throw new ArgumentException("Unable to parse constant expression", nameof(expr));
            }

            tail = last;
            return first;
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

        public bool TryGetValue(
            string key,
#if NETCOREAPP3_1
            [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)]
#endif 
            out IncludeTree value)
        {
            return _children.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        public IncludeTree? this[string key]
        {
#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member.
            get
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member.
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
}
