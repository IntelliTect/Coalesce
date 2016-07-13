
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Intellitect.ComponentModel.Interfaces;
using System.Linq;
// Model Namespaces
using Coalesce.Domain;
using Coalesce.Domain.External;
using static Coalesce.Domain.Company;

namespace Coalesce.Web.Models
{
    public partial class CompanyDto : IClassDto
    {
        public CompanyDto() { }

        public CompanyDto(Company entity, ClaimsPrincipal user = null, string includes = null)
        {
            User = user;
            Includes = includes ?? "";

            // Applicable includes for Company
            

            // Applicable excludes for Company
            

            // Applicable roles for Company
            if (User != null)
			{
			}

			CompanyId = entity.CompanyId;
			Name = entity.Name;
			Address1 = entity.Address1;
			Address2 = entity.Address2;
			City = entity.City;
			State = entity.State;
			ZipCode = entity.ZipCode;
			Employees = entity.Employees;
			AltName = entity.AltName;
        }

        public ClaimsPrincipal User { get; set; }
        public string Includes { get; set; }
            
         public Int32? CompanyId { get; set; }
         public String Name { get; set; }
         public String Address1 { get; set; }
         public String Address2 { get; set; }
         public String City { get; set; }
         public String State { get; set; }
         public String ZipCode { get; set; }
         public ICollection<Person> Employees { get; set; }
         public String AltName { get; set; }

        public void Update(object obj)
        {   
            if (User == null) throw new InvalidOperationException("Updating an entity requires the User property to be populated.");

            // Applicable includes for Company
            

            // Applicable excludes for Company
            

            // Applicable roles for Company
            if (User != null)
			{
			}


            Company entity = (Company)obj;

			entity.Name = Name;
			entity.Address1 = Address1;
			entity.Address2 = Address2;
			entity.City = City;
			entity.State = State;
			entity.ZipCode = ZipCode;
        }
    }
}