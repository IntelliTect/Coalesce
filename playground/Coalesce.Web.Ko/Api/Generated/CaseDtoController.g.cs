
using Coalesce.Web.Ko.Models;
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

namespace Coalesce.Web.Ko.Api
{
    [Route("api/CaseDto")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class CaseDtoController
        : BaseApiController<Coalesce.Domain.Case, Coalesce.Domain.CaseDto, Coalesce.Domain.AppDbContext>
    {
        public CaseDtoController(Coalesce.Domain.AppDbContext db) : base(db)
        {
            GeneratedForClassViewModel = ReflectionRepository.Global.GetClassViewModel<Coalesce.Domain.CaseDto>();
        }

        [HttpGet("get/{id}")]
        [Authorize]
        public virtual Task<ItemResult<Coalesce.Domain.CaseDto>> Get(
            int id,
            DataSourceParameters parameters,
            [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IDataSource<Coalesce.Domain.Case> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [Authorize]
        public virtual Task<ListResult<Coalesce.Domain.CaseDto>> List(
            ListParameters parameters,
            [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IDataSource<Coalesce.Domain.Case> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [Authorize]
        public virtual Task<ItemResult<int>> Count(
            FilterParameters parameters,
            [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IDataSource<Coalesce.Domain.Case> dataSource)
            => CountImplementation(parameters, dataSource);

        [HttpPost("save")]
        [Authorize]
        public virtual Task<ItemResult<Coalesce.Domain.CaseDto>> Save(
            [FromForm] Coalesce.Domain.CaseDto dto,
            [FromQuery] DataSourceParameters parameters,
            [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IDataSource<Coalesce.Domain.Case> dataSource,
            [DeclaredFor(typeof(Coalesce.Domain.CaseDto))] IBehaviors<Coalesce.Domain.Case> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

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
        public virtual async Task<ItemResult<string>> AsyncMethodOnIClassDto(
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromForm(Name = "id")] int id,
            [FromForm(Name = "input")] string input)
        {
            var dataSource = dataSourceFactory.GetDataSource<Coalesce.Domain.Case, Coalesce.Domain.CaseDto>("Default");
            var itemResult = await dataSource.GetMappedItemAsync<Coalesce.Domain.CaseDto>(id, new DataSourceParameters());
            if (!itemResult.WasSuccessful)
            {
                return new ItemResult<string>(itemResult);
            }
            var item = itemResult.Object;
            var _methodResult = await item.AsyncMethodOnIClassDto(input);
            await Db.SaveChangesAsync();
            var _result = new ItemResult<string>();
            _result.Object = _methodResult;
            return _result;
        }
    }
}
