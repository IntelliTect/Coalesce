using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using IntelliTect.Coalesce.DataAnnotations;

namespace IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext
{
    [Table("Product")]
    [Create(Roles = "Admin")]
    [Edit(Roles = "Admin")]
    public class Product
    {
        public int ProductId { get; set; }

        [Search(SearchMethod = SearchAttribute.SearchMethods.Contains)]
        public string Name { get; set; }
    }
}
