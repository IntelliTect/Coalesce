
using Coalesce.Web.Models;
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
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Coalesce.Web.Api
{
    [Route("api/StandaloneReadonly")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class StandaloneReadonlyController
        : BaseApiController<Coalesce.Domain.StandaloneReadonly, StandaloneReadonlyDtoGen>
    {
        public StandaloneReadonlyController() : base()
        {
            GeneratedForClassViewModel = ReflectionRepository.Global.GetClassViewModel<Coalesce.Domain.StandaloneReadonly>();
        }

        [HttpGet("get/{id}")]
        [Authorize]
        public virtual Task<ItemResult<StandaloneReadonlyDtoGen>> Get(
            int id,
            DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.StandaloneReadonly> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [Authorize]
        public virtual Task<ListResult<StandaloneReadonlyDtoGen>> List(
            ListParameters parameters,
            IDataSource<Coalesce.Domain.StandaloneReadonly> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [Authorize]
        public virtual Task<ItemResult<int>> Count(
            FilterParameters parameters,
            IDataSource<Coalesce.Domain.StandaloneReadonly> dataSource)
            => CountImplementation(parameters, dataSource);
    }
}
