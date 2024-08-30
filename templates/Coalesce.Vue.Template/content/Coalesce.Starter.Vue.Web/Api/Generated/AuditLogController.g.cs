
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
    [Route("api/AuditLog")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class AuditLogController
        : BaseApiController<Coalesce.Starter.Vue.Data.Models.AuditLog, AuditLogParameter, AuditLogResponse, Coalesce.Starter.Vue.Data.AppDbContext>
    {
        public AuditLogController(CrudContext<Coalesce.Starter.Vue.Data.AppDbContext> context) : base(context)
        {
            GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<Coalesce.Starter.Vue.Data.Models.AuditLog>();
        }

        [HttpGet("get/{id}")]
        [Authorize(Roles = "ViewAuditLogs")]
        public virtual Task<ItemResult<AuditLogResponse>> Get(
            long id,
            DataSourceParameters parameters,
            IDataSource<Coalesce.Starter.Vue.Data.Models.AuditLog> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [Authorize(Roles = "ViewAuditLogs")]
        public virtual Task<ListResult<AuditLogResponse>> List(
            ListParameters parameters,
            IDataSource<Coalesce.Starter.Vue.Data.Models.AuditLog> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [Authorize(Roles = "ViewAuditLogs")]
        public virtual Task<ItemResult<int>> Count(
            FilterParameters parameters,
            IDataSource<Coalesce.Starter.Vue.Data.Models.AuditLog> dataSource)
            => CountImplementation(parameters, dataSource);

        [HttpPost("bulkSave")]
        [Authorize(Roles = "ViewAuditLogs")]
        public virtual Task<ItemResult<AuditLogResponse>> BulkSave(
            [FromBody] BulkSaveRequest dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Starter.Vue.Data.Models.AuditLog> dataSource,
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromServices] IBehaviorsFactory behaviorsFactory)
            => BulkSaveImplementation(dto, parameters, dataSource, dataSourceFactory, behaviorsFactory);
    }
}
