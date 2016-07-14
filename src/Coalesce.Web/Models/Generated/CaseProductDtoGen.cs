
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Intellitect.ComponentModel.Interfaces;
using Intellitect.ComponentModel.Models;
using Intellitect.ComponentModel.Mapping;
using System.Linq;
using Newtonsoft.Json;
// Model Namespaces
    using Coalesce.Domain;
    using Coalesce.Domain.External;
using static Coalesce.Domain.CaseProduct;

namespace Coalesce.Web.Models
{
    public partial class CaseProductDtoGen : GeneratedDto<CaseProduct, CaseProductDtoGen>
        , IClassDto<CaseProduct, CaseProductDtoGen>
        {
        public CaseProductDtoGen() { }

             public Int32? CaseProductId { get; set; }
             public Int32? CaseId { get; set; }
             public CaseDtoGen Case { get; set; }
             public Int32? ProductId { get; set; }
             public ProductDtoGen Product { get; set; }

        // Create a new version of this object or use it from the lookup.
        public static CaseProductDtoGen Create(CaseProduct obj, ClaimsPrincipal user = null, string includes = null,
                                   Dictionary<string, object> objects = null) {
            // Return null of the object is null;
            if (obj == null) return null;
                        
            if (objects == null) objects = new Dictionary<string, object>();

            if (user == null) throw new InvalidOperationException("Updating an entity requires the User property to be populated.");

            includes = includes ?? "";

            // Applicable includes for CaseProduct
            

            // Applicable excludes for CaseProduct
            

            // Applicable roles for CaseProduct
            if (user != null)
			{
			}



            // See if the object is already created.
            string key = $"CaseProduct{obj.CaseProductId}";
            if (objects.ContainsKey(key)) 
                return (CaseProductDtoGen)objects[key];

            var newObject = new CaseProductDtoGen();
            objects.Add(key, newObject);
            // Fill the properties of the object.
            newObject.CaseId = obj.CaseId;
            newObject.Case = CaseDtoGen.Create(obj.Case, user, includes, objects);
            newObject.ProductId = obj.ProductId;
            newObject.Product = ProductDtoGen.Create(obj.Product, user, includes, objects);
            return newObject;
        }

        // Instance constructor because there is no way to implement a static interface in C#.
        public CaseProductDtoGen CreateInstance(CaseProduct obj, ClaimsPrincipal user = null, string includes = null,
                                Dictionary<string, object> objects = null) {
            return Create(obj, user, includes, objects);
        }

        // Updates an object from the database to the state handed in by the DTO.
        public void Update(CaseProduct entity, ClaimsPrincipal user = null, string includes = null)
        {
        if (user == null) throw new InvalidOperationException("Updating an entity requires the User property to be populated.");

        includes = includes ?? "";

        if (OnUpdate(entity, user, includes)) return;

        // Applicable includes for CaseProduct
        

        // Applicable excludes for CaseProduct
        

        // Applicable roles for CaseProduct
        if (user != null)
			{
			}

			entity.CaseId = (Int32)CaseId;
			entity.ProductId = (Int32)ProductId;
        }

        public void SecurityTrim(ClaimsPrincipal user = null, string includes = null)
        {
        if (OnSecurityTrim(user, includes)) return;

        // Applicable includes for CaseProduct
        

        // Applicable excludes for CaseProduct
        

        // Applicable roles for CaseProduct
        if (user != null)
			{
			}

        }
        }
        }
