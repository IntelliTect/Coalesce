
using IntelliTect.Coalesce.Helpers.IncludeTree;
using IntelliTect.Coalesce.Interfaces;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;

namespace Coalesce.Web.Models
{
    public partial class CaseProductDtoGen : GeneratedDto<Coalesce.Domain.CaseProduct, CaseProductDtoGen>
        , IClassDto<Coalesce.Domain.CaseProduct, CaseProductDtoGen>
    {
        public CaseProductDtoGen() { }

        public int? CaseProductId { get; set; }
        public int? CaseId { get; set; }
        public Coalesce.Web.Models.CaseDtoGen Case { get; set; }
        public int? ProductId { get; set; }
        public Coalesce.Web.Models.ProductDtoGen Product { get; set; }

        // Create a new version of this object or use it from the lookup.
        public static CaseProductDtoGen Create(Coalesce.Domain.CaseProduct obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return null;
            var includes = context.Includes;

            // Applicable includes for CaseProduct


            // Applicable excludes for CaseProduct


            // Applicable roles for CaseProduct



            // See if the object is already created, but only if we aren't restricting by an includes tree.
            // If we do have an IncludeTree, we know the exact structure of our return data, so we don't need to worry about circular refs.
            if (tree == null && context.TryGetMapping(obj, out CaseProductDtoGen existing)) return existing;

            var newObject = new CaseProductDtoGen();
            if (tree == null) context.AddMapping(obj, newObject);
            // Fill the properties of the object.
            newObject.CaseProductId = obj.CaseProductId;
            newObject.CaseId = obj.CaseId;
            newObject.ProductId = obj.ProductId;
            if (tree == null || tree[nameof(newObject.Case)] != null)
                newObject.Case = CaseDtoGen.Create(obj.Case, context, tree?[nameof(newObject.Case)]);

            if (tree == null || tree[nameof(newObject.Product)] != null)
                newObject.Product = ProductDtoGen.Create(obj.Product, context, tree?[nameof(newObject.Product)]);

            return newObject;
        }

        // Instance constructor because there is no way to implement a static interface in C#. And generic constructors don't take arguments.
        public CaseProductDtoGen CreateInstance(Coalesce.Domain.CaseProduct obj, IMappingContext context, IncludeTree tree = null)
        {
            return Create(obj, context, tree);
        }

        // Updates an object from the database to the state handed in by the DTO.
        public void Update(Coalesce.Domain.CaseProduct entity, IMappingContext context)
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
