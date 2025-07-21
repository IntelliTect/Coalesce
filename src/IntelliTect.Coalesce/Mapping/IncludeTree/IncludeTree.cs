using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;

namespace IntelliTect.Coalesce;

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

    public void AddChild(string propertyName) => AddChild(new IncludeTree { PropertyName = propertyName });

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

    public override string ToString() => $"{PropertyName ?? "<root>"} -> {_children.Count}";

#pragma warning disable EF1001 // Internal EF Core API usage.

    /// <summary>
    /// Fake EF query compiler, used so we can build include trees with real EntityQueryProvider
    /// so that .Include() and .ThenInclude() work (they require an EntityQueryProvider).
    /// </summary>
    private class FakeQueryCompiler : IQueryCompiler
    {
        public Func<QueryContext, TResult> CreateCompiledAsyncQuery<TResult>(Expression query) => Throw<Func<QueryContext, TResult>>();
        public Func<QueryContext, TResult> CreateCompiledQuery<TResult>(Expression query) => Throw<Func<QueryContext, TResult>>();
        public TResult Execute<TResult>(Expression query) => Throw<TResult>();
        public TResult ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken) => Throw<TResult>();
        public Expression<Func<QueryContext, TResult>> PrecompileQuery<TResult>(Expression query, bool async) => Throw<Expression<Func<QueryContext, TResult>>>();

        [DoesNotReturn]
        private T Throw<T>()
        {
            throw new InvalidOperationException(
                "The query being operated on was created from IncludeTree for the purpose of capturing the structure of .Include() calls." +
                "It cannot be evaluated or executed.");
        }
    }

    /// <summary>
    /// Shorthand for <code>Enumerable.Empty&lt;T&gt;().AsQueryable()</code>,
    /// which can be used to build up a query upon which <code>GetIncludeTree</code> can be called.
    /// </summary>
    public static IQueryable<T> QueryFor<T>()
    {
        return new EntityQueryProvider(new FakeQueryCompiler())
            .CreateQuery<T>(Expression.Constant(Enumerable.Empty<T>().AsQueryable()));
    }

    /// <summary>
    /// Build an include tree for the specified type by providing a function
    /// that will build up the query by calling <code>IncludedSeprately</code> 
    /// and <code>ThenIncludedSeprately</code> on the query.
    /// </summary>
    public static IncludeTree For<T>(Func<IQueryable<T>, IQueryable<T>> builder)
    {
        return builder(QueryFor<T>()).GetIncludeTree();
    }

#pragma warning restore EF1001 // Internal EF Core API usage.

    internal static IncludeTree ParseMemberExpression(MemberExpression expr, out IncludeTree tail)
    {
        var head = tail = new IncludeTree();
        head.PropertyName = expr.Member.Name;

        // If this lambda was a multi-level property specifier, walk up the chain and add each property as the parent of currentNode.
        // For example, .Include(e => e.Application.ApplicationType)
        var current = expr.Expression;
        while (UnwrapUnary(current!) is MemberExpression memberExpression)
        {
            var newNode = new IncludeTree();
            newNode.PropertyName = memberExpression.Member.Name;
            newNode.AddChild(head);
            head = newNode;

            current = memberExpression.Expression;
        }

        return head;
    }

    private static Expression UnwrapUnary(Expression expr)
    {
        while (expr is UnaryExpression unary) expr = unary.Operand;
        return expr;
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
        [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)]
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
