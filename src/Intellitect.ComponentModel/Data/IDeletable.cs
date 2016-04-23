using Microsoft.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Intellitect.ComponentModel.Data
{
    public interface IDeletable<TContext> where TContext : DbContext
    {
        /// <summary>
        /// In this method remove related items from the context before the save.
        /// </summary>
        /// <param name="db"></param>
        void BeforeDeleteCommit(TContext db);
    }
}
