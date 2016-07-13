
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Intellitect.ComponentModel.Interfaces;
using System.Linq;
// Model Namespaces
using Coalesce.Domain;
using Coalesce.Domain.External;
using static Coalesce.Domain.Product;

namespace Coalesce.Web.TestArea.Models
{
    public partial class ProductDto : IClassDto
    {
        public ProductDto() { }

        public ProductDto(Product entity, ClaimsPrincipal user = null, string includes = null)
        {
            User = user;
            Includes = includes ?? "";

            // Applicable includes for Product
            

            // Applicable excludes for Product
            

            // Applicable roles for Product
            if (User != null)
			{
			}

			ProductId = entity.ProductId;
			Name = entity.Name;
        }

        public ClaimsPrincipal User { get; set; }
        public string Includes { get; set; }
            
         public Int32? ProductId { get; set; }
         public String Name { get; set; }

        public void Update(object obj)
        {   
            if (User == null) throw new InvalidOperationException("Updating an entity requires the User property to be populated.");

            // Applicable includes for Product
            

            // Applicable excludes for Product
            

            // Applicable roles for Product
            if (User != null)
			{
			}


            Product entity = (Product)obj;

			entity.Name = Name;
        }
    }
}