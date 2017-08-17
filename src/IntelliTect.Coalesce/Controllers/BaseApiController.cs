using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic;
using System.Threading.Tasks;
using IntelliTect.Coalesce.Data;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Interfaces;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.Extensions.DependencyInjection;
using IntelliTect.Coalesce.Helpers.IncludeTree;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Linq.Expressions;
using System.Net;
using System.Diagnostics.CodeAnalysis;
using IntelliTect.Coalesce.Helpers;
using IntelliTect.Coalesce.Utilities;
using System.Globalization;

namespace IntelliTect.Coalesce.Controllers
{
    public abstract class BaseApiController<T, TDto, TContext> : BaseControllerWithDb<TContext>
    where T : class, new()
    where TDto : class, IClassDto<T, TDto>, new()
    where TContext : DbContext
    {
        protected BaseApiController()
        {
            // Set up a ViewModel so we can check out this object.
            ClassViewModel = ReflectionRepository.GetClassViewModel<T>();
            if (typeof(T) == typeof(TDto) || typeof(TDto).Name.EndsWith("DtoGen"))
            {
                DtoViewModel = ClassViewModel;
            }
            else
            {
                DtoViewModel = ReflectionRepository.GetClassViewModel<TDto>();
            }

        }

        protected DbSet<T> _dataSource;
        protected IQueryable<T> _readOnlyDataSource;
        protected readonly ClassViewModel ClassViewModel;
        protected readonly ClassViewModel DtoViewModel;

        protected ILogger Logger
        {
            get
            {
                if (_Logger == null)
                {
                    _Logger = HttpContext?.RequestServices.GetService<Logger<BaseApiController<T, TDto, TContext>>>();
                }

                return _Logger;
            }
        }
        private ILogger _Logger = null;

        public static int DefaultPageSizeAll { get; set; } = 25;

        private int? _defaultPageSize = null;
        public int DefaultPageSize { get { return _defaultPageSize ?? DefaultPageSizeAll; } set { _defaultPageSize = value; } }
        public int MaxSearchTerms { get; set; } = 6;

        private static TimeZoneInfo _timeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");
        public static TimeZoneInfo CurrentTimeZone
        {
            get
            {
                return _timeZone;
            }
            set
            {
                _timeZone = value;
            }
        }

        protected virtual IQueryable<T> GetDataSource(ListParameters listParameters)
        {
            if (!string.IsNullOrWhiteSpace(listParameters.DataSource) && listParameters.DataSource != "Default")
            {
                // find the IQueryable if we can
                var method = typeof(T).GetMethod(listParameters.DataSource);
                if (method != null)
                {
                    return (IQueryable<T>)method.Invoke(null, new object[] { Db, User });
                }
            }
            return DataSource ?? ReadOnlyDataSource;
        }

        protected async Task<ListResult> ListImplementation(ListParameters listParameters)
        {
            try
            {
                IQueryable<T> result = GetDataSource(listParameters);

                // Add the Include statements to the result to grab the right object graph. Passing "" gets the standard set.
                if ((result is DbSet<T>) && string.Compare(listParameters.Includes, "none", StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    result = result.Includes(listParameters.Includes);
                }

                // Add filters for the where clause, etc.
                result = AddFilters(result, listParameters);

                // Change to a list so we can sort on other fields not just what is in the database if we need to.
                // TODO: Make this more flexible to allow for searching on computed terms. This could be automatic by detecting fields that are in the database.
                // IEnumerable<T> result2 = result;
                // if (listParameters.ToList) result2 = result.ToList() as IQueryable<T>;
                // Above was removed because IEnumerable uses different extension methods than iQueryable causing very inefficient SQL to be used.

                // Add sorting.
                var orderByParams = listParameters.OrderByList;
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
                                result = result.OrderBy(clause);
                            }
                            else
                            {
                                result = result.OrderBy(string.Join(", ", orderByParams.Select(f => $"{fieldName} {f.Value}")));
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
                        result = result.OrderBy(defaultOrderBy);
                    }
                    // Use the Name property if it exists.
                    else if (ClassViewModel.Properties.Any(f => f.Name == "Name"))
                    {
                        result = result.OrderBy("Name");
                    }
                    // Use the ID property.
                    else
                    {
                        result = result.OrderBy(ClassViewModel.PrimaryKey.Name);
                    }
                }

                // Get a count
                int totalCount;
                if (result is IAsyncQueryProvider) totalCount = await result.CountAsync();
                else totalCount = result.Count();


                // Add paging.
                int page = listParameters.Page ?? 1;
                int pageSize = listParameters.PageSize ?? DefaultPageSize;
                // Fix the page numbers if necessary
                if ((page - 1) * pageSize > totalCount)
                {
                    page = (int)((totalCount - 1) / pageSize) + 1;
                }
                // Skip zero has issues.
                // Due to a bug in both RC1 (fails in SQL 2008) and RC2 (doesn't get all included children) 
                if (page > 1)
                {
                    result = result.Skip((page - 1) * pageSize);
                }
                result = result.Take(pageSize);

                // Make the database call
                IEnumerable<T> result2;
                if (result is IAsyncQueryProvider) result2 = await result.ToListAsync();
                else result2 = result.ToList();

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
                IEnumerable<TDto> result4 = result3.ToList().Select(obj => MapObjToDto(obj, listParameters.Includes, tree)).ToList();

                if (listParameters.FieldList.Any())
                {
                    return new ListResult(result4.ToList().Select("new (" + string.Join(", ", listParameters.FieldList) + ")"),
                        page, totalCount, pageSize);
                }
                return new ListResult(result4, page, totalCount, pageSize);
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                if (Logger != null)
                {
                    Logger.LogError(ex.Message, ex);
                }
                return new ListResult(ex);
            }
        }


        protected async Task<int> CountImplementation(ListParameters listParameters)
        {
            try
            {
                IQueryable<T> result = GetDataSource(listParameters);

                result = AddFilters(result, listParameters);

                int count;
                if (result is IAsyncQueryProvider) count = await result.CountAsync();
                else count = result.Count();

                return count;
            }
            catch (Exception ex)
            {
                //TODO: Log this error.
                Console.WriteLine(ex.Message);
                Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                //TODO: Hide this error from the client.
                throw ex;
            }
        }


        protected IQueryable<T> AddFilters(IQueryable<T> result, ListParameters listParameters)
        {
            // Add key value pairs where = is used.
            foreach (var clause in listParameters.Filters)
            {
                var prop = ClassViewModel.PropertyByName(clause.Key);
                if (prop != null)
                {
                    result = DatabaseCompareExpression(result, prop, clause.Value, CurrentTimeZone);
                }
                else
                {
                    // This property was not recognized as a valid property name for this object.
                    // TODO: Do something about this.
                }
            }

            // Add more free form filters.
            // Because this is processed through LINQ Dynamic,
            // there's no chance for SQL injection here.
            if (!string.IsNullOrWhiteSpace(listParameters.Where))
            {
                result = result.Where(listParameters.Where);
            }

            // Add general search filters.
            // These search specified fields in the class
            if (!string.IsNullOrWhiteSpace(listParameters.Search))
            {
                // See if the user has specified a field with a colon and search on that first
                if (listParameters.Search.Contains(":"))
                {
                    var fieldValueParts = listParameters.Search.Split(new string[] { ":" }, StringSplitOptions.None);

                    var field = fieldValueParts[0].Trim();
                    var value = fieldValueParts[1].Trim();

                    var prop = ClassViewModel.Properties.FirstOrDefault(f => 
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
                            return result.Where(finalSearchClause);
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
                var terms = listParameters.Search
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
                    .SelectMany(p => p.GetLinqDynamicSearchStatements(User, null, listParameters.Search))
                    .Where(f => !f.property.SearchIsSplitOnSpaces)
                    .Select(t => t.statement)
                    .ToList();
                completeSearchClauses.AddRange(searchClauses);
                

                if (completeSearchClauses.Any())
                {
                    string finalSearchClause = string.Join(" || ", completeSearchClauses);
                    result = result.Where(finalSearchClause);
                }
            }

            // Don't put anything after the searches. The property:value search handling returns early
            // if it finds a match. If you need code down here, refactor that part.

            return result;
        }

        /// <summary>
        /// Returns the list of strings in a property so we can provide a list
        /// </summary>
        /// <param name="property"></param>
        /// <param name="page"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        protected IEnumerable<string> PropertyValuesImplementation(string property, int page = 1, string search = "")
        {
            var originalProp = ClassViewModel.PropertyByName(property);
            if (originalProp != null)
            {
                if (!originalProp.SecurityInfo.IsReadable(User)) throw new AccessViolationException($"{property} is not accessible by current user.");

                List<PropertyViewModel> properties = new List<PropertyViewModel>();
                if (originalProp.ListGroup != null)
                {
                    properties.AddRange(ClassViewModel.Properties.Where(f => f.ListGroup == originalProp.ListGroup));
                }
                else
                {
                    properties.Add(originalProp);
                }

                List<string> result = new List<string>();
                foreach (var prop in properties)
                {
                    IQueryable<T> matches = DataSource;
                    matches = matches.Where(string.Format("{0} <> null", prop.Name));
                    if (!string.IsNullOrWhiteSpace(search))
                    {
                        matches = matches.Where(string.Format("{0}.StartsWith(\"{1}\")", prop.Name, search));
                    }
                    var first20 = matches.GroupBy(prop.Name, "it").OrderBy("it.Key").Select("it.Key").Skip(20 * (page - 1)).Take(20);
                    var asString = new List<string>();
                    foreach (var obj in first20) { asString.Add(obj.ToString()); }
                    result.AddRange(asString);
                    result = result.Distinct().ToList();
                    // Bail out if we already have 20 or more items.
                    if (result.Count >= 20) break;
                }
                return result.OrderBy(f => f);
            }
            else
            {
                return new List<string>();
            }
        }

        protected async Task<TDto> GetImplementation(string id, ListParameters listParameters)
        {
            var tuple = await GetUnmapped(id, listParameters);
            var item = tuple.Item1;
            IncludeTree tree = tuple.Item2;
            if (item == null) return null;

            // Map to DTO
            var dto = MapObjToDto(item, listParameters.Includes, tree);

            return dto;
        }

        [SuppressMessage("Async method lacks 'await' operators", "CS1998", Justification = "EF Core 1.0 is slower with async: https://github.com/aspnet/EntityFramework/issues/5816")]
        private async Task<Tuple<T, IncludeTree>> GetUnmapped(string id, ListParameters listParameters)
        {
            // This isn't a list, but the logic is the same regardless for grabbing the data source for grabbing a single object.
            IQueryable<T> source = GetDataSource(listParameters);

            // Get the item and get external data.
            if ((source is DbSet<T>) && string.Compare(listParameters.Includes, "none", StringComparison.InvariantCultureIgnoreCase) != 0)
            {
                source = source.Includes(listParameters.Includes);
            }

            var item = (await source.FindItemAsync(id)).IncludeExternal(listParameters.Includes);

            var tree = source.GetIncludeTree();

            if (!BeforeGet(item))
            {
                return null;
            }

            return new Tuple<T, IncludeTree>(item, tree);
        }

        protected virtual bool BeforeGet(T obj)
        {
            return true;
        }

        protected bool DeleteImplementation(string id)
        {
            T item = DataSource.FindItem(id);
            if (item != null)
            {
                if (BeforeDelete(item))
                {
                    // Allow for other cascade deletes.
                    var validationInfo = new ValidateResult();
                    if (item is IBeforeDelete<TContext>)
                    {
                        validationInfo.Merge((item as IBeforeDelete<TContext>).BeforeDelete(Db, User));
                    }
                    if (validationInfo.WasSuccessful)
                    {
                        DataSource.Remove(item);
                        Db.SaveChanges();
                        if (item is IAfterDelete<TContext>)
                        {
                            (item as IAfterDelete<TContext>).AfterDelete(Db, User);
                        }
                        return AfterDelete(item, Db);
                    }
                    else
                    {
                        // TODO: Fix delete to actually return good information rather than a stupid bool.
                        throw new Exception($"Delete failed: {validationInfo.Message}", null);
                    }
                }
            }
            Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
            return false;
        }

        protected async Task<SaveResult<TDto>> SaveImplementation(TDto dto, string includes = null, string dataSource = null, bool returnObject = true)
        {
            ListParameters listParams = new ListParameters(includes: includes, dataSource: dataSource);

            var result = new SaveResult<TDto>();

            // See if this is new or an update using the key.
            T item = null;

            object idValue = IdValue(dto);

            if (idValue is int && (int)idValue != 0 || idValue is string && (string)idValue != "")
            {
                item = await DataSource.FindItemAsync(idValue);
                if (item == null)
                {
                    result.WasSuccessful = false;
                    result.Message =
                        string.Format("Item with {0} = {1} not found.", ClassViewModel.PrimaryKey.Name, IdValue(dto));
                    Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
                    return result;
                }
            }

            // See if we found it.
            if (item == null)
            {
                item = new T();  // This does not work with Lazy Loading because it gives the POCO not the proxy object.
                DataSource.Add(item);
            }

            // Convert all DateTimeOffsets to the correct Time Zone.
            foreach (var prop in DtoViewModel.Properties.Where(f => f.Type.IsDateTimeOffset))
            {
                var typeProperty = dto.GetType().GetProperty(prop.Name);
                // Make sure the property exists. TODO: Check this out for base classes.
                if (typeProperty != null)
                {
                    DateTimeOffset? value = (DateTimeOffset?)typeProperty.GetValue(dto);
                    if (value != null)
                    {
                        dto.GetType().InvokeMember(prop.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty,
                            Type.DefaultBinder, dto, new object[] { TimeZoneInfo.ConvertTime(value.Value, CurrentTimeZone) });
                    }
                }
            }

            // Create a shallow copy.
            var origItem = item.Copy();

            // Allow the user to stop things from saving.
            try
            {
                if (BeforeSave(dto, item))
                {
                    IncludeTree includeTree = null;

                    var original = item.Copy<T>();
                    MapDtoToObj(dto, item, includes);
                    try
                    {
                        SetFingerprint(item);
                        // Run validation in this controller
                        var validateResult = Validate(original, dto, item);
                        // Run validation from the POCO if it implements IValidatable
                        if (typeof(IBeforeSave<T, TContext>).IsAssignableFrom(typeof(T)))
                        {
                            var itemAsBeforeSave = item as IBeforeSave<T, TContext>;
                            validateResult.Merge(itemAsBeforeSave.BeforeSave(original, Db, User, includes));
                        }

                        if (validateResult.WasSuccessful)
                        {
                            await Db.SaveChangesAsync();

                            // Pull the object to get any changes.
                            var idString = IdValue(item).ToString();
                            listParams.AddFilter("id", idString);
                            var itemResult = await GetUnmapped(idString, listParams);
                            item = itemResult.Item1;
                            includeTree = itemResult.Item2;

                            // Call the AfterSave method to support special cases.
                            var reloadItem = AfterSave(dto, item, origItem, Db);

                            // Call PostSave if the object has that.
                            if (typeof(IAfterSave<T, TContext>).IsAssignableFrom(typeof(T)))
                            {
                                var itemAsAfterSave = item as IAfterSave<T, TContext>;
                                itemAsAfterSave.AfterSave(original, Db, User, includes);
                            }

                            if (reloadItem && returnObject)
                            {
                                itemResult = await GetUnmapped(idString, listParams);
                                item = itemResult.Item1;
                                includeTree = itemResult.Item2;
                            }

                            result.WasSuccessful = true;
                        }
                        else
                        {
                            result.WasSuccessful = false;
                            result.Message = validateResult.Message;
                            if (validateResult.ReturnObject != null)
                            {
                                result.Object = MapObjToDto(validateResult.ReturnObject, includes);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        //Elmah.ErrorSignal.FromCurrentContext().Raise(ex);
                        result = new SaveResult<TDto>(ex);
                        //Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                    }
                    // Get the key back.
                    if (item != null)
                    {
                        result.Object = MapObjToDto(item, includes, includeTree);
                    }
                }
                else
                {
                    result.WasSuccessful = false;
                    result.Message = "Canceled";
                    //Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                }
            }
            catch (Exception ex)
            {

                result.WasSuccessful = false;
                result.Message = ex.Message;
            }

            return result;
        }


        protected virtual void SetFingerprint(T item)
        {

        }

        /// <summary>
        /// Allows for overriding the mapper from Obj to DTO
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected virtual TDto MapObjToDto(T obj, string includes, IncludeTree tree = null)
        {
            //return Activator.CreateInstance(typeof(TDto), new object[] { obj, User, includes }) as TDto;
            return Mapper<T, TDto>.ObjToDtoMapper(obj, User, includes, tree);
        }
        /// <summary>
        /// Allows for overriding the mapper from DTO to Obj
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>        
        protected virtual void MapDtoToObj(TDto dto, T obj, string includes)
        {
            //dto.Update(obj);
            Mapper<T, TDto>.DtoToObjMapper(dto, obj, User, includes);
        }

        protected SaveResult<TDto> ChangeCollection(int id, string propertyName, int childId, string method)
        {
            // Get the object of the middle class.
            var manyToManyProperty = ClassViewModel.Properties.First(f => string.Compare(f.ManyToManyCollectionName, propertyName, true) == 0);
            if (manyToManyProperty != null && manyToManyProperty.Object != null)
            {
                string table = manyToManyProperty.Object.TableName;
                string thisKeyName = manyToManyProperty.Object.Properties.First(f => f.PureType.Name == ClassViewModel.Name).ObjectIdProperty.ColumnName;
                string otherKeyName = manyToManyProperty.Object.Properties.First(f => !f.IsPrimaryKey && f.IsId && f.ColumnName != thisKeyName).ColumnName;

                try
                {
                    if (method == "Remove")
                    {
                        string sql = "Delete from " + table + " where " + thisKeyName + " = {0} and " + otherKeyName + " = {1}";
                        Db.Database.ExecuteSqlCommand(sql, id, childId);
                    }
                    else if (method == "Add")
                    {
                        string sql = "Insert Into " + table + " (" + thisKeyName + ", " + otherKeyName + ") Values ({0}, {1})";
                        Db.Database.ExecuteSqlCommand(sql, id, childId);
                    }
                    return new SaveResult<TDto>(true, null);
                }
                catch (Exception ex)
                {
                    return new SaveResult<TDto>(ex);
                }
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
                return new SaveResult<TDto>("Could not find many-to-many collection: " + propertyName);
            }

            // GME: This is the code for the collections that are handled by EF, this is not available in EF 7 yet.
            //// Get the primary object.
            //T item = DataSource.Includes().FindItem(id);
            //if (item != null)
            //{
            //    PropertyInfo propInfo = item.GetType().GetProperty(propertyName);
            //    Type childType = propInfo.PropertyType.GenericTypeArguments[0];
            //    if (childType != null)
            //    {
            //        var collection = propInfo.GetValue(item);
            //        if (collection != null)
            //        {
            //            MethodInfo methodInfo = propInfo.PropertyType.GetMethod(method);
            //            if (methodInfo != null)
            //            {
            //                // Get an instance of the object being added
            //                var childObj = Find(childType, childId);
            //                if (childObj != null)
            //                {
            //                    // Add the object to the collection
            //                    // Get a reference to the add method
            //                    // Call the Add method with the parent and the child.
            //                    methodInfo.Invoke(collection, new object[] { childObj });
            //                    Db.SaveChanges();
            //                    result.WasSuccessful = true;
            //                    TDto dto = new TDto();
            //                    Mappers.ObjToDtoMapper.Map(item, dto);
            //                    result.Object = dto;
            //                }
            //                else
            //                {
            //                    result.Message = "Could not find child with ID: " + childId;
            //                    result.WasSuccessful = false;
            //                }
            //            }
            //            else
            //            {
            //                result.Message = "Property is not collection: " + propertyName;
            //                result.WasSuccessful = false;
            //            }
            //        }
            //        else
            //        {
            //            result.Message = "Could not get collection: " + propertyName;
            //            result.WasSuccessful = false;
            //        }
            //    }
            //    else
            //    {
            //        result.Message = "Could not find property: " + propertyName;
            //        result.WasSuccessful = false;
            //    }
            //}
            //else
            //{
            //    result.Message = "Could not find item: " + id;
            //    result.WasSuccessful = false;
            //}
            //return result;
        }






        /// <summary>
        /// Gets the value of the ID in the DTO object using IdField.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        protected object IdValue(TDto dto)
        {
            var propInfo = dto.GetType().GetProperty(DtoViewModel.PrimaryKey.Name);
            object id = propInfo.GetValue(dto);
            return id;
        }

        /// <summary>
        /// Gets the value of the ID in the DTO object using IdField.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected object IdValue(T item)
        {
            object id = ClassViewModel.PrimaryKey.PropertyInfo.GetValue(item);
            return id;
        }

        public DbSet<T> DataSource
        {
            get
            {
                if (_dataSource == null)
                {
                    // find the DbSet if we can
                    foreach (var prop in typeof(TContext).GetProperties())
                    {
                        // See if this is the right one
                        if ((prop.PropertyType.Name == "DbSet`1" ||
                            prop.PropertyType.Name == "IDbSet`1")
                            && prop.PropertyType.GenericTypeArguments.Count() == 1
                            && prop.PropertyType.GenericTypeArguments.First().Name == typeof(T).Name)
                        {
                            _dataSource = (DbSet<T>)prop.GetValue(Db);
                            break;
                        }
                    }
                }

                return _dataSource;
            }
        }

        /// <summary>
        /// Gets an enumerable data source that is read only.
        /// </summary>
        public IQueryable<T> ReadOnlyDataSource
        {
            get
            {
                if (_readOnlyDataSource == null)
                {
                    // find the DbSet if we can
                    foreach (var prop in typeof(TContext).GetProperties())
                    {
                        // See if this is the right one
                        if (prop.PropertyType.Name == "IQueryable`1"
                            && prop.PropertyType.GenericTypeArguments.Count() == 1
                            && prop.PropertyType.GenericTypeArguments.First().Name == typeof(T).Name)
                        {
                            _readOnlyDataSource = (IQueryable<T>)prop.GetValue(Db);
                            break;
                        }
                    }
                }

                return _readOnlyDataSource;
            }
        }


        /// <summary>
        /// Gets a DbSet reference for the type.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        protected PropertyInfo GetDbSet(Type t)
        {
            // find the DbSet if we can
            foreach (var prop in typeof(TContext).GetProperties())
            {
                // See if this is the right one
                if (prop.PropertyType.Name.Contains("DbSet")
                    && prop.PropertyType.GenericTypeArguments.Count() == 1
                    && prop.PropertyType.GenericTypeArguments.First().Name == t.Name)
                {
                    return prop;
                }
            }
            return null;
        }

        protected IQueryable<T> DatabaseCompareExpression(
            IQueryable<T> query,
            PropertyViewModel prop,
            string value,
            TimeZoneInfo timeZone)
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
                        DateTimeOffset dateTimeOffset = new DateTimeOffset(parsedValue, timeZone.BaseUtcOffset);
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
        /// Called after the object is mapped and before it is saved. Allows for returning validation information.
        /// </summary>
        /// <param name="original">Property level copy of original object before mapping.</param>
        /// <param name="dto">Values handed in by the DTO.</param>
        /// <param name="updated">Values to be saved.</param>
        /// <returns></returns>
        protected virtual ValidateResult<T> Validate(T original, TDto dto, T updated)
        {
            return new Models.ValidateResult<T>();
        }

        /// <summary>
        /// Called before the object is mapped and saved. Returning false will cause the save to stop.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected virtual bool BeforeSave(TDto dto, T obj)
        {
            return true;
        }

        /// <summary>
        /// Called after the object is mapped and saved with the new data.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="obj"></param>
        protected virtual bool AfterSave(TDto dto, T obj, T orig, TContext context)
        {
            return false;
        }

        /// <summary>
        /// Called before the object is deleted.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        protected virtual bool BeforeDelete(T obj)
        {
            return true;
        }

        /// <summary>
        /// Called after the object is deleted.
        /// </summary>
        /// <param name="obj"></param>
        /// <remarks>When this is called, the object has been deleted.</remarks>
        protected virtual bool AfterDelete(T obj, TContext context)
        {
            return true;
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var response = context.HttpContext.Response;

            if (response.StatusCode == (int)HttpStatusCode.OK)
            {
                var contents = context.Result as ObjectResult;
                if (contents != null)
                {
                    var listResult = contents.Value as ListResult;
                    var saveResult = contents.Value as SaveResult;
                    if ((listResult != null && !listResult.WasSuccessful) || (saveResult != null && !saveResult.WasSuccessful))
                    {
                        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                }
            }
            base.OnActionExecuted(context);
        }
    }
}
