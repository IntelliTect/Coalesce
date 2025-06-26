using Coalesce.Web.Vue3.Models;
using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Api.Behaviors;
using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Web.Vue3.KernelPlugins;
#pragma warning disable CS1998

public class AIAgentServiceKernelPlugin(CrudContext context, IDataSourceFactory dsFactory, IBehaviorsFactory bhFactory, Coalesce.Domain.AIAgentService _service) : KernelPluginBase<Coalesce.Domain.AIAgentService>(context)
{

    [KernelFunction("PersonAgent")]
    [Description("An assistant who works with people and companies.")]
    public async Task<string> PersonAgent(
        System.Threading.CancellationToken cancellationToken,
        string prompt)
    {
        if (!_isScoped) return await InvokeScoped<string>(PersonAgent, cancellationToken, prompt);
        return await Json(async () =>
        {
            var _method = GeneratedForClassViewModel!.MethodByName("PersonAgent");
            if (!_method.SecurityInfo.IsExecuteAllowed(User)) return new ItemResult<ChatResponseResponse>(errorMessage: "Unauthorized");
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
            var _methodResult = await _service.PersonAgent(
                _params.Prompt,
                cancellationToken
            );
            var _result = new ItemResult<ChatResponseResponse>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.AIAgentService.ChatResponse, ChatResponseResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        });
    }

    [KernelFunction("ProductAgent")]
    [Description("An assistant who works with products.")]
    public async Task<string> ProductAgent(
        System.Threading.CancellationToken cancellationToken,
        string prompt)
    {
        if (!_isScoped) return await InvokeScoped<string>(ProductAgent, cancellationToken, prompt);
        return await Json(async () =>
        {
            var _method = GeneratedForClassViewModel!.MethodByName("ProductAgent");
            if (!_method.SecurityInfo.IsExecuteAllowed(User)) return new ItemResult<ChatResponseResponse>(errorMessage: "Unauthorized");
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
            var _methodResult = await _service.ProductAgent(
                _params.Prompt,
                cancellationToken
            );
            var _result = new ItemResult<ChatResponseResponse>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.AIAgentService.ChatResponse, ChatResponseResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        });
    }

}
