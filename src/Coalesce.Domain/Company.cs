﻿using System.Collections.Generic;
using IntelliTect.Coalesce.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IntelliTect.Coalesce;
using System.Threading.Tasks;
using System.Linq;

namespace Coalesce.Domain
{
    [Table("Company")]
    [Create(PermissionLevel = SecurityPermissionLevels.DenyAll)]
    public class Company
    {
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        [Hidden(HiddenAttribute.Areas.List)]
        public string City { get; set; }
        [Hidden(HiddenAttribute.Areas.Edit)]
        public string State { get; set; }
        [Hidden(HiddenAttribute.Areas.All)]
        public string ZipCode { get; set; }

        public bool IsDeleted { get; set; }

        [InverseProperty("Company")]
        public ICollection<Person> Employees { get; set; }

        [NotMapped]
        [ListText]
        public string AltName => Name + ": " + City;

        [Coalesce]
        public static ICollection<Company> GetCertainItems(AppDbContext db, bool isDeleted = false)
        {
            return db.Companies.Where(f => f.IsDeleted == isDeleted).ToList();
        }

        public class DefaultSource : StandardDataSource<Company, AppDbContext>
        {
            public DefaultSource(CrudContext<AppDbContext> context) : base(context) { }

            //public override IQueryable<Company> GetQuery(IDataSourceParameters parameters)
            //{
            //    if (User.)
            //}
        }

        public class Behaviors : StandardBehaviors<Company, AppDbContext>
        {
            public Behaviors(CrudContext<AppDbContext> context) : base(context) { }

            public override Task ExecuteDeleteAsync(Company item)
            {
                // Soft-deletable items. After deleting a company item,
                // it should still be listed in the admin pages, but with an IsActive flag false.
                // This lets us test behavior around reloading soft-deleted items, and keeping them in their parent collection after a soft delete.

                item.IsDeleted = true;
                return Db.SaveChangesAsync();
            }
        }
    }
}