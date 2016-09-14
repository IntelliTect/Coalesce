using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Helpers.IncludeTree
{
    public interface IIncludedSeparatelyQueryable<out TEntity, out TProperty> : IQueryable<TEntity>, IEnumerable<TEntity>, IEnumerable, IQueryable
    {
        IncludeTree IncludeTree { get; set; }
    }
}
