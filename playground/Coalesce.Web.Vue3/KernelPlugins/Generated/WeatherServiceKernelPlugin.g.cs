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
using System.Threading.Tasks;

namespace Coalesce.Web.Vue3.KernelPlugins;
#pragma warning disable CS1998

public class IWeatherServiceKernelPlugin(CrudContext context, Coalesce.Domain.Services.IWeatherService _service, Coalesce.Domain.AppDbContext AppDbContext) : KernelPluginBase<Coalesce.Domain.Services.IWeatherService>(context)
{

    [KernelFunction("get_weather_async")]
    [Description("Returns weather data for the given location")]
    public async Task<string> GetWeatherAsync(
        LocationParameter location,
        System.DateTimeOffset? dateTime)
    {
        if (!_isScoped) return await InvokeScoped<string>(GetWeatherAsync, location, dateTime);

        return await Json(async () =>
        {
            var _method = GeneratedForClassViewModel!.MethodByName("GetWeatherAsync");
            if (!_method.SecurityInfo.IsExecuteAllowed(User)) return new ItemResult<WeatherDataResponse>(errorMessage: "Unauthorized");
            var _params = new
            {
                Location = location,
                DateTime = dateTime
            };
            var parameterDbContext = AppDbContext;

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
                if (!_validationResult.WasSuccessful) return new ItemResult<WeatherDataResponse>(_validationResult);
            }

            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = await _service.GetWeatherAsync(
                parameterDbContext,
                _params.Location?.MapToNew(_mappingContext),
                _params.DateTime
            );
            var _result = new ItemResult<WeatherDataResponse>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.Services.WeatherData, WeatherDataResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        });
    }
}
