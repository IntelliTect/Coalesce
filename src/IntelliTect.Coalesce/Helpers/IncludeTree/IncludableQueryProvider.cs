using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Helpers.IncludeTree
{
    public class IncludableQueryProvider : EntityQueryProvider
    {
        private readonly EntityQueryProvider _baseProvider;

        internal IncludeTree IncludeTree { get; } = new IncludeTree();

        public IncludableQueryProvider(EntityQueryProvider baseProvider)
            // Passing null is fine here because we override all the execute methods to use our wrapped object,
            // so the underlying class won't care if the _queryCompiler is missing.
            // IMPORTANT: Don't override the CreateQuery methods to use the baseProvider - we want the provider used there to be ourself.
            // Otherwise, the base provider would end up getting used for subsequent queries.

            // The alternative to doing this is to get the IQueryCompiler from the baseProvider with reflection,
            // which is a bit less nice, but would let us get rid of all the method overrides: 
            // base((IQueryCompiler)typeof(EntityQueryProvider).GetField("_queryCompiler", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(baseProvider))
            : base(null)
        {
            _baseProvider = baseProvider;
        }

        public override object Execute(Expression expression)
        {
            return _baseProvider.Execute(expression);
        }

        public override TResult Execute<TResult>(Expression expression)
        {
            return ((IAsyncQueryProvider)_baseProvider).Execute<TResult>(expression);
        }

        public override IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            return ((IAsyncQueryProvider)_baseProvider).ExecuteAsync<TResult>(expression);
        }

        public override Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            return ((IAsyncQueryProvider)_baseProvider).ExecuteAsync<TResult>(expression, cancellationToken);
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
            public IQueryProvider Provider { get; private set; }

            public IEnumerator<T> GetEnumerator() => _queryable.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}
