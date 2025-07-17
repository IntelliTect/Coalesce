using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Api.Behaviors;
using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.SemanticKernel;
using MyProject.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyProject.KernelPlugins;
#pragma warning disable CS1998

public class IWeatherServiceKernelPlugin(CrudContext context, IDataSourceFactory dataSourceFactory, IBehaviorsFactory behaviorsFactory, IntelliTect.Coalesce.Tests.TargetClasses.IWeatherService _service, IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.AppDbContext AppDbContext) : KernelPluginBase<IntelliTect.Coalesce.Tests.TargetClasses.IWeatherService>(context)
{

    [KernelFunction("get_weather_async")]
    [Description("GetWeatherAsync")]
    public async Task<string> GetWeatherAsync(
        [Description("The location where weather data should be determined")]
        LocationParameter location,
        System.DateTimeOffset? dateTime,
        IntelliTect.Coalesce.Tests.TargetClasses.SkyConditions? conditions)
    {
        if (!_isScoped) return await InvokeScoped<string>(GetWeatherAsync, location, dateTime, conditions);

        return await Json(async () =>
        {
            var _method = GeneratedForClassViewModel!.MethodByName("GetWeatherAsync");
            if (!_method.SecurityInfo.IsExecuteAllowed(User)) return new ItemResult<WeatherDataResponse>(errorMessage: "Unauthorized");
            var _params = new
            {
                Location = location,
                DateTime = dateTime,
                Conditions = conditions
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
                _params.DateTime,
                _params.Conditions
            );
            var _result = new ItemResult<WeatherDataResponse>();
            _result.Object = Mapper.MapToDto<IntelliTect.Coalesce.Tests.TargetClasses.WeatherData, WeatherDataResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        });
    }
}
