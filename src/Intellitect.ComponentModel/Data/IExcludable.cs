using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intellitect.ComponentModel.Data
{
    /// <summary>
    /// Allows the developer to remove items from object graph before serialization. 
    /// This is the opposite counterpart to includable.
    /// </summary>
    public interface IExcludable
    {
        void Exclude(string exclude = null);
    }
}
