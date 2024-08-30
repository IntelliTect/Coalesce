
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
    [Route("api/AppRoleClaim")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class AppRoleClaimController
        : BaseApiController<AppRoleClaim, AppRoleClaimParameter, AppRoleClaimResponse, Coalesce.Starter.Vue.Data.AppDbContext>
    {
        public AppRoleClaimController(CrudContext<Coalesce.Starter.Vue.Data.AppDbContext> context) : base(context)
        {
            GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<AppRoleClaim>();
        }

        [HttpGet("get/{id}")]
        [Authorize]
        public virtual Task<ItemResult<AppRoleClaimResponse>> Get(
            int id,
            DataSourceParameters parameters,
            IDataSource<AppRoleClaim> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [Authorize]
        public virtual Task<ListResult<AppRoleClaimResponse>> List(
            ListParameters parameters,
            IDataSource<AppRoleClaim> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [Authorize]
        public virtual Task<ItemResult<int>> Count(
            FilterParameters parameters,
            IDataSource<AppRoleClaim> dataSource)
            => CountImplementation(parameters, dataSource);

        [HttpPost("bulkSave")]
        [Authorize]
        public virtual Task<ItemResult<AppRoleClaimResponse>> BulkSave(
            [FromBody] BulkSaveRequest dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<AppRoleClaim> dataSource,
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromServices] IBehaviorsFactory behaviorsFactory)
            => BulkSaveImplementation(dto, parameters, dataSource, dataSourceFactory, behaviorsFactory);
    }
}
