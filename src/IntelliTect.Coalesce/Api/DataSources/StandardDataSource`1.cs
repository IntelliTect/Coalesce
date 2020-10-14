using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Mapping;
using System.Collections.ObjectModel;
using System.Threading;
using IntelliTect.Coalesce.Api;

namespace IntelliTect.Coalesce
{
    public abstract class StandardDataSource<T> : QueryableDataSourceBase<T>, IDataSource<T>, IStandardCrudStrategy
        where T : class, new()
    {
        public StandardDataSource(CrudContext context) : base(context)
        {
        }

        /// <summary>
        /// Get the initial query that will be compounded upon with various other
        /// clauses in order to ultimately retrieve the final resulting data.
        /// </summary>
        /// <returns>The initial query.</returns>
        public abstract Task<IQueryable<T>> GetQueryAsync(IDataSourceParameters parameters);

        /// <summary>
        /// A <see cref="CancellationToken"/> that can be observed to see if the underlying request has been canceled.
        /// </summary>
        public CancellationToken CancellationToken => Context.CancellationToken;

        /// <summary>
        /// Returns the cancellation token to be used to cancel running queries.
        /// Defaults to CancellationToken.None due to poor experience related to https://github.com/dotnet/efcore/issues/19526.
        /// If cancellation based on the current web request is desired, override to use <see cref="CancellationToken"/>.
        /// </summary>
        protected virtual CancellationToken GetEffectiveCancellationToken(IDataSourceParameters parameters)
        {
            return CancellationToken.None;
        }

        /// <summary>
        /// Perform a transformation of the results after the query has been evaluated.
        /// The purpose of this is for populating unmapped propertes on entities.
        /// If possible, this sort of mutation should be performed in a custom IClassDto.
        /// 
        /// DO NOT modify any database-mapped fields in this method - doing so will have adverse
        /// effects when a data source is used in an IBehaviors implementation - namely, mutations to mapped properties will be persisted.
        /// </summary>
        /// <param name="results">The items to be transformed.</param>
        /// <param name="parameters">The parameters by which to filter.</param>
        public virtual void TransformResults(IReadOnlyList<T> results, IDataSourceParameters parameters) { }

        /// <summary>
        /// Perform a transformation of the results after the query has been evaluated.
        /// The purpose of this is for populating unmapped propertes on entities.
        /// If possible, this sort of mutation should be performed in a custom IClassDto.
        /// 
        /// DO NOT modify any database-mapped fields in this method - doing so will have adverse
        /// effects when a data source is used in an IBehaviors implementation - namely, mutations to mapped properties will be persisted.
        /// </summary>
        /// <param name="results">The items to be transformed.</param>
        /// <param name="parameters">The parameters by which to filter.</param>
        public virtual Task TransformResultsAsync(IReadOnlyList<T> results, IDataSourceParameters parameters)
        {
            TransformResults(results, parameters);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Returns a new collection of DTOs with only the fields specified by the IListParameters having values.
        /// </summary>
        /// <param name="mappedResult">The items to be trimmed.</param>
        /// <param name="parameters">The parameters by which to trim.</param>
        /// <returns>The trimmed collection of DTOs.</returns>
        public virtual IList<TDto> TrimListFields<TDto>(IList<TDto> mappedResult, IListParameters parameters)
            where TDto : class, IClassDto<T>, new()
        {
            if (parameters.Fields.Count > 0)
            {
                var allDtoProps = typeof(TDto).GetProperties();
                var requestedProps = parameters.Fields
                    .Select(field => allDtoProps.FirstOrDefault(p => string.Equals(p.Name, field, StringComparison.OrdinalIgnoreCase)))
                    .Where(prop => prop != null)
                    .ToList();

                return mappedResult
                    .Select(dto =>
                    {
                        var newDto = new TDto();
                        foreach (var prop in requestedProps)
                        {
                            prop.SetValue(newDto, prop.GetValue(dto));
                        }

                        return newDto;
                    })
                    .ToList();
            }

            return mappedResult;
        }


        /// <summary>
        /// For the given query, obtain the IncludeTree to be used when serializing the results of this data source.
        /// IncludeTree allows you to control the shape of the data returned to the client.
        /// </summary>
        /// <param name="query">The query that may be used to get the IncludeTree from.</param>
        /// <param name="parameters">The parameters by which to query.</param>
        /// <returns>The IncludeTree that will be used to shape the serialized DTOs.</returns>
        /// <see href="http://coalesce.readthedocs.io/en/latest/pages/loading-and-serialization/include-tree/"/>
        public virtual IncludeTree? GetIncludeTree(IQueryable<T> query, IDataSourceParameters parameters) => query.GetIncludeTree();

        /// <summary>
        /// Evaluate the given query to determine the total count of items to report to the client.
        /// </summary>
        /// <param name="query">The filtered query from which to obtain a count.</param>
        /// <param name="parameters">The parameters by which to query.</param>
        /// <returns>The total count of items represented by the query.</returns>
        public virtual Task<int> GetListTotalCountAsync(IQueryable<T> query, IFilterParameters parameters)
        {
            return Task.FromResult(query.Count());
        }

        /// <summary>
        /// Get an unmapped list of results using all the functionality defined in the DataSource.
        /// </summary>
        /// <returns>A ListResult with the requested data and paging information,
        /// and an IncludeTree to be used when mapping/serializing the data.</returns>
        public virtual async Task<(ListResult<T> List, IncludeTree? IncludeTree)> GetListAsync(IListParameters parameters)
        {
            var query = await GetQueryAsync(parameters);

            query = ApplyListFiltering(query, parameters);

            // Get a count
            int totalCount = await GetListTotalCountAsync(query, parameters);

            // Add paging, sorting only after we've gotten the total count, since they don't affect counts.
            query = ApplyListSorting(query, parameters);
            query = ApplyListPaging(query, parameters, totalCount, out int page, out int pageSize);

            List<T> result = await EvaluateListQueryAsync(query, GetEffectiveCancellationToken(parameters));

            var tree = GetIncludeTree(query, parameters);
            return (new ListResult<T>(result, page: page, totalCount: totalCount, pageSize: pageSize), tree);
        }

        /// <summary>
        /// Evaluate the given query to obtain all its resultant objects.
        /// </summary>
        /// <param name="query">The query to evaluate.</param>
        /// <param name="cancellationToken">A CancellationToken to use.</param>
        /// <returns>The requested list.</returns>
        protected virtual Task<List<T>> EvaluateListQueryAsync(IQueryable<T> query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(query.ToList());
        }

        /// <summary>
        /// Get a mapped list of results using all the functionality defined in the data source.
        /// </summary>
        /// <typeparam name="TDto">The IClassDto to map the data to.</typeparam>
        /// <returns>A ListResult containing the desired data mapped to the desired type.</returns>
        public virtual async Task<ListResult<TDto>> GetMappedListAsync<TDto>(IListParameters parameters)
            where TDto : class, IClassDto<T>, new()
        {
            var (result, tree) = await GetListAsync(parameters);

            if (!result.WasSuccessful || result.List == null)
            {
                return new ListResult<TDto>(result);
            }

            await TransformResultsAsync(new ReadOnlyCollection<T>(result.List), parameters);

            var mappingContext = new MappingContext(Context.User, parameters.Includes);
            IList<TDto> mappedResult = result.List.Select(obj => obj.MapToDto<T, TDto>(mappingContext, tree)).ToList();
            mappedResult = TrimListFields(mappedResult, parameters);

            return new ListResult<TDto>(result, mappedResult);
        }

        /// <summary>
        /// Generate the message to be sent to the client when a specific item is not found.
        /// </summary>
        /// <param name="id">The key of the item that was requested.</param>
        public virtual string GetNotFoundMessage(object id)
        {
            return $"{ClassViewModel.DisplayName} item with ID {id} was not found.";
        }


        /// <summary>
        /// Evaluate the given query to find the single object corresponding to the given primary key.
        /// </summary>
        /// <param name="id">The primary key to find the desired item by.</param>
        /// <param name="query">The query to query.</param>
        /// <param name="cancellationToken">A CancellationToken to use.</param>
        /// <returns>The requested object, or null if it was not found.</returns>
        protected virtual Task<T> EvaluateItemQueryAsync(object id, IQueryable<T> query, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(query.FindItem(id, Context.ReflectionRepository));
        }

        /// <summary>
        /// Get an unmapped single object corresponding to the given primary key.
        /// </summary>
        /// <param name="id">The primary key to find the desired item by.</param>
        /// <param name="parameters">The parameters by which to query.</param>
        /// <returns>The requested item
        /// and an IncludeTree to be used when mapping/serializing the item.</returns>
        public virtual async Task<(ItemResult<T> Item, IncludeTree? IncludeTree)> GetItemAsync(object id, IDataSourceParameters parameters)
        {
            var query = await GetQueryAsync(parameters);
            T result = await EvaluateItemQueryAsync(id, query, GetEffectiveCancellationToken(parameters));

            if (result == null)
            {
                return (GetNotFoundMessage(id), null);
            }

            var tree = GetIncludeTree(query, parameters);
            return (new ItemResult<T>(result), tree);
        }

        /// <summary>
        /// Get a mapped single object corresponding to the given primary key.
        /// </summary>
        /// <param name="id">The primary key to find the desired item by.</param>
        /// <param name="parameters">The parameters by which to query.</param>
        /// <typeparam name="TDto">The IClassDto to map the data to.</typeparam>
        /// <returns>The desired item, mapped to the desired type.</returns>
        public virtual async Task<ItemResult<TDto>> GetMappedItemAsync<TDto>(object id, IDataSourceParameters parameters)
            where TDto : class, IClassDto<T>, new()
        {
            var (result, tree) = await GetItemAsync(id, parameters);

            if (!result.WasSuccessful || result.Object == null)
            {
                return new ItemResult<TDto>(result);
            }

            await TransformResultsAsync(Array.AsReadOnly(new[] { result.Object }), parameters);

            var mappingContext = new MappingContext(Context.User, parameters.Includes);
            var mappedResult = result.Object.MapToDto<T, TDto>(mappingContext, tree);

            return new ItemResult<TDto>(result, mappedResult);
        }

        public virtual async Task<ItemResult<int>> GetCountAsync(IFilterParameters parameters)
        {
            var query = await GetQueryAsync(parameters);
            query = ApplyListFiltering(query, parameters);

            return new ItemResult<int>(await GetListTotalCountAsync(query, parameters));
        }
    }
}
