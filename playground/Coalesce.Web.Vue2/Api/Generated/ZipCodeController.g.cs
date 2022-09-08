
using Coalesce.Web.Vue2.Models;
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

namespace Coalesce.Web.Vue2.Api
{
    [Route("api/ZipCode")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class ZipCodeController
        : BaseApiController<Coalesce.Domain.ZipCode, ZipCodeDtoGen, Coalesce.Domain.AppDbContext>
    {
        public ZipCodeController(Coalesce.Domain.AppDbContext db) : base(db)
        {
            GeneratedForClassViewModel = ReflectionRepository.Global.GetClassViewModel<Coalesce.Domain.ZipCode>();
        }

        [HttpGet("get/{id}")]
        [Authorize]
        public virtual Task<ItemResult<ZipCodeDtoGen>> Get(
            string id,
            DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.ZipCode> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [Authorize]
        public virtual Task<ListResult<ZipCodeDtoGen>> List(
            ListParameters parameters,
            IDataSource<Coalesce.Domain.ZipCode> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [Authorize]
        public virtual Task<ItemResult<int>> Count(
            FilterParameters parameters,
            IDataSource<Coalesce.Domain.ZipCode> dataSource)
            => CountImplementation(parameters, dataSource);

        [HttpPost("save")]
        [Authorize]
        public virtual Task<ItemResult<ZipCodeDtoGen>> Save(
            [FromForm] ZipCodeDtoGen dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.ZipCode> dataSource,
            IBehaviors<Coalesce.Domain.ZipCode> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        [HttpPost("delete/{id}")]
        [Authorize]
        public virtual Task<ItemResult<ZipCodeDtoGen>> Delete(
            string id,
            IBehaviors<Coalesce.Domain.ZipCode> behaviors,
            IDataSource<Coalesce.Domain.ZipCode> dataSource)
            => DeleteImplementation(id, new DataSourceParameters(), dataSource, behaviors);
    }
}
