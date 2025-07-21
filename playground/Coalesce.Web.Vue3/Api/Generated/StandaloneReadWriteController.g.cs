
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

namespace Coalesce.Web.Vue3.Api;

[Route("api/StandaloneReadWrite")]
[Authorize]
[ServiceFilter(typeof(IApiActionFilter))]
public partial class StandaloneReadWriteController
    : BaseApiController<Coalesce.Domain.StandaloneReadWrite, StandaloneReadWriteParameter, StandaloneReadWriteResponse>
{
    public StandaloneReadWriteController(CrudContext context) : base(context)
    {
        GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<Coalesce.Domain.StandaloneReadWrite>();
    }

    [HttpGet("get/{id}")]
    [Authorize]
    public virtual Task<ItemResult<StandaloneReadWriteResponse>> Get(
        int id,
        [FromQuery] DataSourceParameters parameters,
        IDataSource<Coalesce.Domain.StandaloneReadWrite> dataSource)
        => GetImplementation(id, parameters, dataSource);

    [HttpGet("list")]
    [Authorize]
    public virtual Task<ListResult<StandaloneReadWriteResponse>> List(
        [FromQuery] ListParameters parameters,
        IDataSource<Coalesce.Domain.StandaloneReadWrite> dataSource)
        => ListImplementation(parameters, dataSource);

    [HttpGet("count")]
    [Authorize]
    public virtual Task<ItemResult<int>> Count(
        [FromQuery] FilterParameters parameters,
        IDataSource<Coalesce.Domain.StandaloneReadWrite> dataSource)
        => CountImplementation(parameters, dataSource);

    [HttpPost("save")]
    [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
    [Authorize]
    public virtual Task<ItemResult<StandaloneReadWriteResponse>> Save(
        [FromForm] StandaloneReadWriteParameter dto,
        [FromQuery] DataSourceParameters parameters,
        IDataSource<Coalesce.Domain.StandaloneReadWrite> dataSource,
        IBehaviors<Coalesce.Domain.StandaloneReadWrite> behaviors)
        => SaveImplementation(dto, parameters, dataSource, behaviors);

    [HttpPost("save")]
    [Consumes("application/json")]
    [Authorize]
    public virtual Task<ItemResult<StandaloneReadWriteResponse>> SaveFromJson(
        [FromBody] StandaloneReadWriteParameter dto,
        [FromQuery] DataSourceParameters parameters,
        IDataSource<Coalesce.Domain.StandaloneReadWrite> dataSource,
        IBehaviors<Coalesce.Domain.StandaloneReadWrite> behaviors)
        => SaveImplementation(dto, parameters, dataSource, behaviors);

    [HttpPost("delete/{id}")]
    [Authorize]
    public virtual Task<ItemResult<StandaloneReadWriteResponse>> Delete(
        int id,
        IBehaviors<Coalesce.Domain.StandaloneReadWrite> behaviors,
        IDataSource<Coalesce.Domain.StandaloneReadWrite> dataSource)
        => DeleteImplementation(id, new DataSourceParameters(), dataSource, behaviors);
}
