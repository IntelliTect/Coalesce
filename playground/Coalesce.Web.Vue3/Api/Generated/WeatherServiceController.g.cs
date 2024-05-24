
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
    [Route("api/WeatherService")]
    [ServiceFilter(typeof(IApiActionFilter))]
    public partial class WeatherServiceController : Controller
    {
        protected ClassViewModel GeneratedForClassViewModel { get; }
        protected Coalesce.Domain.Services.IWeatherService Service { get; }
        protected CrudContext Context { get; }

        public WeatherServiceController(CrudContext context, Coalesce.Domain.Services.IWeatherService service)
        {
            GeneratedForClassViewModel = context.ReflectionRepository.GetClassViewModel<Coalesce.Domain.Services.IWeatherService>();
            Service = service;
            Context = context;
        }

        /// <summary>
        /// Method: GetWeatherAsync
        /// </summary>
        [HttpPost("GetWeather")]
        [HttpPost("GetWeatherAsync")]
        [Authorize]
        public virtual async Task<ItemResult<WeatherData>> GetWeather(
            [FromServices] Coalesce.Domain.AppDbContext parameterDbContext,
            [FromForm(Name = "location")] Location location,
            [FromForm(Name = "dateTime")] System.DateTimeOffset? dateTime,
            [FromForm(Name = "conditions")] Coalesce.Domain.Services.SkyConditions? conditions)
        {
            var _params = new
            {
                location = location,
                dateTime = dateTime,
                conditions = conditions
            };

            if (Context.Options.ValidateAttributesForMethods)
            {
                var _validationResult = ItemResult.FromParameterValidation(
                    GeneratedForClassViewModel!.MethodByName("GetWeatherAsync"), _params, HttpContext.RequestServices);
                if (!_validationResult.WasSuccessful) return new ItemResult<WeatherData>(_validationResult);
            }

            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(Context);
            var _methodResult = await Service.GetWeatherAsync(
                parameterDbContext,
                _params.location.MapToNew(_mappingContext),
                _params.dateTime,
                _params.conditions
            );
            var _result = new ItemResult<WeatherData>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.Services.WeatherData, WeatherDataResponse>(_methodResult, _mappingContext, includeTree);
            return _result;
        }
    }
}
