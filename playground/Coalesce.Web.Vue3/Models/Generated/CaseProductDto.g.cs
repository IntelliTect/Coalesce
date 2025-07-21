using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Coalesce.Web.Vue3.Models;

public partial class CaseProductParameter : SparseDto, IGeneratedParameterDto<Coalesce.Domain.CaseProduct>
{
    public CaseProductParameter() { }

    private int? _CaseProductId;
    private int? _CaseId;
    private int? _ProductId;

    public int? CaseProductId
    {
        get => _CaseProductId;
        set { _CaseProductId = value; Changed(nameof(CaseProductId)); }
    }
    public int? CaseId
    {
        get => _CaseId;
        set { _CaseId = value; Changed(nameof(CaseId)); }
    }
    public int? ProductId
    {
        get => _ProductId;
        set { _ProductId = value; Changed(nameof(ProductId)); }
    }

    /// <summary>
    /// Map from the current DTO instance to the domain object.
    /// </summary>
    public void MapTo(Coalesce.Domain.CaseProduct entity, IMappingContext context)
    {
        var includes = context.Includes;

        if (ShouldMapTo(nameof(CaseProductId))) entity.CaseProductId = (CaseProductId ?? entity.CaseProductId);
        if (ShouldMapTo(nameof(CaseId))) entity.CaseId = (CaseId ?? entity.CaseId);
        if (ShouldMapTo(nameof(ProductId))) entity.ProductId = (ProductId ?? entity.ProductId);
    }

    /// <summary>
    /// Map from the current DTO instance to a new instance of the domain object.
    /// </summary>
    public Coalesce.Domain.CaseProduct MapToNew(IMappingContext context)
    {
        var entity = new Coalesce.Domain.CaseProduct();
        MapTo(entity, context);
        return entity;
    }

    public Coalesce.Domain.CaseProduct MapToModelOrNew(Coalesce.Domain.CaseProduct obj, IMappingContext context)
    {
        if (obj is null) return MapToNew(context);
        MapTo(obj, context);
        return obj;
    }
}

public partial class CaseProductResponse : IGeneratedResponseDto<Coalesce.Domain.CaseProduct>
{
    public CaseProductResponse() { }

    public int? CaseProductId { get; set; }
    public int? CaseId { get; set; }
    public int? ProductId { get; set; }
    public Coalesce.Web.Vue3.Models.CaseResponse Case { get; set; }
    public Coalesce.Web.Vue3.Models.ProductResponse Product { get; set; }

    /// <summary>
    /// Map from the domain object to the properties of the current DTO instance.
    /// </summary>
    public void MapFrom(Coalesce.Domain.CaseProduct obj, IMappingContext context, IncludeTree tree = null)
    {
        if (obj == null) return;
        var includes = context.Includes;

        this.CaseProductId = obj.CaseProductId;
        this.CaseId = obj.CaseId;
        this.ProductId = obj.ProductId;
        if (tree == null || tree[nameof(this.Case)] != null)
            this.Case = obj.Case.MapToDto<Coalesce.Domain.Case, CaseResponse>(context, tree?[nameof(this.Case)]);

        if (tree == null || tree[nameof(this.Product)] != null)
            this.Product = obj.Product.MapToDto<Coalesce.Domain.Product, ProductResponse>(context, tree?[nameof(this.Product)]);

    }
}
