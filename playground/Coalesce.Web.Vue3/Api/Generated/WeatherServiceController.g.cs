
using Coalesce.Web.Vue3.Models;
using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Api;
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
        protected Coalesce.Domain.Services.IWeatherService Service { get; }

        public WeatherServiceController(Coalesce.Domain.Services.IWeatherService service)
        {
            Service = service;
        }

        /// <summary>
        /// Method: GetWeatherAsync
        /// </summary>
        [HttpPost("GetWeather")]
        [HttpPost("GetWeatherAsync")]
        [Authorize]
        public virtual async Task<ItemResult<WeatherDataDtoGen>> GetWeather(
            [FromServices] Coalesce.Domain.AppDbContext parameterDbContext,
            [FromForm(Name = "location")] LocationDtoGen location,
            [FromForm(Name = "dateTime")] System.DateTimeOffset? dateTime,
            [FromForm(Name = "conditions")] Coalesce.Domain.Services.SkyConditions? conditions)
        {
            IncludeTree includeTree = null;
            var _mappingContext = new MappingContext(User);
            var _methodResult = await Service.GetWeatherAsync(parameterDbContext, location.MapToNew(_mappingContext), dateTime, conditions);
            var _result = new ItemResult<WeatherDataDtoGen>();
            _result.Object = Mapper.MapToDto<Coalesce.Domain.Services.WeatherData, WeatherDataDtoGen>(_methodResult, _mappingContext, includeTree);
            return _result;
        }
    }
}
