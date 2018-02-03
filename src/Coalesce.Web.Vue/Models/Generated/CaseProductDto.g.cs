
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
    public partial class CaseProductDtoGen : GeneratedDto<Coalesce.Domain.CaseProduct>
    {
        public CaseProductDtoGen() { }

        public int? CaseProductId { get; set; }
        public int? CaseId { get; set; }
        public Coalesce.Web.Vue.Models.CaseDtoGen Case { get; set; }
        public int? ProductId { get; set; }
        public Coalesce.Web.Vue.Models.ProductDtoGen Product { get; set; }

        public override void MapFrom(Coalesce.Domain.CaseProduct obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            // Applicable includes for CaseProduct


            // Applicable excludes for CaseProduct


            // Applicable roles for CaseProduct


            // Fill the properties of the object.
            this.CaseProductId = obj.CaseProductId;
            this.CaseId = obj.CaseId;
            this.ProductId = obj.ProductId;
            if (tree == null || tree[nameof(this.Case)] != null)
                this.Case = obj.Case.MapToDto<Coalesce.Domain.Case, CaseDtoGen>(context, tree?[nameof(this.Case)]);

            if (tree == null || tree[nameof(this.Product)] != null)
                this.Product = obj.Product.MapToDto<Coalesce.Domain.Product, ProductDtoGen>(context, tree?[nameof(this.Product)]);

        }

        // Updates an object from the database to the state handed in by the DTO.
        public override void MapTo(Coalesce.Domain.CaseProduct entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            // Applicable includes for CaseProduct


            // Applicable excludes for CaseProduct


            // Applicable roles for CaseProduct


            entity.CaseId = (CaseId ?? 0);
            entity.ProductId = (ProductId ?? 0);
        }

    }
}
