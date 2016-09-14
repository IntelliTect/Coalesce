using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Helpers.IncludeTree
{
    public class IncludedSeparatelyQueryable<T, TProperty> : IQueryable<T>, IIncludedSeparatelyQueryable<T, TProperty>
    {
        private readonly IQueryable<T> _queryable;

        public IncludedSeparatelyQueryable(IQueryable<T> queryable)
        {
            _queryable = queryable;
        }

        public IncludeTree IncludeTree { get; set; }
        public Expression Expression => _queryable.Expression;
        public Type ElementType => _queryable.ElementType;
        public IQueryProvider Provider => _queryable.Provider;

        public IEnumerator<T> GetEnumerator() => _queryable.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
