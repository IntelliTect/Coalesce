using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Data;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.Extensions.DependencyInjection;
using IntelliTect.Coalesce.Mapping.IncludeTrees;
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
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace IntelliTect.Coalesce.Api
{
    public abstract class BaseApiController<T, TDto, TContext> : Controller
        where T : class, new()
        where TDto : class, IClassDto<T>, new()
        where TContext : DbContext
    {
        protected BaseApiController(TContext db)
        {
            Db = db;

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

        public TContext Db { get; }

        protected ClassViewModel ClassViewModel { get; }

        protected ClassViewModel DtoViewModel { get; }

        protected DbSet<T> _dataSource;
        protected IQueryable<T> _readOnlyDataSource;

        // TODO: service antipattern. Inject this properly.
        protected ILogger Logger
        {
            get
            {
                if (_Logger == null)
                {
                    ILogger<object>
                    _Logger = HttpContext?.RequestServices.GetService<Logger<BaseApiController<T, TDto, TContext>>>();
                }

                return _Logger;
            }
        }
        private ILogger _Logger = null;

        protected virtual IDataSource<T> ActivateDataSource<TSource>() where TSource : IDataSource<T> =>
            ActivatorUtilities.GetServiceOrCreateInstance<TSource>(HttpContext.RequestServices);

        protected virtual IDataSource<T> GetDataSource(IDataSourceParameters parameters)
        {
            switch (parameters.DataSource)
            {
                case "":
                case "Default":
                case null:
                    return ActivateDataSource<StandardDataSource<T, TContext>>();
                default:
                    throw new KeyNotFoundException($"Data source '{parameters.DataSource}' not found.");
            }

            // TODO: how does this work for IClassDtos?
            //return DataSource ?? ReadOnlyDataSource;
        }

        protected async Task<ListResult<TDto>> ListImplementation(ListParameters listParameters, IDataSource<T> dataSource)
        {
            try
            {
                return await dataSource.GetMappedListAsync<TDto>(listParameters);
            }
            catch (Exception ex)
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.InternalServerError;
                Logger?.LogError(ex.Message, ex);
                return new ListResult<TDto>(ex);
            }
        }


        protected async Task<int> CountImplementation(FilterParameters parameters, IDataSource<T> dataSource)
        {
            try
            {
                return await dataSource.GetCountAsync(parameters);
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

            // TODO: figure out where this lives in the new world of datasources & behaviors

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

        protected Task<TDto> GetImplementation(object id, DataSourceParameters parameters, IDataSource<T> dataSource)
        {
            return dataSource.GetMappedItemAsync<TDto>(id, parameters);
        }
        
        protected Task<ItemResult> DeleteImplementation(object id, IBehaviors<T> behaviors)
        {
            return behaviors.DeleteAsync(id);
        }

        protected Task<ItemResult<TDto>> SaveImplementation(TDto dto, DataSourceParameters parameters, IDataSource<T> dataSource, IBehaviors<T> behaviors)
        {
            return behaviors.SaveAsync(dto, parameters, dataSource);
        }

        protected ItemResult<TDto> ChangeCollection(object id, string propertyName, int childId, string method)
        {
            // TODO: this only supports ints


            // Get the object of the middle class.
            var manyToManyProperty = ClassViewModel.ClientProperties.First(f => string.Compare(f.ManyToManyCollectionName, propertyName, true) == 0);
            if (manyToManyProperty != null && manyToManyProperty.Object != null)
            {
                // Check security on the collection property that holds the many-to-many objects.
                if (!manyToManyProperty.SecurityInfo.IsEditable(User))
                {
                    Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                    return new ItemResult<TDto>("Unauthorized");
                }

                var joinClass = manyToManyProperty.Object;
                string thisKeyName = joinClass.ClientProperties.First(f => f.PureType.EqualsType(ClassViewModel.Type)).ObjectIdProperty.ColumnName;
                string otherKeyName = joinClass.ClientProperties.First(f => !f.IsPrimaryKey && f.IsId && f.ColumnName != thisKeyName).ColumnName;

                var mapping = Db.Model.FindEntityType(joinClass.FullyQualifiedName).Relational();
                var tableName = $"{mapping.Schema}.{mapping.TableName}";

                try
                {
                    if (method == "Remove")
                    {
                        // Check permissions for deleting the many-to-many objects.
                        if (!joinClass.SecurityInfo.IsDeleteAllowed(User))
                        {
                            Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                            return new ItemResult<TDto>("Unauthorized");
                        }
                        string sql = "DELETE FROM " + tableName + " WHERE " + thisKeyName + " = {0} AND " + otherKeyName + " = {1}";
                        Db.Database.ExecuteSqlCommand(sql, id, childId);
                    }
                    else if (method == "Add")
                    {
                        // Check permissions for creating the many-to-many objects.
                        if (!joinClass.SecurityInfo.IsCreateAllowed(User))
                        {
                            Response.StatusCode = (int)System.Net.HttpStatusCode.Forbidden;
                            return new ItemResult<TDto>("Unauthorized");
                        }

                        // TODO: (maybe?) Check if the user is allowed to read the objects on the other side of the relationship (otherKeyName).
                        // This prevents an attack where the user just makes up foreign key values, hoping they're valid.

                        string sql = "INSERT INTO " + tableName + " (" + thisKeyName + ", " + otherKeyName + ") VALUES ({0}, {1})";
                        Db.Database.ExecuteSqlCommand(sql, id, childId);
                    }
                    return new ItemResult<TDto>(true, null);
                }
                catch (Exception ex)
                {
                    return new ItemResult<TDto>(ex);
                }
            }
            else
            {
                Response.StatusCode = (int)System.Net.HttpStatusCode.NotFound;
                return new ItemResult<TDto>("Could not find many-to-many collection: " + propertyName);
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


        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //context.ModelState.FindKeysWithPrefix("dataSource")
            if (!context.ModelState.IsValid)
            {
                var errors = context.ModelState
                    .Where(v => v.Value.Errors.Any() && v.Key.StartsWith("dataSource", StringComparison.InvariantCultureIgnoreCase))
                    .SelectMany(v => v.Value.Errors.Select(e => (key: v.Key, error: e.ErrorMessage)))
                    .ToList();

                if (errors.Any())
                {
                    // TODO: this could be more robust.
                    // Lots of client methods in the typescript aren't expecting an object that looks like this.
                    // Anything that takes a SaveResult or ListResult should be fine, but other things (Csv..., Count, Delete, Get) won't handle this.
                    context.Result = this.BadRequest(new ApiResult(string.Join("; ", errors.Select(e => $"Invalid value for parameter {e.key}: {e.error}"))));
                    return;
                }
            }

            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            var response = context.HttpContext.Response;

            if (response.StatusCode == (int)HttpStatusCode.OK)
            {
                var contents = context.Result as Microsoft.AspNetCore.Mvc.ObjectResult;
                if (contents != null)
                {
                    var listResult = contents.Value as ListResult<T>;
                    var saveResult = contents.Value as ItemResult;
                    if ((listResult != null && !listResult.WasSuccessful) || (saveResult != null && !saveResult.WasSuccessful))
                    {
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    }
                }
            }
            base.OnActionExecuted(context);
        }
    }
}
