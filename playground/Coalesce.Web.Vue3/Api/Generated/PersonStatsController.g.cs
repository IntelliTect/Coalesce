
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
    [Route("api/PersonStats")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class PersonStatsController
        : BaseApiController<Coalesce.Domain.PersonStats, PersonStatsParameter, PersonStatsResponse, Coalesce.Domain.AppDbContext>
    {
        public PersonStatsController(CrudContext<Coalesce.Domain.AppDbContext> context) : base(context)
        {
            GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<Coalesce.Domain.PersonStats>();
        }

        [HttpGet("get/{id}")]
        [Authorize]
        public virtual Task<ItemResult<PersonStatsResponse>> Get(
            int id,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.PersonStats> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [Authorize]
        public virtual Task<ListResult<PersonStatsResponse>> List(
            [FromQuery] ListParameters parameters,
            IDataSource<Coalesce.Domain.PersonStats> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [Authorize]
        public virtual Task<ItemResult<int>> Count(
            [FromQuery] FilterParameters parameters,
            IDataSource<Coalesce.Domain.PersonStats> dataSource)
            => CountImplementation(parameters, dataSource);

        [HttpPost("save")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        [Authorize]
        public virtual Task<ItemResult<PersonStatsResponse>> Save(
            [FromForm] PersonStatsParameter dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.PersonStats> dataSource,
            IBehaviors<Coalesce.Domain.PersonStats> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        [HttpPost("save")]
        [Consumes("application/json")]
        [Authorize]
        public virtual Task<ItemResult<PersonStatsResponse>> SaveFromJson(
            [FromBody] PersonStatsParameter dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.PersonStats> dataSource,
            IBehaviors<Coalesce.Domain.PersonStats> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        [HttpPost("bulkSave")]
        [Authorize]
        public virtual Task<ItemResult<PersonStatsResponse>> BulkSave(
            [FromBody] BulkSaveRequest dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.PersonStats> dataSource,
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromServices] IBehaviorsFactory behaviorsFactory)
            => BulkSaveImplementation(dto, parameters, dataSource, dataSourceFactory, behaviorsFactory);

        [HttpPost("delete/{id}")]
        [Authorize]
        public virtual Task<ItemResult<PersonStatsResponse>> Delete(
            int id,
            IBehaviors<Coalesce.Domain.PersonStats> behaviors,
            IDataSource<Coalesce.Domain.PersonStats> dataSource)
            => DeleteImplementation(id, new DataSourceParameters(), dataSource, behaviors);
    }
}
