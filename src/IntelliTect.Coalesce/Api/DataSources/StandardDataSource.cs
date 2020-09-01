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

        /// <summary>
        /// A <see cref="CancellationToken"/> that can be observed to see if the underlying request has been canceled.
        /// </summary>
        public CancellationToken CancellationToken => Context.CancellationToken;

        static StandardDataSource()
        {
            // Fixes EF Core query caching issues: https://dzone.com/articles/investigating-a-memory-leak-in-entity-framework-co
            ParsingConfig.Default.UseParameterizedNamesInDynamicQuery = true;
            ParsingConfig.DefaultEFCore21.UseParameterizedNamesInDynamicQuery = true;
        }

        public StandardDataSource(CrudContext<TContext> context) : base(context)
        {
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
        /// Get the initial query that will be compounded upon with various other
        /// clauses in order to ultimately retrieve the final resulting data.
        /// </summary>
        /// <returns>The initial query.</returns>
        public virtual Task<IQueryable<T>> GetQueryAsync(IDataSourceParameters parameters) => Task.FromResult(GetQuery(parameters));

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
            // If no filter value is specified, do nothing.
            if (string.IsNullOrWhiteSpace(value))
            {
                return query;
            }

            if (prop.Type.IsDate)
            {
                // Literal string "null" should match null values if the prop is nullable.
                if (prop.Type.IsNullable && value.Trim().Equals("null", StringComparison.InvariantCultureIgnoreCase))
                {
                    return query.Where($"it.{prop.Name} = null");
                }

                // See if they just passed in a date or a date and a time
                if (DateTime.TryParse(value, out DateTime parsedValue))
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
                            return query.Where($"it.{prop.Name} >= @0 && it.{prop.Name} < @1",
                                dateTimeOffset, nextDate);
                        }
                        else
                        {
                            // Date and Time
                            return query.Where($"it.{prop.Name} = @0", dateTimeOffset);
                        }
                    }
                    else
                    {
                        if (parsedValue.TimeOfDay == TimeSpan.FromHours(0) &&
                            !value.Contains(':'))
                        {
                            // Only a date
                            var nextDate = parsedValue.AddDays(1);
                            return query.Where($"it.{prop.Name} >= @0 && it.{prop.Name} < @1",
                                parsedValue, nextDate);
                        }
                        else
                        {
                            // Date and Time
                            return query.Where($"it.{prop.Name} = @0", parsedValue);
                        }
                    }
                }
                else
                {
                    // Could not parse date string.
                    return query.Where(_ => false);
                }
            }
            else if (prop.Type.IsString)
            {
                if (value.Contains("*"))
                {
                    return query.Where($"it.{prop.Name}.StartsWith(@0)", value.Replace("*", ""));
                }
                else
                {
                    return query.Where($"it.{prop.Name} == @0", value);
                }
            }
            else
            {
                var values = value
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(item =>
                    {
                        var type = prop.Type.NullableUnderlyingType.TypeInfo;

                        // The exact value "null" should match null values exactly.
                        if (prop.Type.IsNullable && item.Trim().Equals("null", StringComparison.InvariantCultureIgnoreCase))
                        {
                            return (Success: true, Result: (object?)null);
                        }

                        if (prop.Type.IsEnum)
                        {
                            var inputIsInt = int.TryParse(item, out int intVal);

                            var enumString = prop.Type.EnumValues.SingleOrDefault(ev =>
                                inputIsInt
                                    ? ev.Key == intVal
                                    : ev.Value.Equals(item.Trim(), StringComparison.InvariantCultureIgnoreCase)
                                ).Value;

                            // If SingleOrDefault doesn't match anything,
                            // `.Value` will be default(string), which is null.
                            // This is the parse failure case.
                            if (enumString == null)
                            {
                                return (Success: false, Result: null);
                            }

                            return (Success: true, Result: enumString);
                        }

                        try
                        {
                            object result = type == typeof(Guid) 
                                ? Guid.Parse(item)
                                : Convert.ChangeType(item, type);

                            return (Success: true, Result: result);
                        }
                        catch { return (Success: false, Result: null); }
                    })
                    .Where(conversion => conversion.Success)
                    .Select((conversion, i) => (
                        Param: conversion.Result,
                        Clause: $"it.{prop.Name} == @{i}"
                    ))
                    .ToList();

                // Something was specified (since we didnt return early), but we couldn't parse it.
                // Make our query return nothing since the targeted field could never equal an 
                // unparsable value.
                if (values.Count == 0)
                {
                    return query.Where(_ => false);
                }

                return query.Where(
                    string.Join(" || ", values.Select(v => v.Clause)),
                    values.Select(v => v.Param).ToArray()
                );
            }
        }

        /// <summary>
        /// Applies a filter to the query based on the search term recieved from the client.
        /// This search term is found in parameters.Search.
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
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return query;
            }

            // See if the user has specified a field with a colon and search on that first
            if (searchTerm.Contains(":"))
            {
                var fieldValueParts = searchTerm.Split(new string[] { ":" }, StringSplitOptions.None);

                var field = fieldValueParts[0].Trim();
                var value = fieldValueParts[1].Trim();

                var prop = ClassViewModel.ClientProperties.FirstOrDefault(f =>
                    string.Equals(f.Name, field, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(f.DisplayName, field, StringComparison.OrdinalIgnoreCase));

                if (prop != null && !string.IsNullOrWhiteSpace(value))
                {
                    var expressions = prop
                        .SearchProperties(ClassViewModel, maxDepth: 1, force: true)
                        .SelectMany(p => p.GetLinqDynamicSearchStatements(Context.User, Context.TimeZone, "it", value))
                        .Select(t => t.statement)
                        .ToList();

                    // Join these together with an 'or'
                    if (expressions.Count > 0)
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
                    .SearchProperties(ClassViewModel)
                    .SelectMany(p => p.GetLinqDynamicSearchStatements(Context.User, Context.TimeZone, "it", termWord))
                    .Where(f => f.property.SearchIsSplitOnSpaces)
                    .Select(t => t.statement)
                    .ToList();

                // For the given term word, allow any of the properties (so we join clauses with OR)
                // to match the term word.
                if (splitOnStringClauses.Count > 0)
                {
                    splitOnStringTermClauses.Add("(" + string.Join(" || ", splitOnStringClauses) + ")");
                }
            }
            // Require each "word clause"
            if (splitOnStringTermClauses.Count > 0)
            {
                completeSearchClauses.Add("( " + string.Join(" && ", splitOnStringTermClauses) + " )");
            }




            // For all searchable properties where SearchIsSplitOnSpaces is false,
            // we only require that the entire search term match at least one of these properties.
            var searchClauses = ClassViewModel
                .SearchProperties(ClassViewModel)
                .SelectMany(p => p.GetLinqDynamicSearchStatements(Context.User, Context.TimeZone, "it", searchTerm))
                .Where(f => !f.property.SearchIsSplitOnSpaces)
                .Select(t => t.statement)
                .ToList();
            completeSearchClauses.AddRange(searchClauses);


            if (completeSearchClauses.Count > 0)
            {
                string finalSearchClause = string.Join(" || ", completeSearchClauses);
                return query.Where(finalSearchClause);
            }

            // A search term was specified (we didn't return early from this method), 
            // but we didn't find any valid properties to search on (we didn't come up with any clauses).
            // We should therefore return no results since the search term entered can't come up with any results.
            return query.Where(_ => false);
        }


        /// <summary>
        /// Applies all filtering that is done when getting a list of data
        /// (or metadata about a particular set of filters, like a count).
        /// This includes ApplyListPropertyFilters and ApplyListSearchTerm.
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
            if (orderByParams.Any(p => p.Key == "none"))
            {
                return query;
            }

            var clauses = orderByParams
                .Select(orderByParam =>
                {
                    string fieldName = orderByParam.Key;
                    string direction = orderByParam.Value.ToString();

                    // Validate that the field accessor is a valid property
                    // that the current user is allowed to read.
                    var parts = fieldName.Split('.');
                    PropertyViewModel? prop = null;
                    foreach (var part in parts)
                    {
                        if (prop != null && !prop.IsPOCO)
                        {
                            // We're accessing a nested prop, but the parent isn't an object,
                            // so this can't be valid.
                            return null;
                        }

                        prop = (prop?.Object ?? ClassViewModel).PropertyByName(part);

                        // Check if the new prop exists and is readable by user.
                        if (prop == null || !prop.IsClientProperty || !prop.SecurityInfo.IsReadable(User))
                        {
                            return null;
                        }

                        // If the prop is an object that isn't readable, then this is no good.
                        if (prop.IsPOCO && prop.Object?.SecurityInfo.IsReadAllowed(User) != true)
                        {
                            return null;
                        }
                    }

                    if (prop == null)
                    {
                        return null;
                    }

                    if (prop.IsPOCO)
                    {
                        // The property is a POCO, not a value.
                        // Get the default order by for the object's type to figure out what field to sort by.
                        string? clause = prop.Type.ClassViewModel?.DefaultOrderByClause($"{fieldName}.");
                        
                        // The default order by clause has an order associated, but we want to override it
                        // with the order that the client specified. A string replacement will do.
                        return clause?
                            .Replace("ASC", direction.ToUpper())
                            .Replace("DESC", direction.ToUpper());
                    }
                    else
                    {
                        // We've validated that `fieldName` is a valid acccessor for a comparable property,
                        // and that the user is allowed to read it.
                        return $"{fieldName} {direction}";
                    }
                })
                // Take all the clauses up until an invalid one is found.
                .TakeWhile(clause => clause != null)
                .ToList();

            if (clauses.Count > 0)
            {
                query = query.OrderBy(string.Join(", ", clauses));
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
            if (orderByParams.Count > 0)
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
            if (totalCount.HasValue && totalCount != -1 && (page - 1) * pageSize > totalCount)
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
            var canUseAsync = CanEvalQueryAsynchronously(query);
            return canUseAsync ? query.CountAsync(GetEffectiveCancellationToken(parameters)) : Task.FromResult(query.Count());
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
            
            var canUseAsync = CanEvalQueryAsynchronously(query);
            List<T> result = canUseAsync ? await query.ToListAsync(GetEffectiveCancellationToken(parameters)) : query.ToList();

            var tree = GetIncludeTree(query, parameters);
            return (new ListResult<T>(result, page: page, totalCount: totalCount, pageSize: pageSize), tree);
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
        protected virtual async Task<T> EvaluateItemQueryAsync(object id, IQueryable<T> query, CancellationToken cancellationToken = default)
        {
            var canUseAsync = CanEvalQueryAsynchronously(query);
            return canUseAsync
                ? await query.FindItemAsync(id, Context.ReflectionRepository, cancellationToken) 
                : query.FindItem(id, Context.ReflectionRepository);
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
