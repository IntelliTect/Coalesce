
using Coalesce.Web.Vue3.Models;
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

namespace Coalesce.Web.Vue3.Api
{
    [Route("api/StandaloneReadonly")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class StandaloneReadonlyController
        : BaseApiController<Coalesce.Domain.StandaloneReadonly, StandaloneReadonlyParameter, StandaloneReadonlyResponse>
    {
        public StandaloneReadonlyController(CrudContext context) : base(context)
        {
            GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<Coalesce.Domain.StandaloneReadonly>();
        }

        [HttpGet("get/{id}")]
        [Authorize]
        public virtual Task<ItemResult<StandaloneReadonlyResponse>> Get(
            int id,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.StandaloneReadonly> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [Authorize]
        public virtual Task<ListResult<StandaloneReadonlyResponse>> List(
            [FromQuery] ListParameters parameters,
            IDataSource<Coalesce.Domain.StandaloneReadonly> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [Authorize]
        public virtual Task<ItemResult<int>> Count(
            [FromQuery] FilterParameters parameters,
            IDataSource<Coalesce.Domain.StandaloneReadonly> dataSource)
            => CountImplementation(parameters, dataSource);
    }
}
