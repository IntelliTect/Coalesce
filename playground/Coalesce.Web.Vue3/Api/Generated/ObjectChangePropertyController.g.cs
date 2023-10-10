
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

namespace Coalesce.Web.Vue3.Api
{
    [Route("api/ObjectChangeProperty")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class ObjectChangePropertyController
        : BaseApiController<IntelliTect.Coalesce.AuditLogging.ObjectChangeProperty, ObjectChangePropertyDtoGen, Coalesce.Domain.AppDbContext>
    {
        public ObjectChangePropertyController(CrudContext<Coalesce.Domain.AppDbContext> context) : base(context)
        {
            GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<IntelliTect.Coalesce.AuditLogging.ObjectChangeProperty>();
        }

        [HttpGet("get/{id}")]
        [Authorize]
        public virtual Task<ItemResult<ObjectChangePropertyDtoGen>> Get(
            long id,
            DataSourceParameters parameters,
            IDataSource<IntelliTect.Coalesce.AuditLogging.ObjectChangeProperty> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [Authorize]
        public virtual Task<ListResult<ObjectChangePropertyDtoGen>> List(
            ListParameters parameters,
            IDataSource<IntelliTect.Coalesce.AuditLogging.ObjectChangeProperty> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [Authorize]
        public virtual Task<ItemResult<int>> Count(
            FilterParameters parameters,
            IDataSource<IntelliTect.Coalesce.AuditLogging.ObjectChangeProperty> dataSource)
            => CountImplementation(parameters, dataSource);

        [HttpPost("bulkSave")]
        [Authorize]
        public virtual Task<ItemResult<ObjectChangePropertyDtoGen>> BulkSave(
            [FromBody] BulkSaveRequest dto,
            [FromQuery] DataSourceParameters parameters,
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromServices] IBehaviorsFactory behaviorsFactory)
            => BulkSaveImplementation(dto, parameters, dataSourceFactory, behaviorsFactory);
    }
}
