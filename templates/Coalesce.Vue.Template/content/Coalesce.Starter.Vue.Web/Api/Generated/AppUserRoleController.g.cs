
using Coalesce.Starter.Vue.Web.Models;
using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Api.Behaviors;
using IntelliTect.Coalesce.Api.Controllers;
using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Mapping.IncludeTrees;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Coalesce.Starter.Vue.Web.Api
{
    [Route("api/AppUserRole")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class AppUserRoleController
        : BaseApiController<Coalesce.Starter.Vue.Data.Models.AppUserRole, AppUserRoleParameter, AppUserRoleResponse, Coalesce.Starter.Vue.Data.AppDbContext>
    {
        public AppUserRoleController(CrudContext<Coalesce.Starter.Vue.Data.AppDbContext> context) : base(context)
        {
            GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<Coalesce.Starter.Vue.Data.Models.AppUserRole>();
        }

        [HttpGet("get/{id}")]
        [Authorize(Roles = "UserAdmin")]
        public virtual Task<ItemResult<AppUserRoleResponse>> Get(
            string id,
            DataSourceParameters parameters,
            IDataSource<Coalesce.Starter.Vue.Data.Models.AppUserRole> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [Authorize(Roles = "UserAdmin")]
        public virtual Task<ListResult<AppUserRoleResponse>> List(
            ListParameters parameters,
            IDataSource<Coalesce.Starter.Vue.Data.Models.AppUserRole> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [Authorize(Roles = "UserAdmin")]
        public virtual Task<ItemResult<int>> Count(
            FilterParameters parameters,
            IDataSource<Coalesce.Starter.Vue.Data.Models.AppUserRole> dataSource)
            => CountImplementation(parameters, dataSource);

        [HttpPost("save")]
        [Authorize]
        public virtual Task<ItemResult<AppUserRoleResponse>> Save(
            [FromForm] AppUserRoleParameter dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Starter.Vue.Data.Models.AppUserRole> dataSource,
            IBehaviors<Coalesce.Starter.Vue.Data.Models.AppUserRole> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        [HttpPost("bulkSave")]
        [Authorize(Roles = "UserAdmin")]
        public virtual Task<ItemResult<AppUserRoleResponse>> BulkSave(
            [FromBody] BulkSaveRequest dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Starter.Vue.Data.Models.AppUserRole> dataSource,
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromServices] IBehaviorsFactory behaviorsFactory)
            => BulkSaveImplementation(dto, parameters, dataSource, dataSourceFactory, behaviorsFactory);

        [HttpPost("delete/{id}")]
        [Authorize(Roles = "UserAdmin")]
        public virtual Task<ItemResult<AppUserRoleResponse>> Delete(
            string id,
            IBehaviors<Coalesce.Starter.Vue.Data.Models.AppUserRole> behaviors,
            IDataSource<Coalesce.Starter.Vue.Data.Models.AppUserRole> dataSource)
            => DeleteImplementation(id, new DataSourceParameters(), dataSource, behaviors);
    }
}
