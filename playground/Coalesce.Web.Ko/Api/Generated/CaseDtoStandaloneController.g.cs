
using Coalesce.Web.Ko.Models;
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

namespace Coalesce.Web.Ko.Api
{
    [Route("api/CaseDtoStandalone")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class CaseDtoStandaloneController
        : BaseApiController<Coalesce.Domain.Case, Coalesce.Domain.CaseDtoStandalone, Coalesce.Domain.AppDbContext>
    {
        public CaseDtoStandaloneController(CrudContext<Coalesce.Domain.AppDbContext> context) : base(context)
        {
            GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<Coalesce.Domain.CaseDtoStandalone>();
        }

        [HttpGet("get/{id}")]
        [Authorize]
        public virtual Task<ItemResult<Coalesce.Domain.CaseDtoStandalone>> Get(
            int id,
            DataSourceParameters parameters,
            [DeclaredFor(typeof(Coalesce.Domain.CaseDtoStandalone))] IDataSource<Coalesce.Domain.Case> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [Authorize]
        public virtual Task<ListResult<Coalesce.Domain.CaseDtoStandalone>> List(
            ListParameters parameters,
            [DeclaredFor(typeof(Coalesce.Domain.CaseDtoStandalone))] IDataSource<Coalesce.Domain.Case> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [Authorize]
        public virtual Task<ItemResult<int>> Count(
            FilterParameters parameters,
            [DeclaredFor(typeof(Coalesce.Domain.CaseDtoStandalone))] IDataSource<Coalesce.Domain.Case> dataSource)
            => CountImplementation(parameters, dataSource);

        [HttpPost("save")]
        [Authorize]
        public virtual Task<ItemResult<Coalesce.Domain.CaseDtoStandalone>> Save(
            [FromForm] Coalesce.Domain.CaseDtoStandalone dto,
            [FromQuery] DataSourceParameters parameters,
            [DeclaredFor(typeof(Coalesce.Domain.CaseDtoStandalone))] IDataSource<Coalesce.Domain.Case> dataSource,
            [DeclaredFor(typeof(Coalesce.Domain.CaseDtoStandalone))] IBehaviors<Coalesce.Domain.Case> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        [HttpPost("bulkSave")]
        [Authorize]
        public virtual Task<ItemResult<Coalesce.Domain.CaseDtoStandalone>> BulkSave(
            [FromBody] BulkSaveRequest dto,
            [FromQuery] DataSourceParameters parameters,
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromServices] IBehaviorsFactory behaviorsFactory)
            => BulkSaveImplementation(dto, parameters, dataSourceFactory, behaviorsFactory);

        [HttpPost("delete/{id}")]
        [Authorize]
        public virtual Task<ItemResult<Coalesce.Domain.CaseDtoStandalone>> Delete(
            int id,
            [DeclaredFor(typeof(Coalesce.Domain.CaseDtoStandalone))] IBehaviors<Coalesce.Domain.Case> behaviors,
            [DeclaredFor(typeof(Coalesce.Domain.CaseDtoStandalone))] IDataSource<Coalesce.Domain.Case> dataSource)
            => DeleteImplementation(id, new DataSourceParameters(), dataSource, behaviors);
    }
}
