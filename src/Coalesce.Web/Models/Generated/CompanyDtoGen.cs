    using IntelliTect.Coalesce.Interfaces;
    using IntelliTect.Coalesce.Mapping;
    using IntelliTect.Coalesce.Models;
    using IntelliTect.Coalesce.Helpers.IncludeTree;
    using Newtonsoft.Json;
    using System;
    using System.Linq;
    using System.Linq.Dynamic;
    using System.Collections.Generic;
    using System.Security.Claims;
    using Coalesce.Domain.Models;
    using Coalesce.Domain;
    using Coalesce.Domain.External;

using static Coalesce.Domain.Company;

namespace Coalesce.Domain.Models
{
    public partial class CompanyDtoGen : GeneratedDto<Coalesce.Domain.Company, CompanyDtoGen>
        , IClassDto<Coalesce.Domain.Company, CompanyDtoGen>
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
        public static CompanyDtoGen Create(Coalesce.Domain.Company obj, ClaimsPrincipal user = null, string includes = null,
                                   Dictionary<object, object> objects = null, IncludeTree tree = null) {
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



            // See if the object is already created, but only if we aren't restricting by an includes tree.
            // If we do have an IncludeTree, we know the exact structure of our return data, so we don't need to worry about circular refs.
            if (tree == null && objects.ContainsKey(obj)) 
                return (CompanyDtoGen)objects[obj];

            var newObject = new CompanyDtoGen();
            if (tree == null) objects.Add(obj, newObject);
            // Fill the properties of the object.
            newObject.CompanyId = obj.CompanyId;
            newObject.Name = obj.Name;
            newObject.Address1 = obj.Address1;
            newObject.Address2 = obj.Address2;
            newObject.City = obj.City;
            newObject.State = obj.State;
            newObject.ZipCode = obj.ZipCode;
            newObject.AltName = obj.AltName;
            if (obj.Employees != null && (tree == null || tree[nameof(newObject.Employees)] != null)) {
                newObject.Employees = obj.Employees.OrderBy("PersonId ASC").Select(f => PersonDtoGen.Create(f, user, includes, objects, tree?[nameof(newObject.Employees)])).ToList();
            } else if (obj.Employees == null && tree?[nameof(newObject.Employees)] != null) {
                newObject.Employees = new PersonDtoGen[0];
            }

            return newObject;
        }

        // Instance constructor because there is no way to implement a static interface in C#. And generic constructors don't take arguments.
        public CompanyDtoGen CreateInstance(Coalesce.Domain.Company obj, ClaimsPrincipal user = null, string includes = null,
                                Dictionary<object, object> objects = null, IncludeTree tree = null) {
            return Create(obj, user, includes, objects, tree);
        }

        // Updates an object from the database to the state handed in by the DTO.
        public void Update(Coalesce.Domain.Company entity, ClaimsPrincipal user = null, string includes = null)
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
