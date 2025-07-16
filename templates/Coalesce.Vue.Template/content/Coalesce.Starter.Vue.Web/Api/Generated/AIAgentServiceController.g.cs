
using Coalesce.Starter.Vue.Data.Services;
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
    [Route("api/AIAgentService")]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class AIAgentServiceController : BaseApiController
    {
        protected AIAgentService Service { get; }

        public AIAgentServiceController(CrudContext context, AIAgentService service) : base(context)
        {
            GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<AIAgentService>();
            Service = service;
        }

        /// <summary>
        /// Method: ChatAgent
        /// </summary>
        [HttpPost("ChatAgent")]
        [Authorize]
        [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
        public virtual async Task<ItemResult<ChatResponseResponse>> ChatAgent(
            [FromForm(Name = "cancellationToken")] System.Threading.CancellationToken cancellationToken,
            [FromForm(Name = "history")] string history,
            [FromForm(Name = "prompt")] string prompt)
        {
            var _params = new
            {
                History = history,
                Prompt = prompt
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("ChatAgent"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<ChatResponseResponse>(_validationResult);
            }

            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = await Service.ChatAgent(
                _params.History,
                _params.Prompt,
                cancellationToken
            );
            var _result = new ItemResult<ChatResponseResponse>();
            _result.Object = Mapper.MapToDto<AIAgentService.ChatResponse, ChatResponseResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        }

        public class ChatAgentParameters
        {
            public string History { get; set; }
            public string Prompt { get; set; }
        }

        /// <summary>
        /// Method: ChatAgent
        /// </summary>
        [HttpPost("ChatAgent")]
        [Authorize]
        [Consumes("application/json")]
        public virtual async Task<ItemResult<ChatResponseResponse>> ChatAgent(
            System.Threading.CancellationToken cancellationToken,
            [FromBody] ChatAgentParameters _params
        )
        {
            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("ChatAgent"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<ChatResponseResponse>(_validationResult);
            }

            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = await Service.ChatAgent(
                _params.History,
                _params.Prompt,
                cancellationToken
            );
            var _result = new ItemResult<ChatResponseResponse>();
            _result.Object = Mapper.MapToDto<AIAgentService.ChatResponse, ChatResponseResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        }
    }
}
