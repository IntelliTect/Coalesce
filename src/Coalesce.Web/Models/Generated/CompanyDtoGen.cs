    using IntelliTect.Coalesce.Interfaces;
    using IntelliTect.Coalesce.Mapping;
    using IntelliTect.Coalesce.Models;
    using Newtonsoft.Json;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Security.Claims;
    using Coalesce.Web.Models;
    using Coalesce.Domain;
    using Coalesce.Domain.External;

using static Coalesce.Domain.Company;

namespace Coalesce.Web.Models
{
    public partial class CompanyDtoGen : GeneratedDto<Company, CompanyDtoGen>
        , IClassDto<Company, CompanyDtoGen>
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

        // Create a new version of this object or use it from the lookup.
        public static CompanyDtoGen Create(Company obj, ClaimsPrincipal user = null, string includes = null,
                                   Dictionary<object, object> objects = null) {
            // Return null of the object is null;
            if (obj == null) return null;
                        
            if (objects == null) objects = new Dictionary<object, object>();

            includes = includes ?? "";

            // Applicable includes for Company
            

            // Applicable excludes for Company
            

            // Applicable roles for Company
            if (user != null)
			{
			}



            // See if the object is already created.
            if (objects.ContainsKey(obj)) 
                return (CompanyDtoGen)objects[obj];

            var newObject = new CompanyDtoGen();
            objects.Add(obj, newObject);
            // Fill the properties of the object.
            newObject.CompanyId = obj.CompanyId;
            newObject.Name = obj.Name;
            newObject.Address1 = obj.Address1;
            newObject.Address2 = obj.Address2;
            newObject.City = obj.City;
            newObject.State = obj.State;
            newObject.ZipCode = obj.ZipCode;
            if (obj.Employees != null) newObject.Employees = obj.Employees.Select(f => PersonDtoGen.Create(f, user, includes, objects)).ToList();
            newObject.AltName = obj.AltName;
            return newObject;
        }

        // Instance constructor because there is no way to implement a static interface in C#. And generic constructors don't take arguments.
        public CompanyDtoGen CreateInstance(Company obj, ClaimsPrincipal user = null, string includes = null,
                                Dictionary<object, object> objects = null) {
            return Create(obj, user, includes, objects);
        }

        // Updates an object from the database to the state handed in by the DTO.
        public void Update(Company entity, ClaimsPrincipal user = null, string includes = null)
        {
            includes = includes ?? "";

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

	}
}
