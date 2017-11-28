using IntelliTect.Coalesce.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using System.Text;

namespace IntelliTect.Coalesce
{
    public class CrudContext
    {
        public CrudContext()
        { }

        public CrudContext(ClaimsPrincipal user)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
        }

        public CrudContext(ClaimsPrincipal user, TimeZoneInfo timeZone)
            : this(user)
        {
            TimeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
        }

        public CrudContext(ClaimsPrincipal user, TimeZoneInfo timeZone, ListParameters listParameters)
            : this(user, timeZone)
        {
            ListParameters = listParameters ?? throw new ArgumentNullException(nameof(listParameters));
        }

        public CrudContext(ClaimsPrincipal user, ListParameters listParameters)
            : this(user)
        {
            ListParameters = listParameters ?? throw new ArgumentNullException(nameof(listParameters));
        }

        /// <summary>
        /// Contains all the parameters that will be used for sorting, searching, filtering,
        /// and any other transformations on the data being requested. 
        /// </summary>
        public ListParameters ListParameters { get; set; } = null;

        /// <summary>
        /// The user making the request for a CRUD action.
        /// </summary>
        public ClaimsPrincipal User { get; set; } = null;

        /// <summary>
        /// The timezone to be used when performing any actions on date inputs that lack time zone information.
        /// </summary>
        public TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Local;
    }

    public class CrudContext<TContext> : CrudContext
        where TContext : DbContext
    {
        public CrudContext(TContext dbContext, ClaimsPrincipal user)
            : base(user)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public CrudContext(TContext dbContext, ClaimsPrincipal user, TimeZoneInfo timeZone)
            : base(user, timeZone)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public CrudContext(TContext dbContext, ClaimsPrincipal user, TimeZoneInfo timeZone, ListParameters listParameters)
            : base(user, timeZone, listParameters)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public CrudContext(TContext dbContext, ClaimsPrincipal user, ListParameters listParameters)
            : base(user, listParameters)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <summary>
        /// The EF DbContext that will be used as the root source of data when performing CRUD operations.
        /// </summary>
        public TContext DbContext { get; }
    }
}
