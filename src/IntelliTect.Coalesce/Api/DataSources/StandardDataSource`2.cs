using IntelliTect.Coalesce.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Api;
using System.Collections.ObjectModel;
using System.Threading;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Security.Claims;

namespace IntelliTect.Coalesce
{
    public class StandardDataSource<T, TContext> : StandardDataSource<T>, IEntityFrameworkDataSource<T, TContext>
        where T : class, new()
        where TContext : DbContext
    {
        /// <summary>
        /// Contains contextual information about the request.
        /// </summary>
        public new CrudContext<TContext> Context => (CrudContext<TContext>)base.Context;

        /// <summary>
        /// The DbContext from the <see cref="CrudContext{TContext}"/> for the data source.
        /// </summary>
        public TContext Db => Context.DbContext;

        /// <summary>
        /// When ListParameters.Includes is this value, the default behavior of including
        /// all immediate relations of an object when making a request will be skipped.
        /// </summary>
        public const string NoDefaultIncludesString = "none";

        public StandardDataSource(CrudContext<TContext> context) : base(context)
        {
            // Ensure that the DbContext is in the ReflectionRepository.
            // We do this so that unit tests will work without having to always do this manually.
            // Cost is very low.
            Context.ReflectionRepository.GetOrAddType(typeof(TContext));
        }

        /// <summary>
        /// Get the initial query that will be compounded upon with various other
        /// clauses in order to ultimately retrieve the final resulting data.
        /// </summary>
        /// <returns>The initial query.</returns>
        public override Task<IQueryable<T>> GetQueryAsync(IDataSourceParameters parameters) => Task.FromResult(GetQuery(parameters));

        /// <summary>
        /// Get the initial query that will be compounded upon with various other
        /// clauses in order to ultimately retrieve the final resulting data.
        /// </summary>
        /// <returns>The initial query.</returns>
        public virtual IQueryable<T> GetQuery(IDataSourceParameters parameters)
        {
            IQueryable<T> query = Db.Set<T>();

            if (!string.Equals(parameters.Includes, NoDefaultIncludesString, StringComparison.OrdinalIgnoreCase))
            {
                query = query.IncludeChildren(this.Context.ReflectionRepository);
            }

            return query;
        }

        /// <summary>
        /// Allows overriding of whether or not queries will run using EF Core Async methods.
        /// </summary>
        /// <param name="query"></param>
        protected virtual bool CanEvalQueryAsynchronously(IQueryable<T> query)
        {
            // Do not use a straight " is IAsyncQueryProvider " check,
            // as this type changed namespace in EF 5 and so cannot be compatible
            // with both EF 2 and EF 5 at the same time.

            return query.Provider.GetType().GetInterface("IAsyncQueryProvider") != null;
        }

        public override Task<int> GetListTotalCountAsync(IQueryable<T> query, IFilterParameters parameters)
        {
            var canUseAsync = CanEvalQueryAsynchronously(query);
            return canUseAsync ? query.CountAsync(GetEffectiveCancellationToken(parameters)) : Task.FromResult(query.Count());
        }

        protected override Task<T> EvaluateItemQueryAsync(object id, IQueryable<T> query, CancellationToken cancellationToken = default)
        {
            var canUseAsync = CanEvalQueryAsynchronously(query);
            return canUseAsync
                ? query.FindItemAsync(id, Context.ReflectionRepository, cancellationToken)
                : Task.FromResult(query.FindItem(id, Context.ReflectionRepository));
        }

        protected override Task<List<T>> EvaluateListQueryAsync(IQueryable<T> query, CancellationToken cancellationToken = default)
        {
            var canUseAsync = CanEvalQueryAsynchronously(query);
            return canUseAsync ? query.ToListAsync(cancellationToken) : Task.FromResult(query.ToList());
        }
    }
}
