using Microsoft.Data.Entity.Query;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Intellitect.ComponentModel.Data
{
    // This class was stolen directly from https://github.com/aspnet/EntityFramework/blob/dev/src/EntityFramework.Core/EntityFrameworkQueryableExtensions.cs around line 2334
    internal class IncludableQueryable<TEntity, TProperty> : IIncludableQueryable<TEntity, TProperty>, IAsyncEnumerable<TEntity>
    {
        private readonly IQueryable<TEntity> _queryable;

        public IncludableQueryable(IQueryable<TEntity> queryable)
        {
            _queryable = queryable;
        }

        public Expression Expression => _queryable.Expression;
        public Type ElementType => _queryable.ElementType;
        public IQueryProvider Provider => _queryable.Provider;

        public IEnumerator<TEntity> GetEnumerator() => _queryable.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IAsyncEnumerator<TEntity> IAsyncEnumerable<TEntity>.GetEnumerator()
            => ((IAsyncEnumerable<TEntity>)_queryable).GetEnumerator();
    }
}
