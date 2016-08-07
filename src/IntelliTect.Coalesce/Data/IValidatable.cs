using IntelliTect.Coalesce.Interfaces;
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
    /// Before a save is done, the Validate method will execute allowing feedback to the client
    /// and the save to be canceled.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IValidatable<T, TContext> where TContext: DbContext where T: class
    {
        /// <summary>
        /// Called after the object is mapped and before it is saved. Allows for returning validation information.
        /// </summary>
        /// <param name="updated">Values to be saved.</param>
        /// <param name="db">Database context.</param>
        /// <param name="user">Current user.</param>
        /// <returns></returns>
        ValidateResult<T> Validate(T original, TContext db, ClaimsPrincipal user);
        
    }
}
