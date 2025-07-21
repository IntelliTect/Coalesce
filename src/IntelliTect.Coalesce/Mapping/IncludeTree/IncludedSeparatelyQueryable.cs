using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace IntelliTect.Coalesce.Mapping.IncludeTrees;

public class IncludedSeparatelyQueryable<T, TProperty> : IIncludedSeparatelyQueryable<T, TProperty>
{
    private readonly IQueryable<T> _queryable;

    public IncludedSeparatelyQueryable(IQueryable<T> queryable)
    {
        _queryable = queryable;
    }

    public Expression Expression => _queryable.Expression;
    public Type ElementType => _queryable.ElementType;
    public IQueryProvider Provider => _queryable.Provider;

    public IEnumerator<T> GetEnumerator() => _queryable.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
