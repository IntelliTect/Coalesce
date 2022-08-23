
using Coalesce.Web.Vue.Models;
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

namespace Coalesce.Web.Vue.Api
{
    [Route("api/Product")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class ProductController
        : BaseApiController<Coalesce.Domain.Product, ProductDtoGen, Coalesce.Domain.AppDbContext>
    {
        public ProductController(Coalesce.Domain.AppDbContext db) : base(db)
        {
            GeneratedForClassViewModel = ReflectionRepository.Global.GetClassViewModel<Coalesce.Domain.Product>();
        }

        [HttpGet("get/{id}")]
        [Authorize]
        public virtual Task<ItemResult<ProductDtoGen>> Get(
            int id,
            DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.Product> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [Authorize]
        public virtual Task<ListResult<ProductDtoGen>> List(
            ListParameters parameters,
            IDataSource<Coalesce.Domain.Product> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [Authorize]
        public virtual Task<ItemResult<int>> Count(
            FilterParameters parameters,
            IDataSource<Coalesce.Domain.Product> dataSource)
            => CountImplementation(parameters, dataSource);

        [HttpPost("save")]
        [Authorize(Roles = "Admin")]
        public virtual Task<ItemResult<ProductDtoGen>> Save(
            ProductDtoGen dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.Product> dataSource,
            IBehaviors<Coalesce.Domain.Product> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        [HttpPost("delete/{id}")]
        [Authorize]
        public virtual Task<ItemResult<ProductDtoGen>> Delete(
            int id,
            IBehaviors<Coalesce.Domain.Product> behaviors,
            IDataSource<Coalesce.Domain.Product> dataSource)
            => DeleteImplementation(id, new DataSourceParameters(), dataSource, behaviors);
    }
}
