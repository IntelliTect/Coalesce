using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query;
using IntelliTect.Coalesce;

namespace IntelliTect.Coalesce.Mapping.IncludeTrees
{
    /// <summary>
    /// Query provider that will capture calls to .IncludedSeparately().
    /// It must inherit from EntityQueryProvider in order to be transparent to EF Core.
    /// Otherwise, EF Core will ignore many of the query extension methods made upon it.
    /// </summary>
    public class IncludableQueryProvider : EntityQueryProvider
    {
        private static readonly IQueryCompiler _nullQueryCompiler = new NullQueryCompiler();

        private readonly IQueryProvider _baseProvider;

        internal IncludeTree IncludeTree { get; } = new IncludeTree();
     
        public IncludableQueryProvider(IQueryProvider baseProvider)
            // Passing _nullQueryCompiler is fine here because we override all the execute methods to use our wrapped object,
            // so the underlying class won't care if the _queryCompiler is doing nothing.
            // IMPORTANT: Don't override the CreateQuery methods to use the baseProvider - we want the provider used there to be ourself.
            // Otherwise, the base provider would end up getting used for subsequent queries.

            // The alternative to doing this is to get the IQueryCompiler from the baseProvider with reflection,
            // but this runs into issues because baseProvider isn't guaranteed to be an EntityQueryProvider. It may be an EnumerableQueryProvider, or something else.
            // base((IQueryCompiler)typeof(EntityQueryProvider).GetField("_queryCompiler", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(baseProvider))
            : base(_nullQueryCompiler)
        {
            _baseProvider = baseProvider;
        }

        public override object Execute(Expression expression)
        {
            return _baseProvider.Execute(expression);
        }

        public override TResult Execute<TResult>(Expression expression)
        {
            return (_baseProvider).Execute<TResult>(expression);
        }

        public override IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            if (!(_baseProvider is IAsyncQueryProvider))
            {
                throw new ArgumentException("Underlying provider for .IncludedSeparately calls was not of type IAsyncQueryProvider");
            }

            return ((IAsyncQueryProvider)_baseProvider).ExecuteAsync<TResult>(expression);
        }

        public override Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            if (!(_baseProvider is IAsyncQueryProvider))
            {
                throw new ArgumentException("Underlying provider for .IncludedSeparately calls was not of type IAsyncQueryProvider");
            }

            return ((IAsyncQueryProvider)_baseProvider).ExecuteAsync<TResult>(expression, cancellationToken);
        }

        /// <summary>
        /// Do-nothing implementation of IQueryCompiler that we pass 
        /// to satisfy the base constructor of EntityQueryProvider.
        /// </summary>
        private class NullQueryCompiler : IQueryCompiler
        {
            public Func<QueryContext, IAsyncEnumerable<TResult>> CreateCompiledAsyncEnumerableQuery<TResult>(Expression query) => null;
            public Func<QueryContext, Task<TResult>> CreateCompiledAsyncTaskQuery<TResult>(Expression query) => null;
            public Func<QueryContext, TResult> CreateCompiledQuery<TResult>(Expression query) => null;
            public TResult Execute<TResult>(Expression query) => default;
            public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression query) => null;
            public Task<TResult> ExecuteAsync<TResult>(Expression query, CancellationToken cancellationToken) => Task.FromResult<TResult>(default);
        }

        public class WrappedProviderQueryable<T> : IQueryable<T>
        {
            private readonly IQueryable<T> _queryable;

            public WrappedProviderQueryable(IQueryable<T> queryable, IncludableQueryProvider provider)
            {
                _queryable = queryable;
                Provider = provider;
            }

            public Expression Expression => _queryable.Expression;
            public Type ElementType => _queryable.ElementType;
            public IQueryProvider Provider { get; }

            public IEnumerator<T> GetEnumerator() => _queryable.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
