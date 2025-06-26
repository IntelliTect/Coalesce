
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
    [Route("api/AIAgentService")]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class AIAgentServiceController : BaseApiController
    {
        protected Coalesce.Domain.AIAgentService Service { get; }

        public AIAgentServiceController(CrudContext context, Coalesce.Domain.AIAgentService service) : base(context)
        {
            GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<Coalesce.Domain.AIAgentService>();
            Service = service;
        }

        /// <summary>
        /// Method: OrchestratedAgent
        /// </summary>
        [HttpPost("OrchestratedAgent")]
        [Authorize]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual async Task<ItemResult<ChatResponseResponse>> OrchestratedAgent(
            [FromForm(Name = "cancellationToken")] System.Threading.CancellationToken cancellationToken,
            [FromForm(Name = "history")] string history,
            [FromForm(Name = "prompt")] string prompt)
        {
            var _method = GeneratedForClassViewModel!.MethodByName("OrchestratedAgent");
            var _params = new
            {
                History = history,
                Prompt = prompt
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return new ItemResult<ChatResponseResponse>(_validationResult);
            }

            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = await Service.OrchestratedAgent(
                _params.History,
                _params.Prompt,
                cancellationToken
            );
            var _result = new ItemResult<ChatResponseResponse>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.AIAgentService.ChatResponse, ChatResponseResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        }

        public class OrchestratedAgentParameters
        {
            public string History { get; set; }
            public string Prompt { get; set; }
        }

        /// <summary>
        /// Method: OrchestratedAgent
        /// </summary>
        [HttpPost("OrchestratedAgent")]
        [Authorize]
        [Consumes("application/json")]
        public virtual async Task<ItemResult<ChatResponseResponse>> OrchestratedAgent(
            System.Threading.CancellationToken cancellationToken,
            [FromBody] OrchestratedAgentParameters _params
        )
        {
            var _method = GeneratedForClassViewModel!.MethodByName("OrchestratedAgent");
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return new ItemResult<ChatResponseResponse>(_validationResult);
            }

            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = await Service.OrchestratedAgent(
                _params.History,
                _params.Prompt,
                cancellationToken
            );
            var _result = new ItemResult<ChatResponseResponse>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.AIAgentService.ChatResponse, ChatResponseResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        }

        /// <summary>
        /// Method: MetaCompletionAgent
        /// </summary>
        [HttpPost("MetaCompletionAgent")]
        [Authorize]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual async Task<ItemResult<ChatResponseResponse>> MetaCompletionAgent(
            [FromForm(Name = "cancellationToken")] System.Threading.CancellationToken cancellationToken,
            [FromForm(Name = "history")] string history,
            [FromForm(Name = "prompt")] string prompt)
        {
            var _method = GeneratedForClassViewModel!.MethodByName("MetaCompletionAgent");
            var _params = new
            {
                History = history,
                Prompt = prompt
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return new ItemResult<ChatResponseResponse>(_validationResult);
            }

            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = await Service.MetaCompletionAgent(
                _params.History,
                _params.Prompt,
                cancellationToken
            );
            var _result = new ItemResult<ChatResponseResponse>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.AIAgentService.ChatResponse, ChatResponseResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        }

        public class MetaCompletionAgentParameters
        {
            public string History { get; set; }
            public string Prompt { get; set; }
        }

        /// <summary>
        /// Method: MetaCompletionAgent
        /// </summary>
        [HttpPost("MetaCompletionAgent")]
        [Authorize]
        [Consumes("application/json")]
        public virtual async Task<ItemResult<ChatResponseResponse>> MetaCompletionAgent(
            System.Threading.CancellationToken cancellationToken,
            [FromBody] MetaCompletionAgentParameters _params
        )
        {
            var _method = GeneratedForClassViewModel!.MethodByName("MetaCompletionAgent");
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return new ItemResult<ChatResponseResponse>(_validationResult);
            }

            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = await Service.MetaCompletionAgent(
                _params.History,
                _params.Prompt,
                cancellationToken
            );
            var _result = new ItemResult<ChatResponseResponse>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.AIAgentService.ChatResponse, ChatResponseResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        }

        /// <summary>
        /// Method: OmniToolAgent
        /// </summary>
        [HttpPost("OmniToolAgent")]
        [Authorize]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual async Task<ItemResult<ChatResponseResponse>> OmniToolAgent(
            [FromForm(Name = "cancellationToken")] System.Threading.CancellationToken cancellationToken,
            [FromForm(Name = "history")] string history,
            [FromForm(Name = "prompt")] string prompt)
        {
            var _method = GeneratedForClassViewModel!.MethodByName("OmniToolAgent");
            var _params = new
            {
                History = history,
                Prompt = prompt
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return new ItemResult<ChatResponseResponse>(_validationResult);
            }

            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = await Service.OmniToolAgent(
                _params.History,
                _params.Prompt,
                cancellationToken
            );
            var _result = new ItemResult<ChatResponseResponse>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.AIAgentService.ChatResponse, ChatResponseResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        }

        public class OmniToolAgentParameters
        {
            public string History { get; set; }
            public string Prompt { get; set; }
        }

        /// <summary>
        /// Method: OmniToolAgent
        /// </summary>
        [HttpPost("OmniToolAgent")]
        [Authorize]
        [Consumes("application/json")]
        public virtual async Task<ItemResult<ChatResponseResponse>> OmniToolAgent(
            System.Threading.CancellationToken cancellationToken,
            [FromBody] OmniToolAgentParameters _params
        )
        {
            var _method = GeneratedForClassViewModel!.MethodByName("OmniToolAgent");
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return new ItemResult<ChatResponseResponse>(_validationResult);
            }

            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = await Service.OmniToolAgent(
                _params.History,
                _params.Prompt,
                cancellationToken
            );
            var _result = new ItemResult<ChatResponseResponse>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.AIAgentService.ChatResponse, ChatResponseResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        }

        /// <summary>
        /// Method: PersonAgent
        /// </summary>
        [HttpPost("PersonAgent")]
        [Authorize]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual async Task<ItemResult<ChatResponseResponse>> PersonAgent(
            [FromForm(Name = "cancellationToken")] System.Threading.CancellationToken cancellationToken,
            [FromForm(Name = "prompt")] string prompt)
        {
            var _method = GeneratedForClassViewModel!.MethodByName("PersonAgent");
            var _params = new
            {
                Prompt = prompt
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return new ItemResult<ChatResponseResponse>(_validationResult);
            }

            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = await Service.PersonAgent(
                _params.Prompt,
                cancellationToken
            );
            var _result = new ItemResult<ChatResponseResponse>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.AIAgentService.ChatResponse, ChatResponseResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        }

        public class PersonAgentParameters
        {
            public string Prompt { get; set; }
        }

        /// <summary>
        /// Method: PersonAgent
        /// </summary>
        [HttpPost("PersonAgent")]
        [Authorize]
        [Consumes("application/json")]
        public virtual async Task<ItemResult<ChatResponseResponse>> PersonAgent(
            System.Threading.CancellationToken cancellationToken,
            [FromBody] PersonAgentParameters _params
        )
        {
            var _method = GeneratedForClassViewModel!.MethodByName("PersonAgent");
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return new ItemResult<ChatResponseResponse>(_validationResult);
            }

            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = await Service.PersonAgent(
                _params.Prompt,
                cancellationToken
            );
            var _result = new ItemResult<ChatResponseResponse>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.AIAgentService.ChatResponse, ChatResponseResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        }

        /// <summary>
        /// Method: ProductAgent
        /// </summary>
        [HttpPost("ProductAgent")]
        [Authorize]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual async Task<ItemResult<ChatResponseResponse>> ProductAgent(
            [FromForm(Name = "cancellationToken")] System.Threading.CancellationToken cancellationToken,
            [FromForm(Name = "prompt")] string prompt)
        {
            var _method = GeneratedForClassViewModel!.MethodByName("ProductAgent");
            var _params = new
            {
                Prompt = prompt
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return new ItemResult<ChatResponseResponse>(_validationResult);
            }

            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = await Service.ProductAgent(
                _params.Prompt,
                cancellationToken
            );
            var _result = new ItemResult<ChatResponseResponse>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.AIAgentService.ChatResponse, ChatResponseResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        }

        public class ProductAgentParameters
        {
            public string Prompt { get; set; }
        }

        /// <summary>
        /// Method: ProductAgent
        /// </summary>
        [HttpPost("ProductAgent")]
        [Authorize]
        [Consumes("application/json")]
        public virtual async Task<ItemResult<ChatResponseResponse>> ProductAgent(
            System.Threading.CancellationToken cancellationToken,
            [FromBody] ProductAgentParameters _params
        )
        {
            var _method = GeneratedForClassViewModel!.MethodByName("ProductAgent");
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return new ItemResult<ChatResponseResponse>(_validationResult);
            }

            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = await Service.ProductAgent(
                _params.Prompt,
                cancellationToken
            );
            var _result = new ItemResult<ChatResponseResponse>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.AIAgentService.ChatResponse, ChatResponseResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        }
    }
}
