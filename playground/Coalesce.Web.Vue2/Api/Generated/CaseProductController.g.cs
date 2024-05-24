
using Coalesce.Web.Vue2.Models;
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

namespace Coalesce.Web.Vue2.Api
{
    [Route("api/CaseProduct")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class CaseProductController
        : BaseApiController<Coalesce.Domain.CaseProduct, CaseProductParameter, CaseProductResponse, Coalesce.Domain.AppDbContext>
    {
        public CaseProductController(CrudContext<Coalesce.Domain.AppDbContext> context) : base(context)
        {
            GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<Coalesce.Domain.CaseProduct>();
        }

        [HttpGet("get/{id}")]
        [Authorize]
        public virtual Task<ItemResult<CaseProductResponse>> Get(
            int id,
            DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.CaseProduct> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [Authorize]
        public virtual Task<ListResult<CaseProductResponse>> List(
            ListParameters parameters,
            IDataSource<Coalesce.Domain.CaseProduct> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [Authorize]
        public virtual Task<ItemResult<int>> Count(
            FilterParameters parameters,
            IDataSource<Coalesce.Domain.CaseProduct> dataSource)
            => CountImplementation(parameters, dataSource);

        [HttpPost("save")]
        [Authorize]
        public virtual Task<ItemResult<CaseProductResponse>> Save(
            [FromForm] CaseProductParameter dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.CaseProduct> dataSource,
            IBehaviors<Coalesce.Domain.CaseProduct> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        [HttpPost("bulkSave")]
        [Authorize]
        public virtual Task<ItemResult<CaseProductResponse>> BulkSave(
            [FromBody] BulkSaveRequest dto,
            [FromQuery] DataSourceParameters parameters,
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromServices] IBehaviorsFactory behaviorsFactory)
            => BulkSaveImplementation(dto, parameters, dataSourceFactory, behaviorsFactory);

        [HttpPost("delete/{id}")]
        [Authorize]
        public virtual Task<ItemResult<CaseProductResponse>> Delete(
            int id,
            IBehaviors<Coalesce.Domain.CaseProduct> behaviors,
            IDataSource<Coalesce.Domain.CaseProduct> dataSource)
            => DeleteImplementation(id, new DataSourceParameters(), dataSource, behaviors);
    }
}
