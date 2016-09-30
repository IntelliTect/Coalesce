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
    /// Consolidates IAfterSave and IBeforeSave
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TContext"></typeparam>
    public interface ISaveCustomizable<T, TContext>
        : IAfterSave<T, TContext>, IBeforeSave<T, TContext>
        where TContext : DbContext where T : class
    {
    }

    public interface IAfterSave<T, TContext> where TContext : DbContext where T : class
    {
        /// <summary>
        /// Called after the object is saved. Allows for making other changes to related objects.
        /// </summary>
        /// <param name="original">Object before being updated from DTO.</param>
        /// <param name="db">Database context.</param>
        /// <param name="user">Current user.</param>
        /// <returns>If true is returned, the object will be reloaded from the database after this call.</returns>
        void AfterSave(T original, TContext db, ClaimsPrincipal user, string includes);


    }
        /// <summary>
    /// Before a save is done, the Validate method will execute allowing feedback to the client
    /// and the save to be canceled.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IBeforeSave<T, TContext> where TContext: DbContext where T: class
    {
        /// <summary>
        /// Called after the object is mapped and before it is saved. Allows for returning validation information.
        /// </summary>
        /// <param name="original">Object before being updated from DTO.</param>
        /// <param name="db">Database context.</param>
        /// <param name="user">Current user.</param>
        /// <returns></returns>
        ValidateResult<T> BeforeSave(T original, TContext db, ClaimsPrincipal user, string includes);
        
    }
}
