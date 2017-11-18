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

namespace IntelliTect.Coalesce
{
    public interface IDataSource<T>
    {
    }

    public class DefaultDataSource<T, TContext> : IDataSource<T>
        where TContext : DbContext
        where T : class
    {
        public const string NoDefaultIncludesString = "none";
        public int MaxSearchTerms { get; set; } = 6;
        public int DefaultPageSize { get; set; } = 25;

        public ListParameters ListParameters { get; set; }
        public ClaimsPrincipal User { get; set; }

        public TContext Context { get; }

        public ClassViewModel ClassViewModel { get; protected set; }

        public DefaultDataSource(TContext context, ReflectionRepository reflectionRepository)
        {
            Context = context;
            ClassViewModel = reflectionRepository.GetClassViewModel<T>();
        }

        public virtual IQueryable<T> GetQuery()
        {
            IQueryable<T> query = Context.Set<T>();

            // TODO: let's reevaluate whether or not we want this behavior.
            // Maybe we want to provide an additional default data source that will 
            if (!string.Equals(ListParameters.Includes, NoDefaultIncludesString, StringComparison.InvariantCultureIgnoreCase))
            {
                query = query.IncludeChildren();
            }

            return query;
        }


        public virtual IQueryable<T> ApplyPropertyFilters(IQueryable<T> query)
        {
            // Add key value pairs where = is used.
            foreach (var clause in ListParameters.Filters)
            {
                var prop = ClassViewModel.PropertyByName(clause.Key);
                if (prop != null)
                {
                    query = DatabaseCompareExpression(query, prop, clause.Value, CurrentTimeZone);
                }
                else
                {
                    // This property was not recognized as a valid property name for this object.
                    // TODO: Do something about this.
                }
            }

            return query;
        }

        public virtual IQueryable<T> ApplyFreeformWhereClause(IQueryable<T> query)
        {
            // Because this is processed through LINQ Dynamic,
            // there's no chance for SQL injection here.
            if (!string.IsNullOrWhiteSpace(ListParameters.Where))
            {
                return query.Where(ListParameters.Where);
            }
        }

        public virtual IQueryable<T> ApplySearchTerm(IQueryable<T> query)
        {
            var searchTerm = ListParameters.Search;

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
                            .SelectMany(p => p.GetLinqDynamicSearchStatements(User, null, value))
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
                        .SelectMany(p => p.GetLinqDynamicSearchStatements(User, null, termWord))
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
                    .SelectMany(p => p.GetLinqDynamicSearchStatements(User, null, searchTerm))
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


        public virtual IQueryable<T> ApplyFiltering(IQueryable<T> query)
        {
            query = ApplyPropertyFilters(query);
            query = ApplyFreeformWhereClause(query);
            query = ApplySearchTerm(query);
            return query;
        }

        public virtual IQueryable<T> ApplySorting(IQueryable<T> query)
        {
            var orderByParams = ListParameters.OrderByList;
            if (orderByParams.Any())
            {
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
            }
            else
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
            }
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public virtual Task<int> GetCountAsync(IQueryable<T> query)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            // Async disabled because of https://github.com/aspnet/EntityFrameworkCore/issues/9038.
            // Renable once microsoft releases the fix and we upgrade our references.
            var canUseAsync = false; // query.Provider is IAsyncQueryProvider;
            return canUseAsync ? query.CountAsync() : Task.FromResult(query.Count());
        }

        public virtual IQueryable<T> ApplyPaging(IQueryable<T> query, int? totalCount)
        {
            int page = ListParameters.Page ?? 1;
            int pageSize = ListParameters.PageSize ?? DefaultPageSize;
            
            // Cap the page number at the last item
            if ((page - 1) * pageSize > totalCount)
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

        public virtual async Task<ListResult> GetListAsync()
        {
            var query = GetQuery();

            query = ApplyFiltering(query);
            query = ApplySorting(query);

            // Get a count
            int totalCount = await GetCountAsync(query);

            // Add paging.
            

            // Make the database call
            IEnumerable<T> result2;
            // Async disabled because of https://github.com/aspnet/EntityFrameworkCore/issues/9038.
            // Renable once microsoft releases the fix and we upgrade our references.
            //if (result.Provider is IAsyncQueryProvider) result2 = await result.ToListAsync();
            //else result2 = result.ToList();
            result2 = query.ToList();

            // Add external entities
            result2.IncludesExternal(listParameters.Includes);

            // Exclude certain data
            if (new T() is IExcludable)
            {
                foreach (var obj in result2)
                {
                    ((IExcludable)obj).Exclude(listParameters.Includes);
                }
            }

            // Allow for security trimming
            // TODO: This needs to be adjusted to handle paging correctly.
            var result3 = result2.Where(f => BeforeGet(f));

            var tree = result.GetIncludeTree();
            var result4 = result3.ToList().Select(obj => MapObjToDto(obj, listParameters.Includes, tree)).ToList();

            if (listParameters.FieldList.Any())
            {
                return new ListResult(result4.AsQueryable().Select("new (" + string.Join(", ", listParameters.FieldList) + ")"),
                    page, totalCount, pageSize);
            }
            return new ListResult(result4, page, totalCount, pageSize);
        }
    }
}
