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
    public class StandardDataSource<T, TContext> : StandardDataSource<T>, IStandardCrudStrategy, IEntityFrameworkDataSource<T, TContext>
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
    }
}
