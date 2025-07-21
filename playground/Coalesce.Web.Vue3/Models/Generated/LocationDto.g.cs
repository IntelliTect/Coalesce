using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Coalesce.Web.Vue3.Models;

public partial class LocationParameter : SparseDto, IGeneratedParameterDto<Coalesce.Domain.Services.Location>
{
    public LocationParameter() { }

    private string _City;
    private string _State;
    private string _Zip;

    public string City
    {
        get => _City;
        set { _City = value; Changed(nameof(City)); }
    }
    public string State
    {
        get => _State;
        set { _State = value; Changed(nameof(State)); }
    }
    public string Zip
    {
        get => _Zip;
        set { _Zip = value; Changed(nameof(Zip)); }
    }

    /// <summary>
    /// Map from the current DTO instance to the domain object.
    /// </summary>
    public void MapTo(Coalesce.Domain.Services.Location entity, IMappingContext context)
    {
        var includes = context.Includes;

        if (ShouldMapTo(nameof(City))) entity.City = City;
        if (ShouldMapTo(nameof(State))) entity.State = State;
        if (ShouldMapTo(nameof(Zip))) entity.Zip = Zip;
    }

    /// <summary>
    /// Map from the current DTO instance to a new instance of the domain object.
    /// </summary>
    public Coalesce.Domain.Services.Location MapToNew(IMappingContext context)
    {
        var entity = new Coalesce.Domain.Services.Location();
        MapTo(entity, context);
        return entity;
    }

    public Coalesce.Domain.Services.Location MapToModelOrNew(Coalesce.Domain.Services.Location obj, IMappingContext context)
    {
        if (obj is null) return MapToNew(context);
        MapTo(obj, context);
        return obj;
    }
}

public partial class LocationResponse : IGeneratedResponseDto<Coalesce.Domain.Services.Location>
{
    public LocationResponse() { }

    public string City { get; set; }
    public string State { get; set; }
    public string Zip { get; set; }

    /// <summary>
    /// Map from the domain object to the properties of the current DTO instance.
    /// </summary>
    public void MapFrom(Coalesce.Domain.Services.Location obj, IMappingContext context, IncludeTree tree = null)
    {
        if (obj == null) return;
        var includes = context.Includes;

        this.City = obj.City;
        this.State = obj.State;
        this.Zip = obj.Zip;
    }
}
