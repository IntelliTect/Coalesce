﻿
using Coalesce.Web.Models;
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

namespace Coalesce.Web.Api
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
        /// Method: GetWeather
        /// </summary>
        [HttpPost("GetWeather")]
        [Authorize]
        public virtual ItemResult<WeatherDataDtoGen> GetWeather([FromServices] Coalesce.Domain.AppDbContext parameterDbContext, LocationDtoGen location, System.DateTimeOffset? dateTime)
        {
            var result = new ItemResult<WeatherDataDtoGen>();

            IncludeTree includeTree = null;
            var methodResult = Service.GetWeather(parameterDbContext, location.MapToModel(new Coalesce.Domain.Services.Location(), new MappingContext(User)), dateTime);

            var mappingContext = new MappingContext(User, "");
            result.Object = Mapper.MapToDto<Coalesce.Domain.Services.WeatherData, WeatherDataDtoGen>(methodResult, mappingContext, includeTree);

            return result;
        }
    }
}
