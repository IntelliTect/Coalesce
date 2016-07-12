
using System;
using System.Collections.Generic;
using Intellitect.ComponentModel.Mapping;
// Model Namespaces
using Coalesce.Domain;
using Coalesce.Domain.External;
using static Coalesce.Domain.Product;

namespace Coalesce.Web.Models
{
    public partial class ProductDto : IClassDto
    {
        public ProductDto() { }

        public ProductDto(Product entity)
        {
                ProductId = entity.ProductId;
                Name = entity.Name;
        }
        
         public Int32? ProductId { get; set; }
         public String Name { get; set; }

        public void Update(object obj)
        {
            Product entity = (Product)obj;

                entity.Name = Name;
        }
    }
}
