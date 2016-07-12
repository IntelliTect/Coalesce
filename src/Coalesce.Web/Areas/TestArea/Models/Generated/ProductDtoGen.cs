
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

        public ProductDto(ClaimsPrincipal user, Product entity)
        {
            User = user;
            List<string> roles;
                    ProductId = entity.ProductId;
                    Name = entity.Name;
        }

        public ClaimsPrincipal User { get; set; }
            
         public Int32? ProductId { get; set; }
         public String Name { get; set; }

        public void Update(object obj)
        {   
            if (User == null) throw new InvalidOperationException("Updating an entity requires the User property to be populated.");

            Product entity = (Product)obj;

            List<string> roles;
                    entity.Name = Name;
        }
    }
}