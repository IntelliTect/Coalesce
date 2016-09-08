    using IntelliTect.Coalesce.Interfaces;
    using IntelliTect.Coalesce.Mapping;
    using IntelliTect.Coalesce.Models;
    using IntelliTect.Coalesce.Helpers;
    using Newtonsoft.Json;
    using System;
    using System.Linq;
    using System.Linq.Dynamic;
    using System.Collections.Generic;
    using System.Security.Claims;
    using Coalesce.Web.Models;
    using Coalesce.Domain;
    using Coalesce.Domain.External;

using static Coalesce.Domain.CaseProduct;

namespace Coalesce.Web.Models
{
    public partial class CaseProductDtoGen : GeneratedDto<Coalesce.Domain.CaseProduct, CaseProductDtoGen>
        , IClassDto<Coalesce.Domain.CaseProduct, CaseProductDtoGen>
        {
        public CaseProductDtoGen() { }

             public Int32? CaseProductId { get; set; }
             public Int32? CaseId { get; set; }
             public CaseDtoGen Case { get; set; }
             public Int32? ProductId { get; set; }
             public ProductDtoGen Product { get; set; }

        // Create a new version of this object or use it from the lookup.
        public static CaseProductDtoGen Create(Coalesce.Domain.CaseProduct obj, ClaimsPrincipal user = null, string includes = null,
                                   Dictionary<object, object> objects = null, IncludeTree tree = null) {
            // Return null of the object is null;
            if (obj == null) return null;
                        
            if (objects == null) objects = new Dictionary<object, object>();

            includes = includes ?? "";

            // Applicable includes for CaseProduct
            

            // Applicable excludes for CaseProduct
            

            // Applicable roles for CaseProduct
            if (user != null)
			{
			}



            // See if the object is already created, but only if we aren't restricting by an includes tree.
            // If we do have an IncludeTree, we know the exact structure of our return data, so we don't need to worry about circular refs.
            if (tree == null && objects.ContainsKey(obj)) 
                return (CaseProductDtoGen)objects[obj];

            var newObject = new CaseProductDtoGen();
            if (tree == null) objects.Add(obj, newObject);
            // Fill the properties of the object.
            newObject.CaseProductId = obj.CaseProductId;
            newObject.CaseId = obj.CaseId;
            newObject.ProductId = obj.ProductId;
            if (tree == null || tree[nameof(newObject.Case)] != null)
                newObject.Case = CaseDtoGen.Create(obj.Case, user, includes, objects, tree?[nameof(newObject.Case)]);

            if (tree == null || tree[nameof(newObject.Product)] != null)
                newObject.Product = ProductDtoGen.Create(obj.Product, user, includes, objects, tree?[nameof(newObject.Product)]);

            return newObject;
        }

        // Instance constructor because there is no way to implement a static interface in C#. And generic constructors don't take arguments.
        public CaseProductDtoGen CreateInstance(Coalesce.Domain.CaseProduct obj, ClaimsPrincipal user = null, string includes = null,
                                Dictionary<object, object> objects = null, IncludeTree tree = null) {
            return Create(obj, user, includes, objects, tree);
        }

        // Updates an object from the database to the state handed in by the DTO.
        public void Update(Coalesce.Domain.CaseProduct entity, ClaimsPrincipal user = null, string includes = null)
        {
            includes = includes ?? "";

            if (OnUpdate(entity, user, includes)) return;

            // Applicable includes for CaseProduct
            

            // Applicable excludes for CaseProduct
            

            // Applicable roles for CaseProduct
            if (user != null)
			{
			}

			entity.CaseId = (Int32)(CaseId ?? 0);
			entity.ProductId = (Int32)(ProductId ?? 0);
        }

	}
}
