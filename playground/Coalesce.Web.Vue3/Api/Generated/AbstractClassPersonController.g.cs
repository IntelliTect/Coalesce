
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

[Route("api/AbstractClassPerson")]
[Authorize]
[ServiceFilter(typeof(IApiActionFilter))]
public partial class AbstractClassPersonController
    : BaseApiController<Coalesce.Domain.AbstractClassPerson, AbstractClassPersonParameter, AbstractClassPersonResponse, Coalesce.Domain.AppDbContext>
{
    public AbstractClassPersonController(CrudContext<Coalesce.Domain.AppDbContext> context) : base(context)
    {
        GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<Coalesce.Domain.AbstractClassPerson>();
    }

    [HttpGet("get/{id}")]
    [Authorize]
    public virtual Task<ItemResult<AbstractClassPersonResponse>> Get(
        int id,
        [FromQuery] DataSourceParameters parameters,
        IDataSource<Coalesce.Domain.AbstractClassPerson> dataSource)
        => GetImplementation(id, parameters, dataSource);

    [HttpGet("list")]
    [Authorize]
    public virtual Task<ListResult<AbstractClassPersonResponse>> List(
        [FromQuery] ListParameters parameters,
        IDataSource<Coalesce.Domain.AbstractClassPerson> dataSource)
        => ListImplementation(parameters, dataSource);

    [HttpGet("count")]
    [Authorize]
    public virtual Task<ItemResult<int>> Count(
        [FromQuery] FilterParameters parameters,
        IDataSource<Coalesce.Domain.AbstractClassPerson> dataSource)
        => CountImplementation(parameters, dataSource);

    [HttpPost("save")]
    [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
    [Authorize]
    public virtual Task<ItemResult<AbstractClassPersonResponse>> Save(
        [FromForm] AbstractClassPersonParameter dto,
        [FromQuery] DataSourceParameters parameters,
        IDataSource<Coalesce.Domain.AbstractClassPerson> dataSource,
        IBehaviors<Coalesce.Domain.AbstractClassPerson> behaviors)
        => SaveImplementation(dto, parameters, dataSource, behaviors);

    [HttpPost("save")]
    [Consumes("application/json")]
    [Authorize]
    public virtual Task<ItemResult<AbstractClassPersonResponse>> SaveFromJson(
        [FromBody] AbstractClassPersonParameter dto,
        [FromQuery] DataSourceParameters parameters,
        IDataSource<Coalesce.Domain.AbstractClassPerson> dataSource,
        IBehaviors<Coalesce.Domain.AbstractClassPerson> behaviors)
        => SaveImplementation(dto, parameters, dataSource, behaviors);

    [HttpPost("bulkSave")]
    [Authorize]
    public virtual Task<ItemResult<AbstractClassPersonResponse>> BulkSave(
        [FromBody] BulkSaveRequest dto,
        [FromQuery] DataSourceParameters parameters,
        IDataSource<Coalesce.Domain.AbstractClassPerson> dataSource,
        [FromServices] IDataSourceFactory dataSourceFactory,
        [FromServices] IBehaviorsFactory behaviorsFactory)
        => BulkSaveImplementation(dto, parameters, dataSource, dataSourceFactory, behaviorsFactory);

    [HttpPost("delete/{id}")]
    [Authorize]
    public virtual Task<ItemResult<AbstractClassPersonResponse>> Delete(
        int id,
        IBehaviors<Coalesce.Domain.AbstractClassPerson> behaviors,
        IDataSource<Coalesce.Domain.AbstractClassPerson> dataSource)
        => DeleteImplementation(id, new DataSourceParameters(), dataSource, behaviors);
}
