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

        private double? _TempFahrenheit;
        private double? _Humidity;
        private Coalesce.Web.Models.LocationDtoGen _Location;

        public double? TempFahrenheit
        {
            get => _TempFahrenheit;
            set { _TempFahrenheit = value; Changed(nameof(TempFahrenheit)); }
        }
        public double? Humidity
        {
            get => _Humidity;
            set { _Humidity = value; Changed(nameof(Humidity)); }
        }
        public Coalesce.Web.Models.LocationDtoGen Location
        {
            get => _Location;
            set { _Location = value; Changed(nameof(Location)); }
        }

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

            if (ShouldMapTo(nameof(TempFahrenheit))) entity.TempFahrenheit = (TempFahrenheit ?? entity.TempFahrenheit);
            if (ShouldMapTo(nameof(Humidity))) entity.Humidity = (Humidity ?? entity.Humidity);
            if (ShouldMapTo(nameof(Location))) entity.Location = Location?.MapToModel<Coalesce.Domain.Services.Location, LocationDtoGen>(entity.Location ?? new Coalesce.Domain.Services.Location(), context);
        }
    }
}
