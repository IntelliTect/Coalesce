using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Data
{
    public interface IPostSavable<T, TContext> where TContext : DbContext where T : class
    {
        /// <summary>
        /// Called after the object is saved. Allows for making other changes to related objects.
        /// </summary>
        /// <param name="original">Object before being updated from DTO.</param>
        /// <param name="db">Database context.</param>
        /// <param name="user">Current user.</param>
        /// <returns>If true is returned, the object will be reloaded from the database after this call.</returns>
        void PostSave(T original, TContext db, ClaimsPrincipal user, string includes);


    }
}
