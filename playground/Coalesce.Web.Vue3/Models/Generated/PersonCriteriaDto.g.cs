using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Coalesce.Web.Vue3.Models;

public partial class PersonCriteriaParameter : SparseDto, IGeneratedParameterDto<Coalesce.Domain.PersonCriteria>
{
    public PersonCriteriaParameter() { }

    private string _Name;
    private int? _BirthdayMonth;
    private string _EmailDomain;

    public string Name
    {
        get => _Name;
        set { _Name = value; Changed(nameof(Name)); }
    }
    public int? BirthdayMonth
    {
        get => _BirthdayMonth;
        set { _BirthdayMonth = value; Changed(nameof(BirthdayMonth)); }
    }
    public string EmailDomain
    {
        get => _EmailDomain;
        set { _EmailDomain = value; Changed(nameof(EmailDomain)); }
    }

    /// <summary>
    /// Map from the current DTO instance to the domain object.
    /// </summary>
    public void MapTo(Coalesce.Domain.PersonCriteria entity, IMappingContext context)
    {
        var includes = context.Includes;

        if (ShouldMapTo(nameof(Name))) entity.Name = Name;
        if (ShouldMapTo(nameof(BirthdayMonth))) entity.BirthdayMonth = BirthdayMonth;
        if (ShouldMapTo(nameof(EmailDomain))) entity.EmailDomain = EmailDomain;
    }

    /// <summary>
    /// Map from the current DTO instance to a new instance of the domain object.
    /// </summary>
    public Coalesce.Domain.PersonCriteria MapToNew(IMappingContext context)
    {
        var entity = new Coalesce.Domain.PersonCriteria();
        MapTo(entity, context);
        return entity;
    }

    public Coalesce.Domain.PersonCriteria MapToModelOrNew(Coalesce.Domain.PersonCriteria obj, IMappingContext context)
    {
        if (obj is null) return MapToNew(context);
        MapTo(obj, context);
        return obj;
    }
}

public partial class PersonCriteriaResponse : IGeneratedResponseDto<Coalesce.Domain.PersonCriteria>
{
    public PersonCriteriaResponse() { }

    public string Name { get; set; }
    public int? BirthdayMonth { get; set; }
    public string EmailDomain { get; set; }

    /// <summary>
    /// Map from the domain object to the properties of the current DTO instance.
    /// </summary>
    public void MapFrom(Coalesce.Domain.PersonCriteria obj, IMappingContext context, IncludeTree tree = null)
    {
        if (obj == null) return;
        var includes = context.Includes;

        this.Name = obj.Name;
        this.BirthdayMonth = obj.BirthdayMonth;
        this.EmailDomain = obj.EmailDomain;
    }
}
