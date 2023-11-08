using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Threading;

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
            Options = new();
        }

        public CrudContext(
            Func<ClaimsPrincipal> userAccessor, 
            TimeZoneInfo timeZone,
            CancellationToken cancellationToken = default,
            CoalesceOptions? coalesceOptions = null,
            IServiceProvider? serviceProvider = null
        )
            : this(userAccessor)
        {
            TimeZone = timeZone ?? throw new ArgumentNullException(nameof(timeZone));
            CancellationToken = cancellationToken;
            Options = coalesceOptions ?? Options;
            ServiceProvider = serviceProvider;
        }

        internal CrudContext(CrudContext baseContext)
        {
            if (baseContext == null)
            {
                throw new ArgumentNullException(nameof(baseContext));
            }

            lazyUser = baseContext.lazyUser;
            TimeZone = baseContext.TimeZone;
            CancellationToken = baseContext.CancellationToken;
            Options = baseContext.Options;
            ServiceProvider = baseContext.ServiceProvider;
        }


        /// <summary>
        /// A shared mapping context instance for this operation.
        /// Use intended for handling inputs before any mutations (if any)
        /// have occurred that might invalidate local state on cached instances
        /// of IPropertyRestriction.
        /// </summary>
        internal MappingContext MappingContext => _mappingContext ??= new MappingContext(this);
        private MappingContext? _mappingContext;

        /// <summary>
        /// The user making the request for a CRUD action.
        /// </summary>
        public ClaimsPrincipal User => lazyUser.Value ?? new ClaimsPrincipal();

        /// <summary>
        /// The timezone to be used when performing any actions on date inputs that lack time zone information.
        /// </summary>
        public TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Local;

        /// <summary>
        /// The ReflectionRepository that will be used to resolve a ClassViewModel for the type handled by the CRUD strategy.
        /// </summary>
        public ReflectionRepository ReflectionRepository { get; set; } = ReflectionRepository.Global;

        /// <summary>
        /// A <see cref="CancellationToken"/> that can be observed to see if the underlying request has been canceled.
        /// </summary>
        public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

        /// <summary>
        /// The application's configured <see cref="Options"/>.
        /// </summary>
        public CoalesceOptions Options { get; set; }

        /// <summary>
        /// This service provider is used to provide services to <see cref="ValidationContext"/> during validation.
        /// It is internal because we don't want people seeing it and using service locator in their
        /// custom datasources/behaviors instead of normal dependency injection.
        /// </summary>
        internal IServiceProvider? ServiceProvider { get; set; }
    }

    public class CrudContext<TContext> : CrudContext
        where TContext : DbContext
    {
        public CrudContext(TContext dbContext, Func<ClaimsPrincipal> userAccessor)
            : base(userAccessor)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public CrudContext(
            TContext dbContext, 
            Func<ClaimsPrincipal> userAccessor, 
            TimeZoneInfo timeZone,
            CancellationToken cancellationToken = default
        )
            : base(userAccessor, timeZone, cancellationToken)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        internal CrudContext(
            CrudContext baseContext,
            TContext dbContext
        )
            : base(baseContext)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        /// <summary>
        /// The EF DbContext that will be used as the root source of data when performing CRUD operations.
        /// </summary>
        public TContext DbContext { get; }
    }
}
