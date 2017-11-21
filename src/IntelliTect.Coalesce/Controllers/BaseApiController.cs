using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
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
            ClassViewModel = ReflectionRepository.Global.GetClassViewModel<T>();
            if (typeof(T) == typeof(TDto) || typeof(TDto).Name.EndsWith("DtoGen"))
            {
                DtoViewModel = ClassViewModel;
            }
            else
            {
                DtoViewModel = ReflectionRepository.Global.GetClassViewModel<TDto>();
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

        public static TimeZoneInfo CurrentTimeZone { get; set; } = TimeZoneInfo.FindSystemTimeZoneById("Pacific Standard Time");

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

        protected IDataSource<T> GetIDataSource(ListParameters listParameters)
        {
            IQueryable<T> query = GetDataSource(listParameters);
            return new OldDataSourceInteropDataSource<T, TContext>(Db, query)
            {
                ListParameters = listParameters,
                User = User,
                TimeZone = CurrentTimeZone
            };
        }

        protected async Task<ListResult<TDto>> ListImplementation(ListParameters listParameters)
        {
            try
            {
                return await GetIDataSource(listParameters).GetMappedListAsync<TDto>();
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                Logger?.LogError(ex.Message, ex);
                return new ListResult<TDto>(ex);
            }
        }


        protected async Task<int> CountImplementation(ListParameters listParameters)
        {
            try
            {
                return await GetIDataSource(listParameters).GetCountAsync();
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex.Message, ex);
                Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;

                // TODO: don't rethrow?
                throw ex;
            }
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
            if (originalProp != null && originalProp.IsClientProperty)
            {
                if (!originalProp.SecurityInfo.IsReadable(User)) throw new AccessViolationException($"{property} is not accessible by current user.");

                List<PropertyViewModel> properties = new List<PropertyViewModel>();
                if (originalProp.ListGroup != null)
                {
                    properties.AddRange(ClassViewModel.ClientProperties.Where(f => f.ListGroup == originalProp.ListGroup));
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

        protected Task<TDto> GetImplementation(string id, ListParameters listParameters)
        {
            return GetIDataSource(listParameters).GetMappedItemAsync<TDto>(id);
        }

        private Task<(T Item, IncludeTree includeTree)> GetUnmapped(string id, ListParameters listParameters)
        {
            return GetIDataSource(listParameters).GetItemAsync(id);
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
            foreach (var prop in DtoViewModel.ClientProperties.Where(f => f.Type.IsDateTimeOffset))
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
            var context = new MappingContext(User, includes);
            return Mapper<T, TDto>.ObjToDtoMapper(obj, context, tree);
        }
        /// <summary>
        /// Allows for overriding the mapper from DTO to Obj
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>        
        protected virtual void MapDtoToObj(TDto dto, T obj, string includes)
        {
            //dto.Update(obj);
            var context = new MappingContext(User, includes);
            Mapper<T, TDto>.DtoToObjMapper(dto, obj, context);
        }

        protected SaveResult<TDto> ChangeCollection(int id, string propertyName, int childId, string method)
        {
            // Get the object of the middle class.
            var manyToManyProperty = ClassViewModel.ClientProperties.First(f => string.Compare(f.ManyToManyCollectionName, propertyName, true) == 0);
            if (manyToManyProperty != null && manyToManyProperty.Object != null)
            {
                // Check security on the collection property that holds the many-to-many objects.
                if (!manyToManyProperty.SecurityInfo.IsEditable(User))
                {
                    Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return new SaveResult<TDto>("Unauthorized");
                }

                var joinClass = manyToManyProperty.Object;
                string tableName = joinClass.TableName;
                string thisKeyName = joinClass.ClientProperties.First(f => f.PureType.Name == ClassViewModel.Name).ObjectIdProperty.ColumnName;
                string otherKeyName = joinClass.ClientProperties.First(f => !f.IsPrimaryKey && f.IsId && f.ColumnName != thisKeyName).ColumnName;

                try
                {
                    if (method == "Remove")
                    {
                        // Check permissions for deleting the many-to-many objects.
                        if (!manyToManyProperty.Object.SecurityInfo.IsDeleteAllowed(User))
                        {
                            Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                            return new SaveResult<TDto>("Unauthorized");
                        }
                        string sql = "Delete from " + tableName + " where " + thisKeyName + " = {0} and " + otherKeyName + " = {1}";
                        Db.Database.ExecuteSqlCommand(sql, id, childId);
                    }
                    else if (method == "Add")
                    {
                        // Check permissions for creating the many-to-many objects.
                        if (!manyToManyProperty.Object.SecurityInfo.IsCreateAllowed(User))
                        {
                            Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                            return new SaveResult<TDto>("Unauthorized");
                        }

                        // TODO: (maybe?) Check if the user is allowed to read the objects on the other side of the relationship (otherKeyName).
                        // This prevents an attack where the user just makes up foreign key values, hoping they're valid.

                        string sql = "Insert Into " + tableName + " (" + thisKeyName + ", " + otherKeyName + ") Values ({0}, {1})";
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
                    var listResult = contents.Value as ListResult<T>;
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
