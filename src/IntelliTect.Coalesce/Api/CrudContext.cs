using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading;

namespace IntelliTect.Coalesce
{
    public class CrudContext
    {

        public CrudContext(ClaimsPrincipal user, CancellationToken cancellationToken)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            CancellationToken = cancellationToken;
        }

        public CrudContext(ClaimsPrincipal user, TimeZoneInfo timeZone, CancellationToken cancellationToken)
            : this(user, cancellationToken)
        {
            TimeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
        }

        /// <summary>
        /// The user making the request for a CRUD action.
        /// </summary>
        public ClaimsPrincipal User { get; }

        public CancellationToken CancellationToken { get; }

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
        public CrudContext(TContext dbContext, ClaimsPrincipal user, CancellationToken cancellationToken)
            : base(user, cancellationToken)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public CrudContext(TContext dbContext, ClaimsPrincipal user, TimeZoneInfo timeZone, CancellationToken cancellationToken)
            : base(user, timeZone, cancellationToken)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <summary>
        /// The EF DbContext that will be used as the root source of data when performing CRUD operations.
        /// </summary>
        public TContext DbContext { get; }
    }
}
