using IntelliTect.Coalesce.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Data
{
    /// <summary>
    /// Consolidates the IBeforeDelete and IAfterDelete
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public interface IDeleteCustomizable<TContext> :IBeforeDelete<TContext>, IAfterDelete<TContext> where TContext : DbContext
    {
    }

    public interface IBeforeDelete<TContext> where TContext : DbContext
    {
        /// <summary>
        /// In this method remove related items from the context before the save.
        /// </summary>
        /// <param name="db"></param>
        ValidateResult BeforeDelete(TContext db, ClaimsPrincipal user);
    }

    public interface IAfterDelete<TContext> where TContext : DbContext
    {
        /// <summary>
        /// In this method do any cleanup necessary after delete.
        /// </summary>
        /// <param name="db"></param>
        void AfterDelete(TContext db, ClaimsPrincipal user);
    }
}
