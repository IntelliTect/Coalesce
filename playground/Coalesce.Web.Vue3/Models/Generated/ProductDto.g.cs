using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Web.Vue3.Models
{
    public partial class ProductDtoGen : GeneratedDto<Coalesce.Domain.Product>
    {
        public ProductDtoGen() { }

        private int? _ProductId;
        private string _Name;
        private Coalesce.Web.Vue3.Models.ProductDetailsDtoGen _Details;
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
        public Coalesce.Web.Vue3.Models.ProductDetailsDtoGen Details
        {
            get => _Details;
            set { _Details = value; Changed(nameof(Details)); }
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
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(Coalesce.Domain.Product obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            // Fill the properties of the object.

            this.ProductId = obj.ProductId;
            this.Name = obj.Name;
            this.UniqueId = obj.UniqueId;
            this.Unknown = obj.Unknown;

            this.Details = obj.Details.MapToDto<Coalesce.Domain.ProductDetails, ProductDetailsDtoGen>(context, tree?[nameof(this.Details)]);

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
            if (ShouldMapTo(nameof(Unknown))) entity.Unknown = Unknown;
            if ((context.IsInRoleCached("Admin")))
            {
                if (ShouldMapTo(nameof(UniqueId))) entity.UniqueId = (UniqueId ?? entity.UniqueId);
            }
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
}
