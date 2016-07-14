
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Intellitect.ComponentModel.Interfaces;
using Intellitect.ComponentModel.Models;
using Intellitect.ComponentModel.Mapping;
using System.Linq;
using Newtonsoft.Json;
// Model Namespaces
using Coalesce.Domain;
using Coalesce.Domain.External;
using static Coalesce.Domain.Company;

namespace Coalesce.Web.Models
{
    public partial class CompanyDtoGen : GeneratedDto<Company, CompanyDtoGen>, IClassDto
    {
        public CompanyDtoGen() { }

         public Int32? CompanyId { get; set; }
         public String Name { get; set; }
         public String Address1 { get; set; }
         public String Address2 { get; set; }
         public String City { get; set; }
         public String State { get; set; }
         public String ZipCode { get; set; }
         public ICollection<PersonDtoGen> Employees { get; set; }
         public String AltName { get; set; }

        public void Update(object obj, ClaimsPrincipal user = null, string includes = null)
        {
            if (user == null) throw new InvalidOperationException("Updating an entity requires the User property to be populated.");

            includes = includes ?? "";

            Company entity = (Company)obj;

            if (OnUpdate(entity, user, includes)) return;

            // Applicable includes for Company
            

            // Applicable excludes for Company
            

            // Applicable roles for Company
            if (user != null)
			{
			}
    
			entity.Name = Name;
			entity.Address1 = Address1;
			entity.Address2 = Address2;
			entity.City = City;
			entity.State = State;
			entity.ZipCode = ZipCode;
        }

        public void SecurityTrim(ClaimsPrincipal user = null, string includes = null)
        {
            if (OnSecurityTrim(user, includes)) return;

            // Applicable includes for Company
            

            // Applicable excludes for Company
            

            // Applicable roles for Company
            if (user != null)
			{
			}

        }
    }
}
