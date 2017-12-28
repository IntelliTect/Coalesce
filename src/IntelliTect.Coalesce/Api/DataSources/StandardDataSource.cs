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

namespace IntelliTect.Coalesce
{
    public class StandardDataSource<T, TContext> : StandardCrudStrategy<T, TContext>, IEntityFrameworkDataSource<T, TContext>
        where T : class, new()
        where TContext : DbContext
    {
        /// <summary>
        /// When ListParameters.Includes is this value, the default behavior of including
        /// all immediate relations of an object when making a request will be skipped.
        /// </summary>
        public const string NoDefaultIncludesString = "none";

        /// <summary>
        /// When performing searches that are split on spaces [Search(IsSplitOnSpaces = true)],
        /// this is the maximum number of "words" in the input specified by the user that will be processed.
        /// </summary>
        public int MaxSearchTerms { get; set; } = 6;
        
        /// <summary>
        /// If no page size is specified, this value will be used as the page size.
        /// </summary>
        public int DefaultPageSize { get; set; } = 25;

        /// <summary>
        /// The maximum allowable page size. Sizes larger than this will be limited to this value.
        /// </summary>
        public int MaxPageSize { get; set; } = 10_000;

        public StandardDataSource(CrudContext<TContext> context) : base(context)
        {
        }

        /// <summary>
        /// Check if the data source may be used. 
        /// This method will be called by the framework.
        /// Use this.User to obtain the current user.
        /// </summary>
        /// <returns>True if the user is authorized, otherwise false.</returns>
        public virtual (bool Authorized, string Message) IsAuthorized() => (true, null);


        /// <summary>
        /// Allows overriding of whether or not queries will run using EF Core Async methods.
        /// </summary>
        /// <param name="query"></param>
        protected virtual bool CanEvalQueryAsynchronously(IQueryable<T> query)
        {
            // Async disabled because of https://github.com/aspnet/EntityFrameworkCore/issues/9038.
            // Renable once microsoft releases the fix and we upgrade our references.
            return false; // query.Provider is IAsyncQueryProvider;
        }

        /// <summary>
        /// Get the initial query that will be compounded upon with various other
        /// clauses in order to ultimately retrieve the final resulting data.
        /// </summary>
        /// <returns></returns>
        public virtual IQueryable<T> GetQuery(IDataSourceParameters parameters)
        {
            IQueryable<T> query = Db.Set<T>();

            if (!string.Equals(parameters.Includes, NoDefaultIncludesString, StringComparison.InvariantCultureIgnoreCase))
            {
                query = query.IncludeChildren();
            }

            return query;
        }



        /// <summary>
        /// Applies the "filter.propertyName=exactValue" filters to a query.
        /// These filters may be set when making a list request, and are found in ListParameters.Filters.
        /// This is called by ApplyListFiltering when constructing a list result.
        /// </summary>
        /// <param name="query">The query to filter.</param>
        /// <param name="parameters">The parameters by which to filter.</param>
        /// <returns>The new query with additional filtering applied.</returns>
        public virtual IQueryable<T> ApplyListPropertyFilters(IQueryable<T> query, IFilterParameters parameters)
        {
            // Add key value pairs where = is used.
            foreach (var clause in parameters.Filter)
            {
                var prop = ClassViewModel.PropertyByName(clause.Key);
                if (prop != null 
                    && prop.IsClientProperty 
                    && prop.IsUrlFilterParameter
                    && prop.SecurityInfo.IsReadable(User))
                {
                    query = ApplyListPropertyFilter(query, prop, clause.Value);
                }
            }

            return query;
        }

        /// <summary>
        /// Applies a Where clause to the query that will filter the query
        /// based on the given property and value of that property.
        /// This is called by ApplyListPropertyFilters on all properties for which a filter has been specified. 
        /// </summary>
        /// <param name="query">The query to filter.</param>
        /// <param name="prop">The property to filter by.</param>
        /// <param name="value">The value to filter on.</param>
        /// <returns>The new query with additional filtering applied.</returns>
        protected virtual IQueryable<T> ApplyListPropertyFilter(
            IQueryable<T> query, PropertyViewModel prop, string value)
        {
            if (prop.Type.IsDate)
            {
                // See if they just passed in a date or a date and a time
                DateTime parsedValue;
                if (DateTime.TryParse(value, out parsedValue))
                {
                    // Correct offset.
                    if (prop.Type.IsDateTimeOffset)
                    {
                        DateTimeOffset dateTimeOffset = new DateTimeOffset(parsedValue, Context.TimeZone.GetUtcOffset(parsedValue));
                        if (dateTimeOffset.TimeOfDay == TimeSpan.FromHours(0) &&
                            !value.Contains(':'))
                        {
                            // Only a date
                            var nextDate = dateTimeOffset.AddDays(1);
                            return query.Where($"{prop.Name} >= @0 && {prop.Name} < @1",
                                dateTimeOffset, nextDate);
                        }
                        else
                        {
                            // Date and Time
                            return query.Where($"{prop.Name} = @0", dateTimeOffset);
                        }
                    }
                    else
                    {
                        if (parsedValue.TimeOfDay == TimeSpan.FromHours(0) &&
                            !value.Contains(':'))
                        {
                            // Only a date
                            var nextDate = parsedValue.AddDays(1);
                            return query.Where($"{prop.Name} >= @0 && {prop.Name} < @1",
                                parsedValue, nextDate);
                        }
                        else
                        {
                            // Date and Time
                            return query.Where($"{prop.Name} = @0", parsedValue);
                        }
                    }

                }
                else
                {
                    // Could not parse date string.
                    return null;
                }
            }
            else if (prop.Type.IsString)
            {
                if (value.Contains("*")) return query.Where($"{prop.Name}.StartsWith(@0)", value.Replace("*", ""));
                else return query.Where($"{prop.Name} == @0", value);
            }
            else if (prop.Type.IsEnum)
            {
                var values = value
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select((item, i) => (
                        Param: prop.Type.EnumValues.SingleOrDefault(ev => 
                            int.TryParse(item, out int intVal) 
                            ? ev.Key == intVal 
                            : ev.Value.Equals(item, StringComparison.InvariantCultureIgnoreCase)
                            ).Value,
                        Clause: $"{prop.Name} == @{i}"
                    ))
                    .ToList();

                return query.Where(
                    string.Join(" || ", values.Select(v => v.Clause)),
                    values.Select(v => v.Param).ToArray()
                );
            }
            else
            {
                var values = value
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select((item, i) => (
                        Param: Convert.ChangeType(item, prop.Type.TypeInfo), 
                        Clause: $"{prop.Name} == @{i}"
                    ))
                    .ToList();

                return query.Where(
                    string.Join(" || ", values.Select(v => v.Clause)),
                    values.Select(v => v.Param).ToArray()
                );
            }
        }

        /// <summary>
        /// Applies a filter to the query based on the search term recieved from the client.
        /// This search term is found in ListParameters.Search.
        /// This is called by ApplyListFiltering when constructing a list result.
        /// </summary>
        /// <param name="query">The query to filter.</param>
        /// <param name="parameters"></param>
        /// <returns>The new query with additional filtering applied.</returns>
        public virtual IQueryable<T> ApplyListSearchTerm(IQueryable<T> query, IFilterParameters parameters)
        {
            var searchTerm = parameters.Search;

            // Add general search filters.
            // These search specified fields in the class
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                // See if the user has specified a field with a colon and search on that first
                if (searchTerm.Contains(":"))
                {
                    var fieldValueParts = searchTerm.Split(new string[] { ":" }, StringSplitOptions.None);

                    var field = fieldValueParts[0].Trim();
                    var value = fieldValueParts[1].Trim();

                    var prop = ClassViewModel.ClientProperties.FirstOrDefault(f =>
                        string.Compare(f.Name, field, true) == 0 ||
                        string.Compare(f.DisplayName, field, true) == 0);

                    if (prop != null && !string.IsNullOrWhiteSpace(value))
                    {
                        var expressions = prop
                            .SearchProperties(ClassViewModel.Name, maxDepth: 1)
                            .SelectMany(p => p.GetLinqDynamicSearchStatements(Context.User, null, value))
                            .Select(t => t.statement)
                            .ToList();

                        // Join these together with an 'or'
                        if (expressions.Any())
                        {
                            string finalSearchClause = string.Join(" || ", expressions);
                            return query.Where(finalSearchClause);
                        }

                    }
                }



                var completeSearchClauses = new List<string>();

                // For all searchable properties where SearchIsSplitOnSpaces is true,
                // we require that each word in the search terms yields at least one match.
                // This allows search results to become more refined as more words are typed in.
                // For example, when searching on properties (FirstName, LastName) with input "steve steverson",
                // we require that "steve" match either a first name or last name, and "steverson" match a first name or last name
                // of the same records. This will yield people named "steve steverson" or "steverson steve".
                var splitOnStringTermClauses = new List<string>();
                var terms = searchTerm
                        .Split(new string[] { " ", "," }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(term => term.Trim())
                        .Distinct()
                        .Where(term => !string.IsNullOrWhiteSpace(term))
                        .Take(MaxSearchTerms);
                foreach (var termWord in terms)
                {
                    var splitOnStringClauses = ClassViewModel
                        .SearchProperties(ClassViewModel.Name)
                        .SelectMany(p => p.GetLinqDynamicSearchStatements(Context.User, null, termWord))
                        .Where(f => f.property.SearchIsSplitOnSpaces)
                        .Select(t => t.statement)
                        .ToList();

                    // For the given term word, allow any of the properties (so we join clauses with OR)
                    // to match the term word.
                    if (splitOnStringClauses.Any())
                        splitOnStringTermClauses.Add("(" + string.Join(" || ", splitOnStringClauses) + ")");
                }
                // Require each "word clause"
                if (splitOnStringTermClauses.Any())
                    completeSearchClauses.Add("( " + string.Join(" && ", splitOnStringTermClauses) + " )");




                // For all searchable properties where SearchIsSplitOnSpaces is false,
                // we only require that the entire search term match at least one of these properties.
                var searchClauses = ClassViewModel
                    .SearchProperties(ClassViewModel.Name)
                    .SelectMany(p => p.GetLinqDynamicSearchStatements(Context.User, null, searchTerm))
                    .Where(f => !f.property.SearchIsSplitOnSpaces)
                    .Select(t => t.statement)
                    .ToList();
                completeSearchClauses.AddRange(searchClauses);


                if (completeSearchClauses.Any())
                {
                    string finalSearchClause = string.Join(" || ", completeSearchClauses);
                    query = query.Where(finalSearchClause);
                }
            }

            // Don't put anything after the searches. The property:value search handling returns early
            // if it finds a match. If you need code down here, refactor that part.

            return query;
        }


        /// <summary>
        /// Applies all filtering that is done when getting a list of data
        /// (or metadata about a particular set of filters, like a count).
        /// This includes ApplyListPropertyFilters, ApplyListFreeformWhereClause, and ApplyListSearchTerm.
        /// This is called by GetListAsync when constructing a list result.
        /// </summary>
        /// <param name="query">The query to filter.</param>
        /// <param name="parameters">The parameters by which to filter.</param>
        /// <returns>The new query with additional filtering applied.</returns>
        public virtual IQueryable<T> ApplyListFiltering(IQueryable<T> query, IFilterParameters parameters)
        {
            query = ApplyListPropertyFilters(query, parameters);
            query = ApplyListSearchTerm(query, parameters);
            return query;
        }




        /// <summary>
        /// Applies user-specified sorting to the query.
        /// These sort parameters may be found in ListParameters.OrderByList.
        /// This is called by ApplyListSorting when constructing a list result.
        /// </summary>
        /// <param name="query">The query to sort.</param>
        /// <param name="parameters">The parameters by which to filter.</param>
        /// <returns>The new query with additional sorting applied.</returns>
        public virtual IQueryable<T> ApplyListClientSpecifiedSorting(IQueryable<T> query, IListParameters parameters)
        {
            var orderByParams = parameters.OrderByList;
            if (!orderByParams.Any(p => p.Key == "none"))
            {
                foreach (var orderByParam in orderByParams)
                {
                    string fieldName = orderByParam.Key;
                    var prop = ClassViewModel.PropertyByName(fieldName);
                    if (!fieldName.Contains(".") && prop != null && prop.IsPOCO)
                    {
                        string clause = prop.Type.ClassViewModel.DefaultOrderByClause($"{fieldName}.");
                        clause = clause.Replace("ASC", orderByParam.Value.ToUpper());
                        clause = clause.Replace("DESC", orderByParam.Value.ToUpper());
                        query = query.OrderBy(clause);
                    }
                    else
                    {
                        query = query.OrderBy(string.Join(", ", orderByParams.Select(f => $"{fieldName} {f.Value}")));
                    }
                }
            }

            return query;
        }

        /// <summary>
        /// Applies default sorting behavior to the query.
        /// These sorting behaviors are those added with [DefaultOrderBy] attributes on model properties.
        /// If no default orderings are found, an attempt will be made to sort on a property called "Name" if one exists,
        /// and finally, on the primary key of the model being requested.
        /// This is called by ApplyListSorting when constructing a list result.
        /// </summary>
        /// <param name="query">The query to sort.</param>
        /// <returns>The new query with additional sorting applied.</returns>
        public virtual IQueryable<T> ApplyListDefaultSorting(IQueryable<T> query)
        {
            // Use the DefaultOrderBy attributes if available
            var defaultOrderBy = ClassViewModel.DefaultOrderByClause();
            if (defaultOrderBy != null)
            {
                query = query.OrderBy(defaultOrderBy);
            }
            // Use the Name property if it exists.
            else if (ClassViewModel.ClientProperties.Any(f => f.Name == "Name"))
            {
                query = query.OrderBy("Name");
            }
            // Use the ID property.
            else
            {
                query = query.OrderBy(ClassViewModel.PrimaryKey.Name);
            }
            return query;
        }

        /// <summary>
        /// Applies any applicable sorting to the query.
        /// If the client has specified any sorting (found in ListParameters.OrderByList),
        /// this will delegate to ApplyListClientSpecifiedSorting.
        /// Otherwise, this will delegate to ApplyListDefaultSorting.
        /// This is called by GetListAsync when constructing a list result.
        /// </summary>
        /// <param name="query">The query to sort.</param>
        /// <param name="parameters">The parameters by which to filter and paginate.</param>
        /// <returns>The new query with additional sorting applied.</returns>
        public virtual IQueryable<T> ApplyListSorting(IQueryable<T> query, IListParameters parameters)
        {
            var orderByParams = parameters.OrderByList;
            if (orderByParams.Any())
            {
                return ApplyListClientSpecifiedSorting(query, parameters);
            }
            else
            {
                return ApplyListDefaultSorting(query);
            }
        }

        /// <summary>
        /// Applies paging to the query as specified by ListParameters.Page and PageSize.
        /// This is called by GetListAsync when constructing a list result.
        /// </summary>
        /// <param name="query">The query to filter.</param>
        /// <param name="parameters">The parameters by which to paginate.</param>
        /// <param name="totalCount">A known total count of results for the query, for limiting the maximum page.</param>
        /// <param name="page">out: The page number that was skipped to.</param>
        /// <param name="pageSize">out: The page size that was used in paging.</param>
        /// <returns></returns>
        public virtual IQueryable<T> ApplyListPaging(IQueryable<T> query, IListParameters parameters, int? totalCount, out int page, out int pageSize)
        {
            page = parameters.Page ?? 1;
            pageSize = parameters.PageSize ?? DefaultPageSize;
            pageSize = Math.Min(pageSize, MaxPageSize);
            pageSize = Math.Max(pageSize, 1);
            
            // Cap the page number at the last item
            if (totalCount.HasValue && (page - 1) * pageSize > totalCount)
            {
                page = (int)((totalCount - 1) / pageSize) + 1;
            }

            if (page > 1)
            {
                query = query.Skip((page - 1) * pageSize);
            }

            query = query.Take(pageSize);
            return query;
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
        /// For the given query, obtain the IncludeTree to be used when serializing the results of this data source.
        /// IncludeTree allows you to control the shape of the data returned to the client.
        /// </summary>
        /// <param name="query">The query that may be used to get the IncludeTree from.</param>
        /// <param name="parameters">The parameters by which to query.</param>
        /// <returns>The IncludeTree that will be used to shape the serialized DTOs.</returns>
        /// <see href="http://coalesce.readthedocs.io/en/latest/pages/loading-and-serialization/include-tree/"/>
        public virtual IncludeTree GetIncludeTree(IQueryable<T> query, IDataSourceParameters parameters) => query.GetIncludeTree();

        /// <summary>
        /// Evaluate the given query to determine the total count of items to report to the client.
        /// </summary>
        /// <param name="query">The filtered query from which to obtain a count.</param>
        /// <param name="parameters">The parameters by which to query.</param>
        /// <returns>The total count of items represented by the query.</returns>
        public virtual Task<int> GetListTotalCountAsync(IQueryable<T> query, IFilterParameters parameters)
        {
            var canUseAsync = CanEvalQueryAsynchronously(query);
            return canUseAsync ? query.CountAsync() : Task.FromResult(query.Count());
        }


        /// <summary>
        /// Get an unmapped list of results using all the behaviors defined in the DataSource.
        /// </summary>
        /// <returns>A ListResult with the requested data and paging information,
        /// and an IncludeTree to be used when mapping/serializing the data.</returns>
        public virtual async Task<(ListResult<T> List, IncludeTree IncludeTree)> GetListAsync(IListParameters parameters)
        {
            var query = GetQuery(parameters);

            query = ApplyListFiltering(query, parameters);
            query = ApplyListSorting(query, parameters);

            // Get a count
            int totalCount = await GetListTotalCountAsync(query, parameters);

            // Add paging after we've gotten the total count.
            query = ApplyListPaging(query, parameters, totalCount, out int page, out int pageSize);
            
            var canUseAsync = CanEvalQueryAsynchronously(query);
            List<T> result = canUseAsync ? await query.ToListAsync() : query.ToList();

            var tree = GetIncludeTree(query, parameters);
            return (new ListResult<T>(result, page, totalCount, pageSize), tree);
        }

        /// <summary>
        /// Get a mapped list of results using all the behaviors defined in the data source.
        /// </summary>
        /// <typeparam name="TDto">The IClassDto to map the data to.</typeparam>
        /// <returns>A ListResult containing the desired data mapped to the desired type.</returns>
        public virtual async Task<ListResult<TDto>> GetMappedListAsync<TDto>(IListParameters parameters)
            where TDto : IClassDto<T>, new()
        {
            var (result, tree) = await GetListAsync(parameters);

            TransformResults(new ReadOnlyCollection<T>(result.List), parameters);

            var mappingContext = new MappingContext(Context.User, parameters.Includes);
            var mappedResult = result.List.Select(obj => Mapper.MapToDto<T, TDto>(obj, mappingContext, tree)).ToList();

            if (parameters.Fields.Any())
            {
                var allDtoProps = typeof(TDto).GetProperties();
                var requestedProps = parameters.Fields
                    .Select(field => allDtoProps.FirstOrDefault(p => string.Equals(p.Name, field, StringComparison.InvariantCultureIgnoreCase)))
                    .Where(prop => prop != null)
                    .ToList();

                mappedResult = mappedResult
                    .Select(dto =>
                    {
                        var newDto = new TDto();
                        foreach (var prop in requestedProps) prop.SetValue(newDto, prop.GetValue(dto));
                        return newDto;
                    })
                    .ToList();
            }

            return new ListResult<TDto>(mappedResult, result.Page, result.TotalCount, result.PageSize);
        }

        /// <summary>
        /// Get an unmapped single object corresponding to the given primary key.
        /// </summary>
        /// <param name="id">The primary key to find the desired item by.</param>
        /// <param name="parameters">The parameters by which to query.</param>
        /// <returns>The requested item
        /// and an IncludeTree to be used when mapping/serializing the item.</returns>
        public virtual async Task<(ItemResult<T> Item, IncludeTree IncludeTree)> GetItemAsync(object id, IDataSourceParameters parameters)
        {
            var query = GetQuery(parameters);

            var canUseAsync = CanEvalQueryAsynchronously(query);
            T result = canUseAsync ? await query.FindItemAsync(id) : query.FindItem(id);

            if (result == null)
            {
                return ($"{ClassViewModel.DisplayName} item with ID {id} was not found.", null);
            }

            var tree = GetIncludeTree(query, parameters);
            return (new ItemResult<T>(true, result), tree);
        }

        /// <summary>
        /// Get a mapped single object corresponding to the given primary key.
        /// </summary>
        /// <param name="id">The primary key to find the desired item by.</param>
        /// <param name="parameters">The parameters by which to query.</param>
        /// <typeparam name="TDto">The IClassDto to map the data to.</typeparam>
        /// <returns>The desired item, mapped to the desired type.</returns>
        public virtual async Task<ItemResult<TDto>> GetMappedItemAsync<TDto>(object id, IDataSourceParameters parameters)
            where TDto : IClassDto<T>, new()
        {
            var (result, tree) = await GetItemAsync(id, parameters);

            if (!result.WasSuccessful || result.Object == null)
            {
                return new ItemResult<TDto>(result);
            }

            TransformResults(Array.AsReadOnly(new[] { result.Object }), parameters);

            var mappingContext = new MappingContext(Context.User, parameters.Includes);
            var mappedResult = Mapper.MapToDto<T, TDto>(result.Object, mappingContext, tree);

            return new ItemResult<TDto>(true, mappedResult);
        }

        public virtual Task<int> GetCountAsync(IFilterParameters parameters)
        {
            var query = GetQuery(parameters);
            query = ApplyListFiltering(query, parameters);

            return GetListTotalCountAsync(query, parameters);
        }
    }
}
