using IntelliTect.Coalesce.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using IntelliTect.Coalesce.Data;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.EntityFrameworkCore.Query.Internal;
using IntelliTect.Coalesce.Mapping.IncludeTree;
using IntelliTect.Coalesce.Interfaces;
using IntelliTect.Coalesce.Mapping;

namespace IntelliTect.Coalesce
{
    public class StandardDataSource<T, TContext> : IDataSource<T>
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
        /// Contains contextual information about the request for data being served.
        /// </summary>
        public CrudContext<TContext> Context { get; }

        /// <summary>
        /// The DbContext to be used for this request.
        /// </summary>
        public TContext Db => Context.DbContext;

        /// <summary>
        /// The user making the request for data.
        /// </summary>
        public ClaimsPrincipal User => Context.User;

        /// <summary>
        /// A ClassViewModel representing the type T that is provided by this data source.
        /// </summary>
        public ClassViewModel ClassViewModel { get; protected set; }

        public StandardDataSource(CrudContext<TContext> context)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            ClassViewModel = ReflectionRepository.Global.GetClassViewModel<T>();
        }

        /// <summary>
        /// Check if the data source may be used. This method will be called by the framework.
        /// Use this.User to obtain the current user.
        /// </summary>
        /// <returns>True if the user is authorized, otherwise false.</returns>
        public virtual (bool Authorized, string Message) IsAuthorized() => (true, null);

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
        /// Applies the "propertyName=exactValue" filters to a query.
        /// These filters may be set when making a list request, and are found in ListParameters.Filters.
        /// This is called by ApplyListFiltering when constructing a list result.
        /// </summary>
        /// <param name="query">The query to filter.</param>
        /// <returns>The new query with additional filtering applied.</returns>
        public virtual IQueryable<T> ApplyListPropertyFilters(IQueryable<T> query, IFilterParameters parameters)
        {
            // Add key value pairs where = is used.
            foreach (var clause in parameters.Filter)
            {
                var prop = ClassViewModel.PropertyByName(clause.Key);
                if (prop != null)
                {
                    query = DatabaseCompareExpression(query, prop, clause.Value);
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
        protected virtual IQueryable<T> DatabaseCompareExpression(
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
                            return query.Where(string.Format(
                                "{0} >= @0 && {0} < @1", prop.Name),
                                dateTimeOffset, nextDate);
                        }
                        else
                        {
                            // Date and Time
                            return query.Where(string.Format("{0} = @0",
                                prop.Name), dateTimeOffset);
                        }
                    }
                    else
                    {
                        if (parsedValue.TimeOfDay == TimeSpan.FromHours(0) &&
                            !value.Contains(':'))
                        {
                            // Only a date
                            var nextDate = parsedValue.AddDays(1);
                            return query.Where(string.Format(
                                "{0} >= @0 && {0} < @1", prop.Name),
                                parsedValue, nextDate);
                        }
                        else
                        {
                            // Date and Time
                            return query.Where(string.Format("{0} = @0",
                                prop.Name), parsedValue);
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
                if (value.Contains("*")) return query.Where($"{prop.Name}.StartsWith(\"{value.Replace("*", "")}\")");
                else return query.Where($"{prop.Name} = \"{value}\"");
            }
            else if (prop.Type.IsEnum)
            {
                var expressions = new List<string>();
                foreach (var valuePart in value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                {
                    expressions.Add($"{prop.Name} = \"{prop.Type.EnumValues.SingleOrDefault(ev => ev.Key == Convert.ToInt32(valuePart)).Value}\"");
                }
                return query.Where(string.Join(" || ", expressions));
            }
            else
            {
                if (value.Contains(","))
                {
                    var whereList = new List<string>();
                    foreach (var num in value.Split(','))
                    {
                        whereList.Add($"{prop.Name} = {num}");
                    }
                    return query.Where(string.Join(" or ", whereList));
                }
                else
                {
                    return query.Where(string.Format("{0} = {1}", prop.Name, value));
                }
            }
        }

        /// <summary>
        /// Applies a filter to the query based on the search term recieved from the client.
        /// This search term is found in ListParameters.Search.
        /// This is called by ApplyListFiltering when constructing a list result.
        /// </summary>
        /// <param name="query">The query to filter.</param>
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
        /// Gets a count of items returned by the given query.
        /// </summary>
        /// <param name="query">The query to retrieve the count of.</param>
        /// <returns>The count of items for the given query.</returns>
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public virtual Task<int> GetCountAsync(IQueryable<T> query)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // Async disabled because of https://github.com/aspnet/EntityFrameworkCore/issues/9038.
            // Renable once microsoft releases the fix and we upgrade our references.
            var canUseAsync = false; // query.Provider is IAsyncQueryProvider;
            return canUseAsync ? query.CountAsync() : Task.FromResult(query.Count());
        }

        /// <summary>
        /// Applies paging to the query as specified by ListParameters.Page & PageSize.
        /// This is called by GetListAsync when constructing a list result.
        /// </summary>
        /// <param name="query">The query to filter.</param>
        /// <param name="totalCount">A known total count of results for the query, for limiting the maximum page.</param>
        /// <param name="page">out: The page number that was skipped to.</param>
        /// <param name="pageSize">out: The page size that was used in paging.</param>
        /// <returns></returns>
        public virtual IQueryable<T> ApplyListPaging(IQueryable<T> query, IListParameters parameters, int? totalCount, out int page, out int pageSize)
        {
            page = parameters.Page ?? 1;
            pageSize = parameters.PageSize ?? DefaultPageSize;
            
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
        /// Examples include: Filtering out specific items, 
        /// selecting out new items with specific properties cleared and/or populated,
        /// or any other transformations that must be done post-query evaluation.
        /// </summary>
        /// <param name="results"></param>
        /// <returns></returns>
        public virtual ICollection<T> TransformResults(ICollection<T> results, IDataSourceParameters parameters) => results;

        /// <summary>
        /// For the given query, obtain the IncludeTree to be used when serializing the results of this data source.
        /// IncludeTree allows you to control the shape of the data returned to the client.
        /// </summary>
        /// <param name="query">The query that may be used to get the IncludeTree from.</param>
        /// <returns>The IncludeTree that will be used to shape the serialized DTOs.</returns>
        /// <see cref="http://coalesce.readthedocs.io/en/latest/pages/loading-and-serialization/include-tree/"/>
        public virtual IncludeTree GetIncludeTree(IQueryable<T> query, IDataSourceParameters parameters) => query.GetIncludeTree();




        /// <summary>
        /// Get an unmapped list of results using all the behaviors defined in the DataSource.
        /// </summary>
        /// <returns>A ListResult with the requested data and paging information,
        /// and an IncludeTree to be used when mapping/serializing the data.</returns>
        public virtual async Task<(ListResult<T> list, IncludeTree includeTree)> GetListAsync(IListParameters parameters)
        {
            var query = GetQuery(parameters);

            query = ApplyListFiltering(query, parameters);
            query = ApplyListSorting(query, parameters);

            // Get a count
            int totalCount = await GetCountAsync(query);

            // Add paging after we've gotten the total count.
            query = ApplyListPaging(query, parameters, totalCount, out int page, out int pageSize);

            // Async disabled because of https://github.com/aspnet/EntityFrameworkCore/issues/9038.
            // Renable once microsoft releases the fix and we upgrade our references.
            var canUseAsync = false; // query.Provider is IAsyncQueryProvider;
            ICollection<T> result = canUseAsync ? await query.ToListAsync() : query.ToList();

            result = TransformResults(result, parameters);

            var tree = GetIncludeTree(query, parameters);
            return (new ListResult<T>(result, page, totalCount, pageSize), tree);
        }

        /// <summary>
        /// Get a mapped list of results using all the behaviors defined in the data source.
        /// </summary>
        /// <typeparam name="TDto">The IClassDto to map the data to.</typeparam>
        /// <returns>A ListResult containing the desired data mapped to the desired type.</returns>
        public virtual async Task<ListResult<TDto>> GetMappedListAsync<TDto>(IListParameters parameters)
            where TDto : IClassDto<T, TDto>, new()
        {
            var (result, tree) = await GetListAsync(parameters);

            var mappingContext = new MappingContext(Context.User, parameters.Includes);
            var mappedResult = result.List.Select(obj => Mapper<T, TDto>.ObjToDtoMapper(obj, mappingContext, tree)).ToList();

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
        /// <returns>The requested item
        /// and an IncludeTree to be used when mapping/serializing the item.</returns>
        public virtual async Task<(T item, IncludeTree includeTree)> GetItemAsync(object id, IDataSourceParameters parameters)
        {
            var query = GetQuery(parameters);

            // Async disabled because of https://github.com/aspnet/EntityFrameworkCore/issues/9038.
            // Renable once microsoft releases the fix and we upgrade our references.
            var canUseAsync = false; // query.Provider is IAsyncQueryProvider;
            T result = canUseAsync ? await query.FindItemAsync(id) : query.FindItem(id);

            result = TransformResults(new[] { result }, parameters).SingleOrDefault();

            var tree = GetIncludeTree(query, parameters);
            return (result, tree);
        }

        /// <summary>
        /// Get a mapped single object corresponding to the given primary key.
        /// </summary>
        /// <param name="id">The primary key to find the desired item by.</param>
        /// <typeparam name="TDto">The IClassDto to map the data to.</typeparam>
        /// <returns>The desired item, mapped to the desired type.</returns>
        public virtual async Task<TDto> GetMappedItemAsync<TDto>(object id, IDataSourceParameters parameters)
            where TDto : IClassDto<T, TDto>, new()
        {
            var (result, tree) = await GetItemAsync(id, parameters);

            var mappingContext = new MappingContext(Context.User, parameters.Includes);
            var mappedResult = Mapper<T, TDto>.ObjToDtoMapper(result, mappingContext, tree);

            return mappedResult;
        }

        public virtual Task<int> GetCountAsync(IFilterParameters parameters)
        {
            var query = GetQuery(parameters);
            query = ApplyListFiltering(query, parameters);
            return GetCountAsync(query);
        }
    }
}
