using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Coalesce.Domain
{
    [Table("Product")]
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
    }
}
