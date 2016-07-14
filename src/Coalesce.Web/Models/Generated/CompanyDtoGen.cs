
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
                                   Dictionary<string, object> objects = null) {
            // Return null of the object is null;
            if (obj == null) return null;
                        
            if (objects == null) objects = new Dictionary<string, object>();

            if (user == null) throw new InvalidOperationException("Updating an entity requires the User property to be populated.");

            includes = includes ?? "";

            // Applicable includes for Company
            

            // Applicable excludes for Company
            

            // Applicable roles for Company
            if (user != null)
			{
			}



            // See if the object is already created.
            string key = $"Company{obj.CompanyId}";
            if (objects.ContainsKey(key)) 
                return (CompanyDtoGen)objects[key];

            var newObject = new CompanyDtoGen();
            objects.Add(key, newObject);
            // Fill the properties of the object.
            newObject.Name = obj.Name;
            newObject.Address1 = obj.Address1;
            newObject.Address2 = obj.Address2;
            newObject.City = obj.City;
            newObject.State = obj.State;
            newObject.ZipCode = obj.ZipCode;
            if (obj.Employees != null) newObject.Employees = obj.Employees.Select(f => PersonDtoGen.Create(f, user, includes, objects)).ToList();
            return newObject;
        }

        // Instance constructor because there is no way to implement a static interface in C#.
        public CompanyDtoGen CreateInstance(Company obj, ClaimsPrincipal user = null, string includes = null,
                                Dictionary<string, object> objects = null) {
            return Create(obj, user, includes, objects);
        }

        // Updates an object from the database to the state handed in by the DTO.
        public void Update(Company entity, ClaimsPrincipal user = null, string includes = null)
        {
        if (user == null) throw new InvalidOperationException("Updating an entity requires the User property to be populated.");

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
