
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
    [Route("api/DateOnlyPk")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class DateOnlyPkController
        : BaseApiController<Coalesce.Domain.DateOnlyPk, DateOnlyPkParameter, DateOnlyPkResponse, Coalesce.Domain.AppDbContext>
    {
        public DateOnlyPkController(CrudContext<Coalesce.Domain.AppDbContext> context) : base(context)
        {
            GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<Coalesce.Domain.DateOnlyPk>();
        }

        [HttpGet("get/{id}")]
        [Authorize]
        public virtual Task<ItemResult<DateOnlyPkResponse>> Get(
            System.DateOnly id,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.DateOnlyPk> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [Authorize]
        public virtual Task<ListResult<DateOnlyPkResponse>> List(
            [FromQuery] ListParameters parameters,
            IDataSource<Coalesce.Domain.DateOnlyPk> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [Authorize]
        public virtual Task<ItemResult<int>> Count(
            [FromQuery] FilterParameters parameters,
            IDataSource<Coalesce.Domain.DateOnlyPk> dataSource)
            => CountImplementation(parameters, dataSource);

        [HttpPost("save")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        [Authorize]
        public virtual Task<ItemResult<DateOnlyPkResponse>> Save(
            [FromForm] DateOnlyPkParameter dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.DateOnlyPk> dataSource,
            IBehaviors<Coalesce.Domain.DateOnlyPk> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        [HttpPost("save")]
        [Consumes("application/json")]
        [Authorize]
        public virtual Task<ItemResult<DateOnlyPkResponse>> SaveFromJson(
            [FromBody] DateOnlyPkParameter dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.DateOnlyPk> dataSource,
            IBehaviors<Coalesce.Domain.DateOnlyPk> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        [HttpPost("bulkSave")]
        [Authorize]
        public virtual Task<ItemResult<DateOnlyPkResponse>> BulkSave(
            [FromBody] BulkSaveRequest dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.DateOnlyPk> dataSource,
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromServices] IBehaviorsFactory behaviorsFactory)
            => BulkSaveImplementation(dto, parameters, dataSource, dataSourceFactory, behaviorsFactory);

        [HttpPost("delete/{id}")]
        [Authorize]
        public virtual Task<ItemResult<DateOnlyPkResponse>> Delete(
            System.DateOnly id,
            IBehaviors<Coalesce.Domain.DateOnlyPk> behaviors,
            IDataSource<Coalesce.Domain.DateOnlyPk> dataSource)
            => DeleteImplementation(id, new DataSourceParameters(), dataSource, behaviors);
    }
}
