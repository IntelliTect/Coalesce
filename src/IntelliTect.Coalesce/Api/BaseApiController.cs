using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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

            EntityClassViewModel = ReflectionRepository.Global.GetClassViewModel<T>();
        }

        public TContext Db { get; }

        /// <summary>
        /// A ClassViewModel representing the entity type T that is served by this controller,
        /// indepdent of the DTO that will encapsulate the type in inputs and outputs.
        /// </summary>
        protected ClassViewModel EntityClassViewModel { get; }

        /// <summary>
        /// For generated controllers, the type that the controller was generated for.
        /// For custom IClassDtos, this is the DTO type. Otherwise, this is the entity type.
        /// </summary>
        protected ClassViewModel GeneratedForClassViewModel { get; set; }

        // TODO: service antipattern. Inject this properly.
        protected ILogger Logger
        {
            get
            {
                if (_Logger != null) return _Logger;
                return _Logger = HttpContext?.RequestServices.GetService<Logger<BaseApiController<T, TDto, TContext>>>();
            }
        }
        private ILogger _Logger = null;

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
            var kind = behaviors.DetermineSaveKind(dto).Kind;

            if (kind == SaveKind.Create && !GeneratedForClassViewModel.SecurityInfo.IsCreateAllowed(User))
            {
                return Task.FromResult<ItemResult<TDto>>($"Creation of {GeneratedForClassViewModel.Name} items not allowed.");
            }
            if (kind == SaveKind.Update && !GeneratedForClassViewModel.SecurityInfo.IsEditAllowed(User))
            {
                return Task.FromResult<ItemResult<TDto>>($"Editing of {GeneratedForClassViewModel.Name} items not allowed.");
            }

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
