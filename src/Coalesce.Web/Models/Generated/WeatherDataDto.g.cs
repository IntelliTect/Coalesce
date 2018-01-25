
using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;

namespace Coalesce.Web.Models
{
    public partial class WeatherDataDtoGen : GeneratedDto<Coalesce.Domain.Services.WeatherData>
    {
        public WeatherDataDtoGen() { }

        public double? TempFahrenheit { get; set; }
        public double? Humidity { get; set; }

        public override void MapFrom(Coalesce.Domain.Services.WeatherData obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;





            // Fill the properties of the object.
            this.TempFahrenheit = obj.TempFahrenheit;
            this.Humidity = obj.Humidity;
        }

        // Updates an object from the database to the state handed in by the DTO.
        public override void MapTo(Coalesce.Domain.Services.WeatherData entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;





            entity.TempFahrenheit = (TempFahrenheit ?? 0);
            entity.Humidity = (Humidity ?? 0);
        }

    }
}
