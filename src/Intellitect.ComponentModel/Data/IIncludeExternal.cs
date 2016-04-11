using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intellitect.ComponentModel.Data
{
    public interface IIncludeExternal<T>
    {
        IEnumerable<T> IncludeExternal(IEnumerable<T> entities, string include = null);
    }
}
