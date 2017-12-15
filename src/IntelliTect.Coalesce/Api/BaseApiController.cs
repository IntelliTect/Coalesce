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
                    IQueryable<T> matches = Db.Set<T>();
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
                    context.Result = BadRequest(
                        new ApiResult(string.Join("; ", errors.Select(e => $"Invalid value for parameter {e.key}: {e.error}")))
                    );
                    return;
                }
            }

            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            
            var response = context.HttpContext.Response;

            if (response.StatusCode == (int)HttpStatusCode.OK
                && context.Result is ObjectResult result
                && result.Value is ApiResult apiResult
                && !apiResult.WasSuccessful
            )
            {
                response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
            base.OnActionExecuted(context);
        }
    }
}
