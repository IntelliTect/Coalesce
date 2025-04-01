using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;

namespace IntelliTect.Coalesce
{
    public abstract class QueryableDataSourceBase<T>
        where T : class
    {
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
        /// Contains contextual information about the request.
        /// </summary>
        public CrudContext Context { get; }

        /// <summary>
        /// The user making the request.
        /// </summary>
        public ClaimsPrincipal User => Context.User;

        /// <summary>
        /// A ClassViewModel representing the type T that is handled by these strategies.
        /// </summary>
        public ClassViewModel ClassViewModel { get; protected set; }

        public QueryableDataSourceBase(CrudContext context)
        {
            Context = context
                ?? throw new ArgumentNullException(nameof(context));

            ClassViewModel = Context.ReflectionRepository.GetClassViewModel<T>()
                ?? throw new ArgumentException("Generic type T has no ClassViewModel.", nameof(T));
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
                    && prop.SecurityInfo.IsFilterAllowed(Context))
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

            TypeViewModel propType = prop.Type;
            if (propType.IsDate)
            {
                // Literal string "null" should match null values if the prop is nullable.
                if (value.Trim().Equals("null", StringComparison.InvariantCultureIgnoreCase))
                {
                    if (propType.IsReferenceOrNullableValue)
                    {
                        return query.WhereExpression(it =>
                            Expression.Equal(it.Prop(prop), Expression.Constant(null))
                        );
                    }
                    
                    return query.Where(_ => false);
                }

                // See if they just passed in a date or a date and a time
                if (DateTime.TryParse(value, out DateTime parsedValue))
                {
                    // Correct offset.
                    Expression min, max;
                    if (propType.IsDateTimeOffset)
                    {
                        DateTimeOffset dateTimeOffset = new DateTimeOffset(parsedValue, Context.TimeZone.GetUtcOffset(parsedValue));

                        // convert to UTC, to support Postgres which doesn't allow non-zero offsets
                        dateTimeOffset = dateTimeOffset.ToUniversalTime();

                        min = dateTimeOffset.AsQueryParam(propType);
                        max = dateTimeOffset.AddDays(1).AsQueryParam(propType);
                    }
                    else
                    {
                        min = parsedValue.AsQueryParam(propType);
                        max = parsedValue.AddDays(1).AsQueryParam(propType);
                    }

                    if (parsedValue.TimeOfDay == TimeSpan.FromHours(0) &&
                        !value.Contains(':'))
                    {
                        // Only a date. Find all values on that date.
                        return query.WhereExpression(it =>
                            Expression.AndAlso(
                                Expression.GreaterThanOrEqual(it.Prop(prop), min),
                                Expression.LessThan(it.Prop(prop), max)
                            ));
                    }
                    else
                    {
                        // Date and Time
                        // The == operator doesn't work well here in memory (which our tests use),
                        // so we have to generate it as a call to .Equals.
                        return query.WhereExpression(it => min.Call("Equals", Expression.Convert(it.Prop(prop), typeof(object))));
                    }
                }
                else
                {
                    // Could not parse date string.
                    return query.Where(_ => false);
                }
            }
            else if (propType.IsString)
            {
                if (value.Contains("*"))
                {
                    return query.WhereExpression(it => it
                        .Prop(prop)
                        .Call(MethodInfos.StringStartsWith, value.Replace("*", "").AsQueryParam())
                    );
                }
                else
                {
                    return query.WhereExpression(it => 
                        Expression.Equal(it.Prop(prop), value.AsQueryParam())
                    );
                }
            }
            else
            {
                var values = value
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(item =>
                    {
                        var type = propType.NullableValueUnderlyingType.TypeInfo;

                        // The exact value "null" should match null values exactly.
                        if (item.Trim().Equals("null", StringComparison.InvariantCultureIgnoreCase))
                        {
                            if (propType.IsReferenceOrNullableValue) return (Success: true, Result: (object?)null);
                            else return (Success: false, Result: null);
                        }

                        if (propType.IsEnum)
                        {
                            var isLong = long.TryParse(item, out long longVal);
                            var integralValue = propType.EnumValues.SingleOrDefault(ev =>
                                isLong
                                    // Match user input as the enum's numeric value
                                    ? longVal.Equals(Convert.ToInt64(ev.Value))
                                    // Match user input as the enum's string value.
                                    : ev.Name.Equals(item.Trim(), StringComparison.InvariantCultureIgnoreCase)
                                )?.Value;

                            if (integralValue == null)
                            {
                                // Parsing the user input to an enum value failed
                                return (Success: false, Result: null);
                            }

                            return (Success: true, Result: Enum.ToObject(type, integralValue));
                        }

                        try
                        {
                            var typeConverter = System.ComponentModel.TypeDescriptor.GetConverter(type);
                            if (!typeConverter.IsValid(item))
                            {
                                return (Success: false, Result: null);
                            }

                            object result = type == typeof(Guid)
                                ? Guid.Parse(item)
                                : typeConverter.ConvertFromString(item)!;

                            return (Success: true, Result: result);
                        }
                        catch { return (Success: false, Result: null); }
                    })
                    .Where(conversion => conversion.Success)
                    .Select(conversion => conversion.Result)
                    .ToList();

                // Something was specified (since we didnt return early), but we couldn't parse it.
                // Make our query return nothing since the targeted field could never equal an 
                // unparsable value.
                if (values.Count == 0)
                {
                    return query.Where(_ => false);
                }

                return query.WhereExpression(it =>
                    values.Select(v => Expression.Equal(it.Prop(prop), v.AsQueryParam(prop.Type))).OrAny()
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

            // The `x` in `.Where(x => x. ...)`
            var param = Expression.Parameter(typeof(T));

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
                        .SelectMany(p => p.GetLinqSearchStatements(Context, param, value))
                        .Select(t => t.statement)
                        .ToList();

                    // Join these together with an 'or'
                    if (expressions.Count > 0)
                    {
                        var predicate = Expression.Lambda<Func<T, bool>>(expressions.OrAny(), param);
                        return query.Where(predicate);
                    }
                }
            }

            var completeSearchClauses = new List<Expression>();

            // For all searchable properties where SearchIsSplitOnSpaces is true,
            // we require that each word in the search terms yields at least one match.
            // This allows search results to become more refined as more words are typed in.
            // For example, when searching on properties (FirstName, LastName) with input "steve steverson",
            // we require that "steve" match either a first name or last name, and "steverson" match a first name or last name
            // of the same records. This will yield people named "steve steverson" or "steverson steve".
            var splitOnStringTermClauses = new List<Expression>();
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
                    .SelectMany(p => p.GetLinqSearchStatements(Context, param, termWord))
                    .Where(f => f.property.SearchIsSplitOnSpaces)
                    .Select(t => t.statement)
                    .ToList();

                // For the given term word, allow any of the properties (so we join clauses with OR)
                // to match the term word.
                if (splitOnStringClauses.Count > 0)
                {
                    splitOnStringTermClauses.Add(splitOnStringClauses.OrAny());
                }
            }
            // Require each "word clause"
            if (splitOnStringTermClauses.Count > 0)
            {
                completeSearchClauses.Add(splitOnStringTermClauses.AndAll());
            }




            // For all searchable properties where SearchIsSplitOnSpaces is false,
            // we only require that the entire search term match at least one of these properties.
            var searchClauses = ClassViewModel
                .SearchProperties(ClassViewModel)
                .SelectMany(p => p.GetLinqSearchStatements(Context, param, searchTerm))
                .Where(f => !f.property.SearchIsSplitOnSpaces)
                .Select(t => t.statement)
                .ToList();
            completeSearchClauses.AddRange(searchClauses);


            if (completeSearchClauses.Count > 0)
            {
                var predicate = Expression.Lambda<Func<T, bool>>(completeSearchClauses.OrAny(), param);
                return query.Where(predicate);
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

            return query.OrderBy(orderByParams
                .SelectMany(GetOrderInfos)
                // Take all the clauses up until an invalid one is found.
                .TakeWhile(clause => clause != null)!
            );

            IEnumerable<OrderByInformation?> GetOrderInfos(KeyValuePair<string, SortDirection> orderByParam)
            {
                string fieldName = orderByParam.Key;
                SortDirection direction = orderByParam.Value;

                var props = GetPropChain(fieldName);
                if (props == null)
                {
                    // User-provided field name is no good.
                    // We specifically yield null in order to halt and
                    // not consume any user input after an invalid ordering is found,
                    // as doing so could produce really weird orderings.
                    yield return null;
                    yield break;
                }

                if (props.Last() is { IsPOCO: true } lastProp)
                {
                    // The property is a POCO, not a value.
                    // Emit all the default orderings of that object.
                    foreach (var info in lastProp.Object!.DefaultOrderBy)
                    {
                        yield return info with 
                        {
                            Properties = [..props, ..info.Properties],
                            // Override the direction specified by the user's input.
                            SortDirection = direction 
                        };
                    }
                }
                else
                {
                    // The end of a prop chain is a value.
                    // Return it as a directly sortable property.
                    yield return new OrderByInformation()
                    {
                        Properties = props,
                        SortDirection = direction
                    };
                }
            }

            List<PropertyViewModel>? GetPropChain(string fieldName)
            {
                // Split a dotted property accessor chain into its sequence properties.
                // Return null if anything in the chain is not searchable.

                var parts = fieldName.Split('.');
                List<PropertyViewModel> props = new(parts.Length);
                PropertyViewModel? current = null;

                foreach (var part in parts)
                {
                    if (current != null && !current.IsPOCO)
                    {
                        // We're accessing a nested prop, but the parent isn't an object,
                        // so this can't be valid.
                        return null;
                    }

                    current = (current?.Object ?? ClassViewModel).PropertyByName(part);

                    // Check if the prop exists and is readable by user.
                    if (current == null || !current.IsClientProperty || !current.SecurityInfo.IsFilterAllowed(Context))
                    {
                        return null;
                    }

                    // If the prop is an object that isn't readable, then this is no good.
                    if (current.IsPOCO && current.Object?.SecurityInfo.IsReadAllowed(User) != true)
                    {
                        return null;
                    }

                    if (!current.IsDbMapped && current.Parent.IsDbMappedType)
                    {
                        // Unmapped property on a db mapped type. Won't translate to SQL.
                        return null;
                    }

                    props.Add(current);
                }
                return props;
            }
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
            return query.OrderBy(ClassViewModel.DefaultOrderBy);
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
    }
}
