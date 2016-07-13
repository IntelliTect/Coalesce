
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Intellitect.ComponentModel.Interfaces;
using System.Linq;
// Model Namespaces
using Coalesce.Domain;
using Coalesce.Domain.External;
using static Coalesce.Domain.CaseProduct;

namespace Coalesce.Web.Models
{
    public partial class CaseProductDto : IClassDto
    {
        public CaseProductDto() { }

        public CaseProductDto(CaseProduct entity, ClaimsPrincipal user = null, string includes = null)
        {
            User = user;
            Includes = includes ?? "";

            // Applicable includes for CaseProduct
            

            // Applicable excludes for CaseProduct
            

            // Applicable roles for CaseProduct
            if (User != null)
			{
			}

			CaseProductId = entity.CaseProductId;
			CaseId = entity.CaseId;
			Case = entity.Case;
			ProductId = entity.ProductId;
			Product = entity.Product;
        }

        public ClaimsPrincipal User { get; set; }
        public string Includes { get; set; }
            
         public Int32? CaseProductId { get; set; }
         public Int32? CaseId { get; set; }
         public Case Case { get; set; }
         public Int32? ProductId { get; set; }
         public Product Product { get; set; }

        public void Update(object obj)
        {   
            if (User == null) throw new InvalidOperationException("Updating an entity requires the User property to be populated.");

            // Applicable includes for CaseProduct
            

            // Applicable excludes for CaseProduct
            

            // Applicable roles for CaseProduct
            if (User != null)
			{
			}


            CaseProduct entity = (CaseProduct)obj;

			entity.CaseId = (Int32)CaseId;
			entity.ProductId = (Int32)ProductId;
        }
    }
}