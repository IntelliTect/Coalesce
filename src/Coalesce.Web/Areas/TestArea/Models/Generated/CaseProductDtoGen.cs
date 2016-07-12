
using System;
using System.Collections.Generic;
using Intellitect.ComponentModel.Mapping;
// Model Namespaces
using Coalesce.Domain;
using Coalesce.Domain.External;
using static Coalesce.Domain.CaseProduct;

namespace Coalesce.Web.TestArea.Models
{
    public partial class CaseProductDto : IClassDto
    {
        public CaseProductDto() { }

        public CaseProductDto(CaseProduct entity)
        {
                CaseProductId = entity.CaseProductId;
                CaseId = entity.CaseId;
                Case = entity.Case;
                ProductId = entity.ProductId;
                Product = entity.Product;
        }
        
         public Int32? CaseProductId { get; set; }
         public Int32? CaseId { get; set; }
         public Case Case { get; set; }
         public Int32? ProductId { get; set; }
         public Product Product { get; set; }

        public void Update(object obj)
        {
            CaseProduct entity = (CaseProduct)obj;

                entity.CaseId = (Int32)CaseId;
                entity.Case = Case;
                entity.ProductId = (Int32)ProductId;
                entity.Product = Product;
        }
    }
}
