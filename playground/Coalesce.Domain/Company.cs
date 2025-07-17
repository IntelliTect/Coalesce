using System.Collections.Generic;
using IntelliTect.Coalesce.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IntelliTect.Coalesce;
using System.Threading.Tasks;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System;

namespace Coalesce.Domain
{
    [Table("Company")]
    public class Company
    {
#nullable disable
        [Column("CompanyId")]
        public int Id { get; set; }


        [Search(SearchMethod = SearchAttribute.SearchMethods.Contains)]
#if NET7_0_OR_GREATER
        required
#endif
        public string Name
        { get; set; }

        public string Address1 { get; init; }

        public string Address2 { get; set; }
        [Hidden(HiddenAttribute.Areas.List)]
        public string City { get; set; }
        [Hidden(HiddenAttribute.Areas.Edit)]
        public string State { get; set; }
        [Hidden(HiddenAttribute.Areas.All)]
        public string ZipCode { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        [Url]
        public string WebsiteUrl { get; set; }

        [DataType(DataType.ImageUrl)]
        [Search(SearchMethod = SearchAttribute.SearchMethods.BeginsWith)]
        public Uri LogoUrl { get; set; }

        public bool IsDeleted { get; set; }

        [InverseProperty("Company")]
        public ICollection<Person> Employees { get; set; }

        [NotMapped]
        [ListText]
        public string AltName => Name + ": " + City;

#nullable enable


        [Coalesce]
        public void ConflictingParameterNames(Company companyParam, string name)
        {

        }

        [Coalesce]
        public static ICollection<Company> GetCertainItems(AppDbContext db, bool isDeleted = false)
        {
            return db.Companies.Where(f => f.IsDeleted == isDeleted).ToList();
        }

        [DefaultDataSource]
        [SemanticKernel("")]
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