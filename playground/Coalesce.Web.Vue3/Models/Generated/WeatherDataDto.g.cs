using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Coalesce.Web.Vue3.Models;

public partial class WeatherDataParameter : SparseDto, IGeneratedParameterDto<Coalesce.Domain.Services.WeatherData>
{
    public WeatherDataParameter() { }

    private double? _TempFahrenheit;
    private double? _Humidity;
    private Coalesce.Web.Vue3.Models.LocationParameter _Location;

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
    public Coalesce.Web.Vue3.Models.LocationParameter Location
    {
        get => _Location;
        set { _Location = value; Changed(nameof(Location)); }
    }

    /// <summary>
    /// Map from the current DTO instance to the domain object.
    /// </summary>
    public void MapTo(Coalesce.Domain.Services.WeatherData entity, IMappingContext context)
    {
        var includes = context.Includes;

        if (ShouldMapTo(nameof(TempFahrenheit))) entity.TempFahrenheit = (TempFahrenheit ?? entity.TempFahrenheit);
        if (ShouldMapTo(nameof(Humidity))) entity.Humidity = (Humidity ?? entity.Humidity);
        if (ShouldMapTo(nameof(Location))) entity.Location = Location?.MapToModelOrNew(entity.Location, context);
    }

    /// <summary>
    /// Map from the current DTO instance to a new instance of the domain object.
    /// </summary>
    public Coalesce.Domain.Services.WeatherData MapToNew(IMappingContext context)
    {
        var entity = new Coalesce.Domain.Services.WeatherData();
        MapTo(entity, context);
        return entity;
    }

    public Coalesce.Domain.Services.WeatherData MapToModelOrNew(Coalesce.Domain.Services.WeatherData obj, IMappingContext context)
    {
        if (obj is null) return MapToNew(context);
        MapTo(obj, context);
        return obj;
    }
}

public partial class WeatherDataResponse : IGeneratedResponseDto<Coalesce.Domain.Services.WeatherData>
{
    public WeatherDataResponse() { }

    public double? TempFahrenheit { get; set; }
    public double? Humidity { get; set; }
    public Coalesce.Web.Vue3.Models.LocationResponse Location { get; set; }

    /// <summary>
    /// Map from the domain object to the properties of the current DTO instance.
    /// </summary>
    public void MapFrom(Coalesce.Domain.Services.WeatherData obj, IMappingContext context, IncludeTree tree = null)
    {
        if (obj == null) return;
        var includes = context.Includes;

        this.TempFahrenheit = obj.TempFahrenheit;
        this.Humidity = obj.Humidity;

        this.Location = obj.Location.MapToDto<Coalesce.Domain.Services.Location, LocationResponse>(context, tree?[nameof(this.Location)]);

    }
}
