using IntelliTect.Coalesce.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.EntityFrameworkCore.Query.Internal;
using IntelliTect.Coalesce.Mapping.IncludeTrees;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Utilities;
using System.Collections.ObjectModel;
using IntelliTect.Coalesce.Api.DataSources;
using System.Threading;

namespace IntelliTect.Coalesce
{
    /// <summary>
    /// A data source that can be used to perform mappings to custom IClassDto types using EF projections,
    /// permitting aggregations and transformations to be performed by the database with SQL where possible.
    /// </summary>
    /// <typeparam name="T">The entity type to be queried</typeparam>
    /// <typeparam name="TDto">The <see cref="IClassDto{T}"/> type to project to.</typeparam>
    /// <typeparam name="TContext">The <see cref="DbContext"/> to query from.</typeparam>
    public abstract class ProjectedDtoDataSource<T, TDto, TContext> : StandardDataSource<T, TContext>
        where T : class
        where TDto : class, IClassDto<T>, new()
        where TContext : DbContext
    {
        public ProjectedDtoDataSource(CrudContext<TContext> context) : base(context) { }

        /// <summary>
        /// <para>
        /// Apply a projection (e.g. with <see cref="Queryable.Select{TSource, TResult}(IQueryable{TSource}, System.Linq.Expressions.Expression{Func{TSource, TResult}})"/>)
        /// to the EF entity query to transform it into instances of TDto.
        /// Results will bypass <see cref="IClassDto{T}.MapFrom(T, IMappingContext, IncludeTree)"/>.
        /// </para>
        /// <para>Do not perform filtering to the query in this method, as it will not be accounted for in the total list count.</para>
        /// </summary>
        /// <param name="query">The query to filter.</param>
        /// <param name="parameters">The parameters for the data request.</param>
        /// <returns>The projected query.</returns>
        public abstract IQueryable<TDto> ApplyProjection(IQueryable<T> query, IDataSourceParameters parameters);

        /// <summary>
        /// Get a mapped single object corresponding to the given primary key.
        /// </summary>
        /// <param name="id">The primary key to find the desired item by.</param>
        /// <param name="parameters">The parameters by which to query.</param>
        /// <typeparam name="TRequestedDto">The IClassDto to map the data to. Must match type param TDto of the class.</typeparam>
        /// <returns>The desired item, mapped to type TDto.</returns>
        public override async Task<ItemResult<TRequestedDto>> GetMappedItemAsync<TRequestedDto>(object id, IDataSourceParameters parameters)
        {
            AssertTMatchesTDto<TRequestedDto>();

            var query = await GetQueryAsync(parameters);
            var canUseAsync = CanEvalQueryAsynchronously(query);
            var projectedQuery = ApplyProjection(query, parameters);
            TDto? mappedResult = await EvaluateItemQueryAsync(id, projectedQuery, canUseAsync, GetEffectiveCancellationToken(parameters));

            if (mappedResult == null)
            {
                return GetNotFoundMessage(id);
            }

            return new ItemResult<TRequestedDto>(true, mappedResult as TRequestedDto);
        }

        /// <summary>
        /// Evaluate the given query to find the single object corresponding to the given primary key.
        /// </summary>
        /// <param name="id">The primary key to find the desired item by.</param>
        /// <param name="query">The query to query.</param>
        /// <param name="canUseAsync">True if CanEvalQueryAsynchronously returned true for the underlying entity query.</param>
        /// <param name="cancellationToken">A CancellationToken to use.</param>
        /// <returns>The requested object, or null if it was not found.</returns>
        protected virtual async Task<TDto?> EvaluateItemQueryAsync(object id, IQueryable<TDto> query, bool canUseAsync, CancellationToken cancellationToken = default)
        {
            return canUseAsync
                ? await query.FindItemAsync(id, Context.ReflectionRepository, cancellationToken)
                : query.FindItem(id, Context.ReflectionRepository);
        }


        /// <summary>
        /// Get a mapped list of results using all the functionality defined in the data source.
        /// </summary>
        /// <typeparam name="TRequestedDto">The IClassDto to map the data to. Must match type param TDto of the class.</typeparam>
        /// <returns>A ListResult containing the desired data mapped to type TDto.</returns>
        public override async Task<ListResult<TRequestedDto>> GetMappedListAsync<TRequestedDto>(IListParameters parameters)
        {
            AssertTMatchesTDto<TRequestedDto>();

            var query = await GetQueryAsync(parameters);

            query = ApplyListFiltering(query, parameters);

            // Get a count. This doesn't need to account for the projection.
            int totalCount = await GetListTotalCountAsync(query, parameters);

            // Add paging, sorting only after we've gotten the total count, since they don't affect counts.
            query = ApplyListSorting(query, parameters);
            query = ApplyListPaging(query, parameters, totalCount, out int page, out int pageSize);

            var canUseAsync = CanEvalQueryAsynchronously(query);
            var projectedQuery = ApplyProjection(query, parameters);
            IList<TDto> mappedList = canUseAsync ? await projectedQuery.ToListAsync(GetEffectiveCancellationToken(parameters)) : projectedQuery.ToList();

            mappedList = TrimListFields(mappedList, parameters);

            return new ListResult<TRequestedDto>(mappedList.Cast<TRequestedDto>().ToList(), page: page, totalCount: totalCount, pageSize: pageSize);
        }

        public sealed override IncludeTree GetIncludeTree(IQueryable<T> query, IDataSourceParameters parameters)
        {
            throw new NotSupportedException($"ProjectedDtoDataSource does not utilize {nameof(IncludeTree)} - tree trimming is performed in {nameof(ApplyProjection)}");
        }

        public sealed override void TransformResults(IReadOnlyList<T> results, IDataSourceParameters parameters)
        {
            throw new NotSupportedException($"ProjectedDtoDataSource does not utilize {nameof(TransformResults)} - transformations should be performed in {nameof(ApplyProjection)}");
        }

        public sealed override Task TransformResultsAsync(IReadOnlyList<T> results, IDataSourceParameters parameters)
        {
            throw new NotSupportedException($"ProjectedDtoDataSource does not utilize {nameof(TransformResultsAsync)} - transformations should be performed in {nameof(ApplyProjection)}");
        }

        protected void AssertTMatchesTDto<TActual>()
        {
            if (typeof(TActual) != typeof(TDto))
            {
                throw new InvalidCastException($"Cannot request DTO of type {typeof(TActual).FullName} from data source for DTO of type {typeof(TDto).FullName}");
            }
        }
    }
}
