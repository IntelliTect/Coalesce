
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
using static Coalesce.Domain.Product;

namespace Coalesce.Web.Models
{
    public partial class ProductDtoGen : GeneratedDto<Product, ProductDtoGen>, IClassDto
    {
        public ProductDtoGen() { }

         public Int32? ProductId { get; set; }
         public String Name { get; set; }

        public void Update(object obj, ClaimsPrincipal user = null, string includes = null)
        {
            if (user == null) throw new InvalidOperationException("Updating an entity requires the User property to be populated.");

            includes = includes ?? "";

            Product entity = (Product)obj;

            if (OnUpdate(entity, user, includes)) return;

            // Applicable includes for Product
            

            // Applicable excludes for Product
            

            // Applicable roles for Product
            if (user != null)
			{
			}
    
			entity.Name = Name;
        }

        public void SecurityTrim(ClaimsPrincipal user = null, string includes = null)
        {
            if (OnSecurityTrim(user, includes)) return;

            // Applicable includes for Product
            

            // Applicable excludes for Product
            

            // Applicable roles for Product
            if (user != null)
			{
			}

        }
    }
}
