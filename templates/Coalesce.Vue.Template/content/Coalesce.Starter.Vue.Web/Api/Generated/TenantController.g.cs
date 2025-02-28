
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
    [Route("api/Tenant")]
    [Authorize]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class TenantController
        : BaseApiController<Coalesce.Starter.Vue.Data.Models.Tenant, TenantParameter, TenantResponse, Coalesce.Starter.Vue.Data.AppDbContext>
    {
        public TenantController(CrudContext<Coalesce.Starter.Vue.Data.AppDbContext> context) : base(context)
        {
            GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<Coalesce.Starter.Vue.Data.Models.Tenant>();
        }

        [HttpGet("get/{id}")]
        [Authorize]
        public virtual Task<ItemResult<TenantResponse>> Get(
            string id,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Starter.Vue.Data.Models.Tenant> dataSource)
            => GetImplementation(id, parameters, dataSource);

        [HttpGet("list")]
        [Authorize]
        public virtual Task<ListResult<TenantResponse>> List(
            [FromQuery] ListParameters parameters,
            IDataSource<Coalesce.Starter.Vue.Data.Models.Tenant> dataSource)
            => ListImplementation(parameters, dataSource);

        [HttpGet("count")]
        [Authorize]
        public virtual Task<ItemResult<int>> Count(
            [FromQuery] FilterParameters parameters,
            IDataSource<Coalesce.Starter.Vue.Data.Models.Tenant> dataSource)
            => CountImplementation(parameters, dataSource);

        [HttpPost("save")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        [Authorize]
        public virtual Task<ItemResult<TenantResponse>> Save(
            [FromForm] TenantParameter dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Starter.Vue.Data.Models.Tenant> dataSource,
            IBehaviors<Coalesce.Starter.Vue.Data.Models.Tenant> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        [HttpPost("save")]
        [Consumes("application/json")]
        [Authorize]
        public virtual Task<ItemResult<TenantResponse>> SaveFromJson(
            [FromBody] TenantParameter dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Starter.Vue.Data.Models.Tenant> dataSource,
            IBehaviors<Coalesce.Starter.Vue.Data.Models.Tenant> behaviors)
            => SaveImplementation(dto, parameters, dataSource, behaviors);

        [HttpPost("bulkSave")]
        [Authorize]
        public virtual Task<ItemResult<TenantResponse>> BulkSave(
            [FromBody] BulkSaveRequest dto,
            [FromQuery] DataSourceParameters parameters,
            IDataSource<Coalesce.Starter.Vue.Data.Models.Tenant> dataSource,
            [FromServices] IDataSourceFactory dataSourceFactory,
            [FromServices] IBehaviorsFactory behaviorsFactory)
            => BulkSaveImplementation(dto, parameters, dataSource, dataSourceFactory, behaviorsFactory);

        // Methods from data class exposed through API Controller.

        /// <summary>
        /// Method: Create
        /// </summary>
        [HttpPost("Create")]
        [Authorize(Roles = "GlobalAdmin")]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual async Task<ItemResult> Create(
            [FromServices] Coalesce.Starter.Vue.Data.Auth.InvitationService invitationService,
            [FromForm(Name = "name")] string name,
            [FromForm(Name = "adminEmail")] string adminEmail)
        {
            var _params = new
            {
                Name = name,
                AdminEmail = adminEmail
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("Create"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return _validationResult;
            }

            var _methodResult = await Coalesce.Starter.Vue.Data.Models.Tenant.Create(
                Db,
                invitationService,
                _params.Name,
                _params.AdminEmail
            );
            var _result = new ItemResult(_methodResult);
            return _result;
        }

        public class CreateParameters
        {
            public string Name { get; set; }
            public string AdminEmail { get; set; }
        }

        /// <summary>
        /// Method: Create
        /// </summary>
        [HttpPost("Create")]
        [Authorize(Roles = "GlobalAdmin")]
        [Consumes("application/json")]
        public virtual async Task<ItemResult> Create(
            [FromServices] Coalesce.Starter.Vue.Data.Auth.InvitationService invitationService,
            [FromBody] CreateParameters _params
        )
        {
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("Create"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return _validationResult;
            }

            var _methodResult = await Coalesce.Starter.Vue.Data.Models.Tenant.Create(
                Db,
                invitationService,
                _params.Name,
                _params.AdminEmail
            );
            var _result = new ItemResult(_methodResult);
            return _result;
        }
    }
}
