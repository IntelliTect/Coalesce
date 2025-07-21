using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable

namespace IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;

public class StringIdentity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string StringIdentityId { get; set; } = null!;

    public string? ParentId { get; set; }
    // Arbitrary reference navigation (optional)
    public StringIdentity Parent { get; set; } = null!;

    public string ParentReqId { get; set; } = null!;
    // Arbitrary reference navigation (req)
    public StringIdentity? ParentReq { get; set; }

    // Artbirary collection navigation
    [InverseProperty(nameof(Parent))]
    public ICollection<StringIdentity> Children { get; set; } = null!;
}


#nullable disable

public partial class StringIdentityDtoGen : SparseDto, IGeneratedParameterDto<StringIdentity>
{
    public StringIdentityDtoGen() { }

    private string _StringIdentityId;
    private string _ParentId;
    private string _ParentReqId;
    private StringIdentityDtoGen _Parent;
    private StringIdentityDtoGen _ParentReq;
    private ICollection<StringIdentityDtoGen> _Children;

    public string StringIdentityId
    {
        get => _StringIdentityId;
        set { _StringIdentityId = value; Changed(nameof(StringIdentityId)); }
    }

    public string ParentId
    {
        get => _ParentId;
        set { _ParentId = value; Changed(nameof(ParentId)); }
    }

    public StringIdentityDtoGen Parent
    {
        get => _Parent;
        set { _Parent = value; Changed(nameof(Parent)); }
    }

    public string ParentReqId
    {
        get => _ParentReqId;
        set { _ParentReqId = value; Changed(nameof(ParentReqId)); }
    }

    public StringIdentityDtoGen ParentReq
    {
        get => _ParentReq;
        set { _ParentReq = value; Changed(nameof(ParentReq)); }
    }

    public ICollection<StringIdentityDtoGen> Children
    {
        get => _Children;
        set { _Children = value; Changed(nameof(Children)); }
    }

    /// <summary>
    /// Map from the current DTO instance to the domain object.
    /// </summary>
    public void MapTo(StringIdentity entity, IMappingContext context)
        => throw new NotImplementedException(
            "This 'generated dto' is actually hand-written for these tests. Mapping methods are unused.");

    /// <summary>
    /// Map from the current DTO instance to a new instance of the domain object.
    /// </summary>
    public StringIdentity MapToNew(IMappingContext context)
    {
        var entity = new StringIdentity();
        MapTo(entity, context);
        return entity;
    }
}
