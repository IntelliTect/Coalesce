using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Web.Models
{
    public partial class WeatherDataDtoGen : GeneratedDto<Coalesce.Domain.Services.WeatherData>
    {
        public WeatherDataDtoGen() { }

        public double? TempFahrenheit { get; set; }
        public double? Humidity { get; set; }
        public Coalesce.Web.Models.LocationDtoGen Location { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(Coalesce.Domain.Services.WeatherData obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            // Fill the properties of the object.

            this.TempFahrenheit = obj.TempFahrenheit;
            this.Humidity = obj.Humidity;

            this.Location = obj.Location.MapToDto<Coalesce.Domain.Services.Location, LocationDtoGen>(context, tree?[nameof(this.Location)]);

        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(Coalesce.Domain.Services.WeatherData entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            entity.TempFahrenheit = (TempFahrenheit ?? entity.TempFahrenheit);
            entity.Humidity = (Humidity ?? entity.Humidity);
        }
    }
}
