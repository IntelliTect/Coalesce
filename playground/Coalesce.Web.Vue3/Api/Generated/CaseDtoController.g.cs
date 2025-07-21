
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

[Route("api/CaseDto")]
[Authorize]
[ServiceFilter(typeof(IApiActionFilter))]
public partial class CaseDtoController
    : BaseApiController<Coalesce.Domain.Case, Coalesce.Domain.CaseDto, Coalesce.Domain.CaseDto, Coalesce.Domain.AppDbContext>
{
    public CaseDtoController(CrudContext<Coalesce.Domain.AppDbContext> context) : base(context)
    {
        GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<Coalesce.Domain.CaseDto>();
    }

    [HttpGet("get/{id}")]
    [Authorize]
    public virtual Task<ItemResult<Coalesce.Domain.CaseDto>> Get(
        int id,
        [FromQuery] DataSourceParameters parameters,
        [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IDataSource<Coalesce.Domain.Case> dataSource)
        => GetImplementation(id, parameters, dataSource);

    [HttpGet("list")]
    [Authorize]
    public virtual Task<ListResult<Coalesce.Domain.CaseDto>> List(
        [FromQuery] ListParameters parameters,
        [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IDataSource<Coalesce.Domain.Case> dataSource)
        => ListImplementation(parameters, dataSource);

    [HttpGet("count")]
    [Authorize]
    public virtual Task<ItemResult<int>> Count(
        [FromQuery] FilterParameters parameters,
        [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IDataSource<Coalesce.Domain.Case> dataSource)
        => CountImplementation(parameters, dataSource);

    [HttpPost("save")]
    [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
    [Authorize]
    public virtual Task<ItemResult<Coalesce.Domain.CaseDto>> Save(
        [FromForm] Coalesce.Domain.CaseDto dto,
        [FromQuery] DataSourceParameters parameters,
        [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IDataSource<Coalesce.Domain.Case> dataSource,
        [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IBehaviors<Coalesce.Domain.Case> behaviors)
        => SaveImplementation(dto, parameters, dataSource, behaviors);

    [HttpPost("save")]
    [Consumes("application/json")]
    [Authorize]
    public virtual Task<ItemResult<Coalesce.Domain.CaseDto>> SaveFromJson(
        [FromBody] Coalesce.Domain.CaseDto dto,
        [FromQuery] DataSourceParameters parameters,
        [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IDataSource<Coalesce.Domain.Case> dataSource,
        [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IBehaviors<Coalesce.Domain.Case> behaviors)
        => SaveImplementation(dto, parameters, dataSource, behaviors);

    [HttpPost("bulkSave")]
    [Authorize]
    public virtual Task<ItemResult<Coalesce.Domain.CaseDto>> BulkSave(
        [FromBody] BulkSaveRequest dto,
        [FromQuery] DataSourceParameters parameters,
        [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IDataSource<Coalesce.Domain.Case> dataSource,
        [FromServices] IDataSourceFactory dataSourceFactory,
        [FromServices] IBehaviorsFactory behaviorsFactory)
        => BulkSaveImplementation(dto, parameters, dataSource, dataSourceFactory, behaviorsFactory);

    [HttpPost("delete/{id}")]
    [Authorize]
    public virtual Task<ItemResult<Coalesce.Domain.CaseDto>> Delete(
        int id,
        [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IBehaviors<Coalesce.Domain.Case> behaviors,
        [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IDataSource<Coalesce.Domain.Case> dataSource)
        => DeleteImplementation(id, new DataSourceParameters(), dataSource, behaviors);

    // Methods from data class exposed through API Controller.

    /// <summary>
    /// Method: AsyncMethodOnIClassDto
    /// </summary>
    [HttpPost("AsyncMethodOnIClassDto")]
    [Authorize]
    [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
    public virtual async Task<ItemResult<string>> AsyncMethodOnIClassDto(
        [FromServices] IDataSourceFactory dataSourceFactory,
        [FromForm(Name = "id")] int id,
        [FromForm(Name = "input")] string input)
    {
        var _method = GeneratedForClassViewModel!.MethodByName("AsyncMethodOnIClassDto");
        var _params = new
        {
            Id = id,
            Input = input
        };

        var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Case, Coalesce.Domain.CaseDto>("Default");
        var itemResult = await dataSource.GetMappedItemAsync<Coalesce.Domain.CaseDto>(_params.Id, new DataSourceParameters());
        if (!itemResult.WasSuccessful)
        {
            return new ItemResult<string>(itemResult);
        }
        var item = itemResult.Object;
        if (Context.Options.ValidateAttributesForMethods)
        {
            var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
            if (!_validationResult.WasSuccessful) return new ItemResult<string>(_validationResult);
        }

        var _methodResult = await item.AsyncMethodOnIClassDto(
            _params.Input
        );
        var _result = new ItemResult<string>();
        _result.Object = _methodResult;
        return _result;
    }

    public class AsyncMethodOnIClassDtoParameters
    {
        public int Id { get; set; }
        public string Input { get; set; }
    }

    /// <summary>
    /// Method: AsyncMethodOnIClassDto
    /// </summary>
    [HttpPost("AsyncMethodOnIClassDto")]
    [Authorize]
    [Consumes("application/json")]
    public virtual async Task<ItemResult<string>> AsyncMethodOnIClassDto(
        [FromServices] IDataSourceFactory dataSourceFactory,
        [FromBody] AsyncMethodOnIClassDtoParameters _params
    )
    {
        var _method = GeneratedForClassViewModel!.MethodByName("AsyncMethodOnIClassDto");
        var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Case, Coalesce.Domain.CaseDto>("Default");
        var itemResult = await dataSource.GetMappedItemAsync<Coalesce.Domain.CaseDto>(_params.Id, new DataSourceParameters());
        if (!itemResult.WasSuccessful)
        {
            return new ItemResult<string>(itemResult);
        }
        var item = itemResult.Object;
        if (Context.Options.ValidateAttributesForMethods)
        {
            var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
            if (!_validationResult.WasSuccessful) return new ItemResult<string>(_validationResult);
        }

        var _methodResult = await item.AsyncMethodOnIClassDto(
            _params.Input
        );
        var _result = new ItemResult<string>();
        _result.Object = _methodResult;
        return _result;
    }
}
