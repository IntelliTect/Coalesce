
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
    [Route("api/PasskeyService")]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class PasskeyServiceController : BaseApiController
    {
        protected Coalesce.Starter.Vue.Data.Auth.PasskeyService Service { get; }

        public PasskeyServiceController(CrudContext context, Coalesce.Starter.Vue.Data.Auth.PasskeyService service) : base(context)
        {
            GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<Coalesce.Starter.Vue.Data.Auth.PasskeyService>();
            Service = service;
        }

        /// <summary>
        /// Method: GetRequestOptions
        /// </summary>
        [HttpPost("GetRequestOptions")]
        [AllowAnonymous]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual async Task<ItemResult<string>> GetRequestOptions(
            [FromForm(Name = "username")] string username = default)
        {
            var _method = GeneratedForClassViewModel!.MethodByName("GetRequestOptions");
            var _params = new
            {
                Username = username
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return new ItemResult<string>(_validationResult);
            }

            var _methodResult = await Service.GetRequestOptions(
                _params.Username
            );
            var _result = new ItemResult<string>();
            _result.Object = _methodResult;
            return _result;
        }

        public class GetRequestOptionsParameters
        {
            public string Username { get; set; } = default;
        }

        /// <summary>
        /// Method: GetRequestOptions
        /// </summary>
        [HttpPost("GetRequestOptions")]
        [AllowAnonymous]
        [Consumes("application/json")]
        public virtual async Task<ItemResult<string>> GetRequestOptions(
            [FromBody] GetRequestOptionsParameters _params
        )
        {
            var _method = GeneratedForClassViewModel!.MethodByName("GetRequestOptions");
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return new ItemResult<string>(_validationResult);
            }

            var _methodResult = await Service.GetRequestOptions(
                _params.Username
            );
            var _result = new ItemResult<string>();
            _result.Object = _methodResult;
            return _result;
        }

        /// <summary>
        /// Method: GetCreationOptions
        /// </summary>
        [HttpPost("GetCreationOptions")]
        [Authorize]
        public virtual async Task<ItemResult<string>> GetCreationOptions()
        {
            var _method = GeneratedForClassViewModel!.MethodByName("GetCreationOptions");
            var _methodResult = await Service.GetCreationOptions(
                User
            );
            var _result = new ItemResult<string>();
            _result.Object = _methodResult;
            return _result;
        }

        /// <summary>
        /// Method: GetPasskeys
        /// </summary>
        [HttpPost("GetPasskeys")]
        [Authorize]
        public virtual async Task<ItemResult<System.Collections.Generic.ICollection<UserPasskeyInfoResponse>>> GetPasskeys()
        {
            var _method = GeneratedForClassViewModel!.MethodByName("GetPasskeys");
            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = await Service.GetPasskeys(
                User
            );
            var _result = new ItemResult<System.Collections.Generic.ICollection<UserPasskeyInfoResponse>>();
            _result.Object = _methodResult?.ToList().Select(o => Mapper.MapToDto<Coalesce.Starter.Vue.Data.Auth.UserPasskeyInfo, UserPasskeyInfoResponse>(o, _mappingContext, includeTree)).ToList();
            return _result;
        }

        /// <summary>
        /// Method: AddPasskey
        /// </summary>
        [HttpPost("AddPasskey")]
        [Authorize]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual async Task<ItemResult> AddPasskey(
            [FromForm(Name = "credentialJson")] string credentialJson,
            [FromForm(Name = "name")] string name = default)
        {
            var _method = GeneratedForClassViewModel!.MethodByName("AddPasskey");
            var _params = new
            {
                CredentialJson = credentialJson,
                Name = name
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return _validationResult;
            }

            var _methodResult = await Service.AddPasskey(
                User,
                _params.CredentialJson,
                _params.Name
            );
            var _result = new ItemResult(_methodResult);
            return _result;
        }

        public class AddPasskeyParameters
        {
            public string CredentialJson { get; set; }
            public string Name { get; set; } = default;
        }

        /// <summary>
        /// Method: AddPasskey
        /// </summary>
        [HttpPost("AddPasskey")]
        [Authorize]
        [Consumes("application/json")]
        public virtual async Task<ItemResult> AddPasskey(
            [FromBody] AddPasskeyParameters _params
        )
        {
            var _method = GeneratedForClassViewModel!.MethodByName("AddPasskey");
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return _validationResult;
            }

            var _methodResult = await Service.AddPasskey(
                User,
                _params.CredentialJson,
                _params.Name
            );
            var _result = new ItemResult(_methodResult);
            return _result;
        }

        /// <summary>
        /// Method: RenamePasskey
        /// </summary>
        [HttpPost("RenamePasskey")]
        [Authorize]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual async Task<ItemResult> RenamePasskey(
            [FromForm(Name = "credentialId")] byte[] credentialId,
            [FromForm(Name = "name")] string name)
        {
            var _method = GeneratedForClassViewModel!.MethodByName("RenamePasskey");
            var _params = new
            {
                CredentialId = credentialId ?? await ((await Request.ReadFormAsync()).Files["credentialId"]?.OpenReadStream().ReadAllBytesAsync(true) ?? Task.FromResult<byte[]>(null)),
                Name = name
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return _validationResult;
            }

            var _methodResult = await Service.RenamePasskey(
                User,
                _params.CredentialId,
                _params.Name
            );
            var _result = new ItemResult(_methodResult);
            return _result;
        }

        public class RenamePasskeyParameters
        {
            public byte[] CredentialId { get; set; }
            public string Name { get; set; }
        }

        /// <summary>
        /// Method: RenamePasskey
        /// </summary>
        [HttpPost("RenamePasskey")]
        [Authorize]
        [Consumes("application/json")]
        public virtual async Task<ItemResult> RenamePasskey(
            [FromBody] RenamePasskeyParameters _params
        )
        {
            var _method = GeneratedForClassViewModel!.MethodByName("RenamePasskey");
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return _validationResult;
            }

            var _methodResult = await Service.RenamePasskey(
                User,
                _params.CredentialId,
                _params.Name
            );
            var _result = new ItemResult(_methodResult);
            return _result;
        }

        /// <summary>
        /// Method: DeletePasskey
        /// </summary>
        [HttpPost("DeletePasskey")]
        [Authorize]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual async Task<ItemResult> DeletePasskey(
            [FromForm(Name = "credentialId")] byte[] credentialId)
        {
            var _method = GeneratedForClassViewModel!.MethodByName("DeletePasskey");
            var _params = new
            {
                CredentialId = credentialId ?? await ((await Request.ReadFormAsync()).Files["credentialId"]?.OpenReadStream().ReadAllBytesAsync(true) ?? Task.FromResult<byte[]>(null))
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return _validationResult;
            }

            var _methodResult = await Service.DeletePasskey(
                User,
                _params.CredentialId
            );
            var _result = new ItemResult(_methodResult);
            return _result;
        }

        public class DeletePasskeyParameters
        {
            public byte[] CredentialId { get; set; }
        }

        /// <summary>
        /// Method: DeletePasskey
        /// </summary>
        [HttpPost("DeletePasskey")]
        [Authorize]
        [Consumes("application/json")]
        public virtual async Task<ItemResult> DeletePasskey(
            [FromBody] DeletePasskeyParameters _params
        )
        {
            var _method = GeneratedForClassViewModel!.MethodByName("DeletePasskey");
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return _validationResult;
            }

            var _methodResult = await Service.DeletePasskey(
                User,
                _params.CredentialId
            );
            var _result = new ItemResult(_methodResult);
            return _result;
        }
    }
}
