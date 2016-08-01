using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Data
{
    /// <summary>
    /// Allows the developer to specify additional .include(f=>f.xxx) to group whenever this type is returned
    /// as the primary type. If on 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IIncludable<T>
    {
        IQueryable<T> Include(IQueryable<T> entities, string include = null);
    }

    public interface IIncludableWithDbContext<T>
    {
        IQueryable<T> Include(IQueryable<T> entities, DbContext context = null, string include = null);
    }
}
