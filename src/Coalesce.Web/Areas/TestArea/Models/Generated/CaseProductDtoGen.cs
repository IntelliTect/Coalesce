
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

namespace Coalesce.Web.TestArea.Models
{
    public partial class CaseProductDtoGen : GeneratedDto<CaseProduct, CaseProductDtoGen>, IClassDto
    {
        public CaseProductDtoGen() { }

         public Int32? CaseProductId { get; set; }
         public Int32? CaseId { get; set; }
         public CaseDtoGen Case { get; set; }
         public Int32? ProductId { get; set; }
         public ProductDtoGen Product { get; set; }

        public void Update(object obj, ClaimsPrincipal user = null, string includes = null)
        {
            if (user == null) throw new InvalidOperationException("Updating an entity requires the User property to be populated.");

            includes = includes ?? "";

            CaseProduct entity = (CaseProduct)obj;

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
