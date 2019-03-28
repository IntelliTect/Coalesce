using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;

namespace IntelliTect.Coalesce
{
    public class CrudContext
    {
        private readonly Lazy<ClaimsPrincipal> lazyUser;

        public CrudContext(Func<ClaimsPrincipal> userAccessor)
        {
            if (userAccessor == null)
            {
                throw new ArgumentNullException(nameof(userAccessor));
            }

            lazyUser = new Lazy<ClaimsPrincipal>(userAccessor, true);
        }

        public CrudContext(Func<ClaimsPrincipal> userAccessor, TimeZoneInfo timeZone)
            : this(userAccessor)
        {
            TimeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
        }

        /// <summary>
        /// The user making the request for a CRUD action.
        /// </summary>
        public ClaimsPrincipal User => lazyUser.Value;

        /// <summary>
        /// The timezone to be used when performing any actions on date inputs that lack time zone information.
        /// </summary>
        public TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Local;

        /// <summary>
        /// The ReflectionRepository that will be used to resolve a ClassViewModel for the type handled by the CRUD strategy.
        /// </summary>
        public ReflectionRepository ReflectionRepository { get; set; } = ReflectionRepository.Global;
    }

    public class CrudContext<TContext> : CrudContext
        where TContext : DbContext
    {
        public CrudContext(TContext dbContext, Func<ClaimsPrincipal> userAccessor)
            : base(userAccessor)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public CrudContext(TContext dbContext, Func<ClaimsPrincipal> userAccessor, TimeZoneInfo timeZone)
            : base(userAccessor, timeZone)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <summary>
        /// The EF DbContext that will be used as the root source of data when performing CRUD operations.
        /// </summary>
        public TContext DbContext { get; }
    }
}
