using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using IntelliTect.Coalesce.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext
{
    [Table("Product")]
    [Read(RoleNames.Admin, RoleNames.User)]
    [Create(Roles = RoleNames.Admin)]
    [Edit(Roles = RoleNames.Admin)]
    // These indexes are used for ExceptionResultTests (StandardBehaviors.GetExceptionResult)
    [Index(nameof(UniqueId1), IsUnique = true)]
    [Index(nameof(UniqueId1), nameof(UniqueId2), IsUnique = true)]
    [Index(nameof(TenantId), nameof(UniqueId1), IsUnique = true)]
    public class Product
    {
        public int ProductId { get; set; }

        [Search(SearchMethod = SearchAttribute.SearchMethods.Contains)]
        [DefaultOrderBy]
        public string Name { get; set; }

        [InternalUse]
        public int TenantId { get; set; }

        [Display(Name = "ID1")]
        public string UniqueId1 { get; set; }

        [Display(Name = "ID2")]
        public string UniqueId2 { get; set; }
    }
}
