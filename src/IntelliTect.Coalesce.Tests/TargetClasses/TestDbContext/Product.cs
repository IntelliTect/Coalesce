using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using IntelliTect.Coalesce.DataAnnotations;

namespace IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext
{
    [Table("Product")]
    [Read(RoleNames.Admin, RoleNames.User)]
    [Create(Roles = RoleNames.Admin)]
    [Edit(Roles = RoleNames.Admin)]
    public class Product
    {
        public int ProductId { get; set; }

        [Search(SearchMethod = SearchAttribute.SearchMethods.Contains)]
        [DefaultOrderBy]
        public string Name { get; set; }
    }
}
