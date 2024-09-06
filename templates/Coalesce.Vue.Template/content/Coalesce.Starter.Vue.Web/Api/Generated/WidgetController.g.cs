
using Coalesce.Starter.Vue.Web.Models;
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

namespace Coalesce.Starter.Vue.Web.Api
{
    [Route("api/Widget")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class WidgetController
        : BaseApiController<Coalesce.Starter.Vue.Data.Models.Widget, WidgetParameter, WidgetResponse, Coalesce.Starter.Vue.Data.AppDbContext>
    {
        public WidgetController(CrudContext<Coalesce.Starter.Vue.Data.AppDbContext> context) : base(context)
        {
            GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<Coalesce.Starter.Vue.Data.Models.Widget>();
        }

        [HttpGet("get/{id}")]
        [Authorize]
        public virtual Task<ItemResult<WidgetResponse>> Get(
            int id,
            DataSourceParameters parameters,
            IDataSource<Coalesce.Starter.Vue.Data.Models.Widget> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [Authorize]
        public virtual Task<ListResult<WidgetResponse>> List(
            ListParameters parameters,
            IDataSource<Coalesce.Starter.Vue.Data.Models.Widget> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [Authorize]
        public virtual Task<ItemResult<int>> Count(
            FilterParameters parameters,
            IDataSource<Coalesce.Starter.Vue.Data.Models.Widget> dataSource)
            => CountImplementation(parameters, dataSource);

        [HttpPost("save")]
        [Authorize]
        public virtual Task<ItemResult<WidgetResponse>> Save(
            [FromForm] WidgetParameter dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Starter.Vue.Data.Models.Widget> dataSource,
            IBehaviors<Coalesce.Starter.Vue.Data.Models.Widget> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        [HttpPost("bulkSave")]
        [Authorize]
        public virtual Task<ItemResult<WidgetResponse>> BulkSave(
            [FromBody] BulkSaveRequest dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Starter.Vue.Data.Models.Widget> dataSource,
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromServices] IBehaviorsFactory behaviorsFactory)
            => BulkSaveImplementation(dto, parameters, dataSource, dataSourceFactory, behaviorsFactory);

        [HttpPost("delete/{id}")]
        [Authorize]
        public virtual Task<ItemResult<WidgetResponse>> Delete(
            int id,
            IBehaviors<Coalesce.Starter.Vue.Data.Models.Widget> behaviors,
            IDataSource<Coalesce.Starter.Vue.Data.Models.Widget> dataSource)
            => DeleteImplementation(id, new DataSourceParameters(), dataSource, behaviors);
    }
}
