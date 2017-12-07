
// Model Namespaces 
using Coalesce.Domain;
using Coalesce.Web.Models;

namespace Coalesce.Web.Api
{
    public partial class PersonController
         : LocalBaseApiController<Person, PersonDtoGen>
    {
        protected override bool BeforeSave(PersonDtoGen dto, Person obj)
        {
            if (dto.FirstName != null && dto.FirstName.Contains("[user]"))
            {
                dto.FirstName = dto.FirstName.Replace("[user]", User.Identity.Name);
            }
            return true;
        }

        protected override bool AfterSave(PersonDtoGen dto, Person obj, Person orig, AppDbContext context)
        {
            // Add the company name to the last name if it changed.
            if (obj.CompanyId != orig.CompanyId && !string.IsNullOrEmpty(obj.LastName) && !obj.LastName.Contains(obj.Company.Name))
            {
                // obj.LastName = obj.LastName + "-" + obj.Company.Name;
                Db.SaveChanges();
            }
            return true;
        }
    }

}