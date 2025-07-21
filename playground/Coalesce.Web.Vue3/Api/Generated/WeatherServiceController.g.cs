
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

[Route("api/WeatherService")]
[ServiceFilter(typeof(IApiActionFilter))]
public partial class WeatherServiceController : BaseApiController
{
    protected Coalesce.Domain.Services.IWeatherService Service { get; }

    public WeatherServiceController(CrudContext context, Coalesce.Domain.Services.IWeatherService service) : base(context)
    {
        GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<Coalesce.Domain.Services.IWeatherService>();
        Service = service;
    }

    /// <summary>
    /// Method: GetWeatherAsync
    /// </summary>
    [HttpPost("GetWeather")]
    [HttpPost("GetWeatherAsync")]
    [Authorize]
    [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
    public virtual async Task<ItemResult<WeatherDataResponse>> GetWeather(
        [FromServices] Coalesce.Domain.AppDbContext parameterDbContext,
        [FromForm(Name = "location")] LocationParameter location,
        [FromForm(Name = "dateTime")] System.DateTimeOffset? dateTime)
    {
        var _method = GeneratedForClassViewModel!.MethodByName("GetWeatherAsync");
        var _params = new
        {
            Location = !Request.Form.HasAnyValue("location") ? null : location,
            DateTime = dateTime
        };

        if (Context.Options.ValidateAttributesForMethods)
        {
            var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
            if (!_validationResult.WasSuccessful) return new ItemResult<WeatherDataResponse>(_validationResult);
        }

        IncludeTree includeTree = null;
        var _mappingContext = new MappingContext(Context);
        var _methodResult = await Service.GetWeatherAsync(
            parameterDbContext,
            _params.Location?.MapToNew(_mappingContext),
            _params.DateTime
        );
        var _result = new ItemResult<WeatherDataResponse>();
        _result.Object = Mapper.MapToDto<Coalesce.Domain.Services.WeatherData, WeatherDataResponse>(_methodResult, _mappingContext, includeTree);
        return _result;
    }

    public class GetWeatherParameters
    {
        public LocationParameter Location { get; set; }
        public System.DateTimeOffset? DateTime { get; set; }
    }

    /// <summary>
    /// Method: GetWeatherAsync
    /// </summary>
    [HttpPost("GetWeather")]
    [HttpPost("GetWeatherAsync")]
    [Authorize]
    [Consumes("application/json")]
    public virtual async Task<ItemResult<WeatherDataResponse>> GetWeather(
        [FromServices] Coalesce.Domain.AppDbContext parameterDbContext,
        [FromBody] GetWeatherParameters _params
    )
    {
        var _method = GeneratedForClassViewModel!.MethodByName("GetWeatherAsync");
        if (Context.Options.ValidateAttributesForMethods)
        {
            var _validationResult = ItemResult.FromParameterValidation(_method, _params, ServiceProvider);
            if (!_validationResult.WasSuccessful) return new ItemResult<WeatherDataResponse>(_validationResult);
        }

        IncludeTree includeTree = null;
        var _mappingContext = new MappingContext(Context);
        var _methodResult = await Service.GetWeatherAsync(
            parameterDbContext,
            _params.Location?.MapToNew(_mappingContext),
            _params.DateTime
        );
        var _result = new ItemResult<WeatherDataResponse>();
        _result.Object = Mapper.MapToDto<Coalesce.Domain.Services.WeatherData, WeatherDataResponse>(_methodResult, _mappingContext, includeTree);
        return _result;
    }
}
