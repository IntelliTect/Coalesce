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
using Microsoft.AspNetCore.Hosting;

namespace IntelliTect.Coalesce.Api.Controllers
{
    public abstract class BaseApiController<T, TDto> : Controller
        where T : class, new()
        where TDto : class, IClassDto<T>, new()
    {
        protected BaseApiController()
        {
            EntityClassViewModel = ReflectionRepository.Global.GetClassViewModel<T>()!;
        }

        /// <summary>
        /// A ClassViewModel representing the entity type T that is served by this controller,
        /// independent of the DTO that will encapsulate the type in inputs and outputs.
        /// </summary>
        protected ClassViewModel EntityClassViewModel { get; }

        /// <summary>
        /// For generated controllers, the type that the controller was generated for.
        /// For custom IClassDtos, this is the DTO type. Otherwise, this is the entity type.
        /// </summary>
        protected ClassViewModel? GeneratedForClassViewModel { get; set; }

        protected Task<ItemResult<TDto>> GetImplementation(object id, DataSourceParameters parameters, IDataSource<T> dataSource)
        {
            return dataSource.GetMappedItemAsync<TDto>(id, parameters);
        }

        protected Task<ListResult<TDto>> ListImplementation(ListParameters listParameters, IDataSource<T> dataSource)
        {
            return dataSource.GetMappedListAsync<TDto>(listParameters);
        }

        protected Task<ItemResult<int>> CountImplementation(FilterParameters parameters, IDataSource<T> dataSource)
        {
            return dataSource.GetCountAsync(parameters);
        }

        protected async Task<ItemResult<TDto?>> SaveImplementation(TDto dto, DataSourceParameters parameters, IDataSource<T> dataSource, IBehaviors<T> behaviors)
        {
            var kind = (await behaviors.DetermineSaveKindAsync(dto, dataSource, parameters)).Kind;

            if (GeneratedForClassViewModel is null)
            {
                throw new InvalidOperationException($"{nameof(GeneratedForClassViewModel)} must be set.");
            }

            if (kind == SaveKind.Create && !GeneratedForClassViewModel.SecurityInfo.IsCreateAllowed(User))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return $"Creation of {GeneratedForClassViewModel.DisplayName} items not allowed.";
            }
            if (kind == SaveKind.Update && !GeneratedForClassViewModel.SecurityInfo.IsEditAllowed(User))
            {
                Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                return $"Editing of {GeneratedForClassViewModel.DisplayName} items not allowed.";
            }

            return await behaviors.SaveAsync(dto, dataSource, parameters);
        }

        protected Task<ItemResult<TDto?>> DeleteImplementation(object id, DataSourceParameters parameters, IDataSource<T> dataSource, IBehaviors<T> behaviors)
        {
            return behaviors.DeleteAsync<TDto>(id, dataSource, parameters);
        }
    }

    public abstract class BaseApiController<T, TDto, TContext> : BaseApiController<T, TDto>
        where T : class, new()
        where TDto : class, IClassDto<T>, new()
        where TContext : DbContext
    {
        protected BaseApiController(TContext db) : base()
        {
            Db = db;
        }

        public TContext Db { get; }
    }
}
