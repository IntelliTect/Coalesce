
using Coalesce.Web.Vue3.Models;
using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Api;
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
    [Route("api/StandaloneReadCreate")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class StandaloneReadCreateController
        : BaseApiController<Coalesce.Domain.StandaloneReadCreate, StandaloneReadCreateDtoGen>
    {
        public StandaloneReadCreateController(CrudContext context) : base(context)
        {
            GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<Coalesce.Domain.StandaloneReadCreate>();
        }

        [HttpGet("get/{id}")]
        [Authorize]
        public virtual Task<ItemResult<StandaloneReadCreateDtoGen>> Get(
            int id,
            DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.StandaloneReadCreate> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [Authorize]
        public virtual Task<ListResult<StandaloneReadCreateDtoGen>> List(
            ListParameters parameters,
            IDataSource<Coalesce.Domain.StandaloneReadCreate> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [Authorize]
        public virtual Task<ItemResult<int>> Count(
            FilterParameters parameters,
            IDataSource<Coalesce.Domain.StandaloneReadCreate> dataSource)
            => CountImplementation(parameters, dataSource);

        [HttpPost("save")]
        [Authorize]
        public virtual Task<ItemResult<StandaloneReadCreateDtoGen>> Save(
            [FromForm] StandaloneReadCreateDtoGen dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.StandaloneReadCreate> dataSource,
            IBehaviors<Coalesce.Domain.StandaloneReadCreate> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        [HttpPost("delete/{id}")]
        [Authorize]
        public virtual Task<ItemResult<StandaloneReadCreateDtoGen>> Delete(
            int id,
            IBehaviors<Coalesce.Domain.StandaloneReadCreate> behaviors,
            IDataSource<Coalesce.Domain.StandaloneReadCreate> dataSource)
            => DeleteImplementation(id, new DataSourceParameters(), dataSource, behaviors);
    }
}
