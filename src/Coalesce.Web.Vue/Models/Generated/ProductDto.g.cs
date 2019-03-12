using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Web.Vue.Models
{
    public partial class ProductDtoGen : GeneratedDto<Coalesce.Domain.Product>
    {
        public ProductDtoGen() { }

        public int? ProductId { get; set; }
        public string Name { get; set; }
        public Coalesce.Web.Vue.Models.ProductDetailsDtoGen Details { get; set; }
        public System.Guid? UniqueId { get; set; }

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

            this.Details = obj.Details.MapToDto<Coalesce.Domain.ProductDetails, ProductDetailsDtoGen>(context, tree?[nameof(this.Details)]);

        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(Coalesce.Domain.Product entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            entity.ProductId = (ProductId ?? entity.ProductId);
            entity.Name = Name;
            entity.UniqueId = (UniqueId ?? entity.UniqueId);
        }
    }
}
