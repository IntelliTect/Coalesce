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
using IntelliTect.Coalesce.Helpers;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Linq.Expressions;

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
            ClassViewModel = ReflectionRepository.GetClassViewModel(typeof(T), null, ApiName);
            if (typeof(T) == typeof(TDto) || typeof(TDto).Name.EndsWith("DtoGen"))
            {
                DtoViewModel = ClassViewModel;
            }
            else
            {
                DtoViewModel = ReflectionRepository.GetClassViewModel(typeof(TDto), null, ApiName);
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

        protected string ApiName
        {
            get { return this.GetType().Name.Replace("Controller", ""); }
        }

        protected virtual IQueryable<T> GetListDataSource(ListParameters listParameters)
        {
            if (!string.IsNullOrWhiteSpace(listParameters.ListDataSource) && listParameters.ListDataSource != "Default")
            {
                // find the IQueryable if we can
                var method = typeof(T).GetMethod(listParameters.ListDataSource);
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
                IQueryable<T> result = GetListDataSource(listParameters);

                // Add the Include statements to the result to grab the right object graph. Passing "" gets the standard set.
                if (string.Compare(listParameters.Includes, "none", StringComparison.InvariantCultureIgnoreCase) != 0)
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
                    result = result.OrderBy(string.Join(", ", orderByParams.Select(f => $"{f.Key} {f.Value}")));
                }
                else
                {
                    // Use the DefaultOrderBy attributes if available
                    if (ClassViewModel.DefaultOrderBy.Any())
                    {
                        var orderByClauseList = new List<string>();
                        foreach (var orderInfo in ClassViewModel.DefaultOrderBy)
                        {
                            if (orderInfo.OrderByDirection == DefaultOrderByAttribute.OrderByDirections.Ascending)
                            {
                                orderByClauseList.Add($"{orderInfo.FieldName} ASC");
                            }
                            else
                            {
                                orderByClauseList.Add($"{orderInfo.FieldName} DESC");
                            }
                        }
                        result = result.OrderBy(string.Join(",", orderByClauseList));
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
                //if (new T() is IExcludable)
                //{
                //    foreach (var obj in result2)
                //    {
                //        ((IExcludable)obj).Exclude(listParameters.Includes);
                //    }
                //}

                // Allow for security trimming
                // TODO: This needs to be adjusted to handle paging correctly.
                var result3 = result2.Where(f => BeforeGet(f));

                IEnumerable<TDto> result4 = result3.ToList().Select(obj => MapObjToDto(obj, listParameters.Includes)).ToList();

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
                IQueryable<T> result = GetListDataSource(listParameters);

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
            // TODO: Fix for SQL Injection
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
            // TODO: Fix for SQL Injection
            if (!string.IsNullOrWhiteSpace(listParameters.Where))
            {
                result = result.Where(listParameters.Where);
            }

            // Add general search filters.
            // These search specified fields in the class
            if (!string.IsNullOrWhiteSpace(listParameters.Search))
            {
                var completeSearchClauses = new List<string>();
                // Handle the split on spaces first because it will be done differently with ands and ors.
                if (ClassViewModel.SearchProperties().Any(f => f.Value.SearchIsSplitOnSpaces))
                {
                    var splitSearchClauses = new List<string>();

                    var clauses = listParameters.Search.Split(new string[] { " ", ", ", " ," }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var clause in clauses)
                    {
                        var searchClauses = new List<string>();
                        foreach (var prop in ClassViewModel.SearchProperties().Where(f => f.Value.SearchIsSplitOnSpaces))
                        {
                            string expr;
                            if (prop.Value.PureType.IsString)
                            {
                                if (prop.Key.Contains("[]."))
                                {
                                    var parts = prop.Key.Split(new[] { "[]." }, StringSplitOptions.RemoveEmptyEntries);
                                    expr = $@"{parts[0]}.Count({parts[1]}.{prop.Value.SearchMethodName}(""{clause}"")) > 0";
                                }
                                else
                                {
                                    expr = $"{prop.Key}.{prop.Value.SearchMethodName}(\"{clause}\")";
                                }
                            }
                            else
                            {
                                if (prop.Key.Contains("[]."))
                                {
                                    var parts = prop.Key.Split(new[] { "[]." }, StringSplitOptions.RemoveEmptyEntries);
                                    expr = $@"{parts[0]}.Count({parts[1]}.ToString().{prop.Value.SearchMethodName}(""{clause}"")) > 0";
                                }
                                else
                                {
                                    expr = $@"{prop.Key}.ToString().{prop.Value.SearchMethodName}(""{clause}"")";
                                }
                            }
                            searchClauses.Add(expr);
                        }
                        if (searchClauses.Count > 0)
                        {
                            splitSearchClauses.Add("( " + string.Join(" || ", searchClauses) + " )");
                        }
                    }
                    completeSearchClauses.Add("( " + string.Join(" && ", splitSearchClauses) + " )");
                }

                // Handle not split on spaces with simple ors.
                if (ClassViewModel.SearchProperties().Any(f => !f.Value.SearchIsSplitOnSpaces))
                {
                    foreach (var prop in ClassViewModel.SearchProperties().Where(f => !f.Value.SearchIsSplitOnSpaces))
                    {
                        int temp;
                        if (prop.Value.PureType.IsString)
                        {
                            string expr;
                            if (prop.Key.Contains("[]."))
                            {
                                var parts = prop.Key.Split(new[] { "[]." }, StringSplitOptions.RemoveEmptyEntries);
                                expr = $@"{parts[0]}.Count({parts[1]}.{prop.Value.SearchMethodName}(""{listParameters.Search}"")) > 0";
                            }
                            else
                            {
                                expr = $"{prop.Key}.{prop.Value.SearchMethodName}(\"{listParameters.Search}\")";
                            }
                            completeSearchClauses.Add(expr);
                        }
                        else if (int.TryParse(listParameters.Search, out temp))
                        {
                            string expr;
                            if (prop.Key.Contains("[]."))
                            {
                                var parts = prop.Key.Split(new[] { "[]." }, StringSplitOptions.RemoveEmptyEntries);
                                expr = $@"{parts[0]}.Count({prop.Key} = {listParameters.Search}) > 0";
                            }
                            else
                            {
                                expr = $"{prop.Key} = {listParameters.Search}";
                            }
                            completeSearchClauses.Add(expr);
                        }
                    }
                }

                if (completeSearchClauses.Any())
                {
                    string finalSearchClause = string.Join(" || ", completeSearchClauses);
                    result = result.Where(finalSearchClause);
                }

            }
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
                    var result2 = matches.GroupBy(prop.Name, "it").OrderBy("it.Key").Select("it.Key").Skip(20 * (page - 1)).Take(20) as IEnumerable<string>;
                    result.AddRange(result2);
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

        protected async Task<TDto> GetImplementation(string id, string includes = null)
        {
            // Get the item and get external data.
            var item = (await DataSource.Includes(includes).FindItemAsync(id)).IncludeExternal(includes);

            if (!BeforeGet(item))
            {
                return null;
            }
            //// Exclude data
            //if (item is IExcludable)
            //{
            //    ((IExcludable)item).Exclude(includes);
            //}
            // Map to DTO
            var dto = MapObjToDto(item, includes);

            return dto;
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
                    DataSource.Remove(item);
                    // Allow for other cascade deletes.
                    if (item is IDeletable<TContext>)
                    {
                        (item as IDeletable<TContext>).BeforeDeleteCommit(Db);
                    }
                    Db.SaveChanges();
                    return AfterDelete(item, Db);
                }
            }
            Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
            return false;
        }

        protected SaveResult<TDto> SaveImplementation(TDto dto, string includes = null, bool returnObject = true)
        {
            var result = new SaveResult<TDto>();

            // See if this is new or an update using the key.
            T item = null;

            object idValue = IdValue(dto);

            if (idValue is int && (int)idValue != 0 || idValue is string && (string)idValue != "")
            {
                item = DataSource.FindItem(idValue);
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
                    var original = item.Copy<T>();
                    MapDtoToObj(dto, item, includes);
                    try
                    {
                        SetFingerprint(item);
                        // Run validation in this controller
                        var validateResult = Validate(original, dto, item);
                        // Run validation from the POCO if it implements IValidatable
                        if (typeof(IValidatable<T, TContext>).IsAssignableFrom(typeof(T)))
                        {
                            var validatable = item as IValidatable<T, TContext>;
                            validateResult.Merge(validatable.Validate(original, Db, User, includes));
                        }

                        if (validateResult.WasSuccessful)
                        {
                            Db.SaveChanges();
                            // Pull the object to get any changes.
                            item = DataSource.Includes(includes).FindItem(IdValue(item));
                            // Call the method to support special cases.
                            if (AfterSave(dto, item, origItem, Db))
                            {
                                // Call PostSave if the object has that.
                                if (typeof(IPostSavable<T, TContext>).IsAssignableFrom(typeof(T)))
                                {
                                    var postSavable = item as IPostSavable<T, TContext>;
                                    postSavable.PostSave(original, Db, User, includes);
                                }

                                if (returnObject)
                                {
                                    item = DataSource.Includes(includes).FindItem(IdValue(item));
                                }
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
                        //if (item is IExcludable)
                        //{
                        //    ((IExcludable)item).Exclude(includes);
                        //}
                        result.Object = MapObjToDto(item, includes);
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
        protected virtual TDto MapObjToDto(T obj, string includes)
        {
            //return Activator.CreateInstance(typeof(TDto), new object[] { obj, User, includes }) as TDto;
            return Mapper<T, TDto>.ObjToDtoMapper(obj, User, includes);
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

            if (response.StatusCode == 200)
            {
                var contents = context.Result as ObjectResult;
                if (contents != null)
                {
                    var listResult = contents.Value as ListResult;
                    var saveResult = contents.Value as SaveResult;
                    if ((listResult != null && !listResult.WasSuccessful) || (saveResult != null && !saveResult.WasSuccessful))
                    {
                        context.HttpContext.Response.StatusCode = 500;
                    }
                }
            }
            base.OnActionExecuted(context);
        }
    }
}
