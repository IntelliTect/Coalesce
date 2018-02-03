
using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;

namespace Coalesce.Web.Vue.Models
{
    public partial class ProductDtoGen : GeneratedDto<Coalesce.Domain.Product>
    {
        public ProductDtoGen() { }

        public int? ProductId { get; set; }
        public string Name { get; set; }

        public override void MapFrom(Coalesce.Domain.Product obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            // Applicable includes for Product


            // Applicable excludes for Product


            // Applicable roles for Product


            // Fill the properties of the object.
            this.ProductId = obj.ProductId;
            this.Name = obj.Name;
        }

        // Updates an object from the database to the state handed in by the DTO.
        public override void MapTo(Coalesce.Domain.Product entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            // Applicable includes for Product


            // Applicable excludes for Product


            // Applicable roles for Product


            entity.Name = Name;
        }

    }
}
