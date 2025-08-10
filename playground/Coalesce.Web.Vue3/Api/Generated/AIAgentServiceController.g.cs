
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

namespace Coalesce.Web.Vue3.Api;

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
    /// Method: ChatAgent
    /// </summary>
    [HttpPost("ChatAgent")]
    [Authorize]
    [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
    public virtual async Task<ItemResult<ChatResponseResponse>> ChatAgent(
        System.Threading.CancellationToken cancellationToken,
        [FromForm(Name = "history")] string history,
        [FromForm(Name = "prompt")] string prompt)
    {
        var _method = GeneratedForClassViewModel!.MethodByName("ChatAgent");
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
        var _methodResult = await Service.ChatAgent(
            _params.History,
            _params.Prompt,
            cancellationToken
        );
        var _result = new ItemResult<ChatResponseResponse>();
        _result.Object = Mapper.MapToDto<Coalesce.Domain.AIAgentService.ChatResponse, ChatResponseResponse>(_methodResult, _mappingContext, includeTree);
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
        var _method = GeneratedForClassViewModel!.MethodByName("ChatAgent");
        if (Context.Options.ValidateAttributesForMethods)
        {
            var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
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
        _result.Object = Mapper.MapToDto<Coalesce.Domain.AIAgentService.ChatResponse, ChatResponseResponse>(_methodResult, _mappingContext, includeTree);
        return _result;
    }
}
