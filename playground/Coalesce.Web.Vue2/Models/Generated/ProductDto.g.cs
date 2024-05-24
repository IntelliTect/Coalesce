using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Web.Vue2.Models
{
    public partial class ProductParameter : GeneratedParameterDto<Coalesce.Domain.Product>
    {
        public ProductParameter() { }

        private int? _ProductId;
        private string _Name;
        private System.Guid? _UniqueId;
        private object _Unknown;

        public int? ProductId
        {
            get => _ProductId;
            set { _ProductId = value; Changed(nameof(ProductId)); }
        }
        public string Name
        {
            get => _Name;
            set { _Name = value; Changed(nameof(Name)); }
        }
        public System.Guid? UniqueId
        {
            get => _UniqueId;
            set { _UniqueId = value; Changed(nameof(UniqueId)); }
        }
        public object Unknown
        {
            get => _Unknown;
            set { _Unknown = value; Changed(nameof(Unknown)); }
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(Coalesce.Domain.Product entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            if (ShouldMapTo(nameof(ProductId))) entity.ProductId = (ProductId ?? entity.ProductId);
            if (ShouldMapTo(nameof(Name))) entity.Name = Name;
            if (ShouldMapTo(nameof(UniqueId)) && (context.IsInRoleCached("User") && context.IsInRoleCached("Admin"))) entity.UniqueId = (UniqueId ?? entity.UniqueId);
            if (ShouldMapTo(nameof(Unknown))) entity.Unknown = Unknown;
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public override Coalesce.Domain.Product MapToNew(IMappingContext context)
        {
            var entity = new Coalesce.Domain.Product();
            MapTo(entity, context);
            return entity;
        }
    }

    public partial class ProductResponse : GeneratedResponseDto<Coalesce.Domain.Product>
    {
        public ProductResponse() { }

        public int? ProductId { get; set; }
        public string Name { get; set; }
        public System.Guid? UniqueId { get; set; }
        public object Unknown { get; set; }
        public Coalesce.Web.Vue2.Models.ProductDetailsResponse Details { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(Coalesce.Domain.Product obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.ProductId = obj.ProductId;
            this.Name = obj.Name;
            this.Unknown = obj.Unknown;

            this.Details = obj.Details.MapToDto<Coalesce.Domain.ProductDetails, ProductDetailsResponse>(context, tree?[nameof(this.Details)]);

            if ((context.IsInRoleCached("User"))) this.UniqueId = obj.UniqueId;
        }
    }
}
