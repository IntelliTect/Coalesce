
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
    [Route("api/StandaloneReadWrite")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class StandaloneReadWriteController
        : BaseApiController<Coalesce.Domain.StandaloneReadWrite, StandaloneReadWriteDtoGen>
    {
        public StandaloneReadWriteController() : base()
        {
            GeneratedForClassViewModel = ReflectionRepository.Global.GetClassViewModel<Coalesce.Domain.StandaloneReadWrite>();
        }

        [HttpGet("get/{id}")]
        [Authorize]
        public virtual Task<ItemResult<StandaloneReadWriteDtoGen>> Get(
            int id,
            DataSourceParameters parameters,
            IDataSource<Coalesce.Domain.StandaloneReadWrite> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [Authorize]
        public virtual Task<ListResult<StandaloneReadWriteDtoGen>> List(
            ListParameters parameters,
            IDataSource<Coalesce.Domain.StandaloneReadWrite> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [Authorize]
        public virtual Task<ItemResult<int>> Count(
            FilterParameters parameters,
            IDataSource<Coalesce.Domain.StandaloneReadWrite> dataSource)
            => CountImplementation(parameters, dataSource);
    }
}
