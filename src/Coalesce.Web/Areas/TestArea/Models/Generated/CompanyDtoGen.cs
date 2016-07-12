
using System;
using System.Collections.Generic;
using Intellitect.ComponentModel.Mapping;
// Model Namespaces
using Coalesce.Domain;
using Coalesce.Domain.External;
using static Coalesce.Domain.Company;

namespace Coalesce.Web.TestArea.Models
{
    public partial class CompanyDto : IClassDto
    {
        public CompanyDto() { }

        public CompanyDto(Company entity)
        {
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
            Company entity = (Company)obj;

                entity.Name = Name;
                entity.Address1 = Address1;
                entity.Address2 = Address2;
                entity.City = City;
                entity.State = State;
                entity.ZipCode = ZipCode;
                entity.Employees = Employees;
        }
    }
}
