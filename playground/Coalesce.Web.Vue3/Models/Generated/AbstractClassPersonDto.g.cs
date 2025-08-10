using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Coalesce.Web.Vue3.Models;

public partial class AbstractClassPersonParameter : SparseDto, IGeneratedParameterDto<Coalesce.Domain.AbstractClassPerson>
{
    public AbstractClassPersonParameter() { }

    private int? _Id;
    private int? _PersonId;
    private int? _AbstractClassId;

    public int? Id
    {
        get => _Id;
        set { _Id = value; Changed(nameof(Id)); }
    }
    public int? PersonId
    {
        get => _PersonId;
        set { _PersonId = value; Changed(nameof(PersonId)); }
    }
    public int? AbstractClassId
    {
        get => _AbstractClassId;
        set { _AbstractClassId = value; Changed(nameof(AbstractClassId)); }
    }

    /// <summary>
    /// Map from the current DTO instance to the domain object.
    /// </summary>
    public void MapTo(Coalesce.Domain.AbstractClassPerson entity, IMappingContext context)
    {
        var includes = context.Includes;

        if (ShouldMapTo(nameof(Id))) entity.Id = (Id ?? entity.Id);
        if (ShouldMapTo(nameof(PersonId))) entity.PersonId = (PersonId ?? entity.PersonId);
        if (ShouldMapTo(nameof(AbstractClassId))) entity.AbstractClassId = (AbstractClassId ?? entity.AbstractClassId);
    }

    /// <summary>
    /// Map from the current DTO instance to a new instance of the domain object.
    /// </summary>
    public Coalesce.Domain.AbstractClassPerson MapToNew(IMappingContext context)
    {
        var entity = new Coalesce.Domain.AbstractClassPerson();
        MapTo(entity, context);
        return entity;
    }

    public Coalesce.Domain.AbstractClassPerson MapToModelOrNew(Coalesce.Domain.AbstractClassPerson obj, IMappingContext context)
    {
        if (obj is null) return MapToNew(context);
        MapTo(obj, context);
        return obj;
    }
}

public partial class AbstractClassPersonResponse : IGeneratedResponseDto<Coalesce.Domain.AbstractClassPerson>
{
    public AbstractClassPersonResponse() { }

    public int? Id { get; set; }
    public int? PersonId { get; set; }
    public int? AbstractClassId { get; set; }
    public Coalesce.Web.Vue3.Models.PersonResponse Person { get; set; }
    public Coalesce.Web.Vue3.Models.AbstractClassResponse AbstractClass { get; set; }

    /// <summary>
    /// Map from the domain object to the properties of the current DTO instance.
    /// </summary>
    public void MapFrom(Coalesce.Domain.AbstractClassPerson obj, IMappingContext context, IncludeTree tree = null)
    {
        if (obj == null) return;
        var includes = context.Includes;

        this.Id = obj.Id;
        this.PersonId = obj.PersonId;
        this.AbstractClassId = obj.AbstractClassId;
        if (tree == null || tree[nameof(this.Person)] != null)
            this.Person = obj.Person.MapToDto<Coalesce.Domain.Person, PersonResponse>(context, tree?[nameof(this.Person)]);

        if (tree == null || tree[nameof(this.AbstractClass)] != null)
            this.AbstractClass = obj.AbstractClass.MapToDto<Coalesce.Domain.AbstractClass, AbstractClassResponse>(context, tree?[nameof(this.AbstractClass)]);

    }
}
