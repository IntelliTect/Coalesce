﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Mapping.IncludeTrees
{
    public interface IIncludedSeparatelyQueryable<out TEntity, out TProperty> : IQueryable<TEntity>
    {
    }
}
