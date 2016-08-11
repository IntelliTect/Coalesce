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

using static Coalesce.Domain.Person;

namespace Coalesce.Web.Models
{
    public partial class PersonDtoGen : GeneratedDto<Person, PersonDtoGen>
        , IClassDto<Person, PersonDtoGen>
        {
        public PersonDtoGen() { }

             public Int32? PersonId { get; set; }
             public Titles? Title { get; set; }
             public String FirstName { get; set; }
             public String LastName { get; set; }
             public String Email { get; set; }
             public Genders? Gender { get; set; }
             public ICollection<CaseDtoGen> CasesAssigned { get; set; }
             public ICollection<CaseDtoGen> CasesReported { get; set; }
             public Nullable<DateTime> BirthDate { get; set; }
             public Nullable<DateTime> LastBath { get; set; }
             public Nullable<DateTimeOffset> NextUpgrade { get; set; }
             public Nullable<Int32> PersonStatsId { get; set; }
             public PersonStats PersonStats { get; set; }
             public TimeZoneInfo TimeZone { get; set; }
             public String Name { get; set; }
             public Int32? CompanyId { get; set; }
             public CompanyDtoGen Company { get; set; }

        // Create a new version of this object or use it from the lookup.
        public static PersonDtoGen Create(Person obj, ClaimsPrincipal user = null, string includes = null,
                                   Dictionary<object, object> objects = null) {
            // Return null of the object is null;
            if (obj == null) return null;
                        
            if (objects == null) objects = new Dictionary<object, object>();

            includes = includes ?? "";

            // Applicable includes for Person
            

            // Applicable excludes for Person
            

            // Applicable roles for Person
            bool isAdmin = false;
			if (user != null)
			{
				isAdmin = user.IsInRole("Admin");
			}



            // See if the object is already created.
            if (objects.ContainsKey(obj)) 
                return (PersonDtoGen)objects[obj];

            var newObject = new PersonDtoGen();
            objects.Add(obj, newObject);
            // Fill the properties of the object.
            newObject.PersonId = obj.PersonId;
            newObject.Title = obj.Title;
            newObject.FirstName = obj.FirstName;
            newObject.LastName = obj.LastName;
            newObject.Email = obj.Email;
          if ((isAdmin))
            {
                newObject.Gender = obj.Gender;
            }  
                        if (obj.CasesAssigned != null) newObject.CasesAssigned = obj.CasesAssigned.Select(f => CaseDtoGen.Create(f, user, includes, objects)).ToList();
            if (obj.CasesReported != null) newObject.CasesReported = obj.CasesReported.Select(f => CaseDtoGen.Create(f, user, includes, objects)).ToList();
            newObject.BirthDate = obj.BirthDate;
            newObject.LastBath = obj.LastBath;
            newObject.NextUpgrade = obj.NextUpgrade;
            newObject.PersonStatsId = obj.PersonStatsId;
            newObject.PersonStats = obj.PersonStats;
            newObject.TimeZone = obj.TimeZone;
            newObject.Name = obj.Name;
            newObject.CompanyId = obj.CompanyId;
            newObject.Company = CompanyDtoGen.Create(obj.Company, user, includes, objects);
            return newObject;
        }

        // Instance constructor because there is no way to implement a static interface in C#. And generic constructors don't take arguments.
        public PersonDtoGen CreateInstance(Person obj, ClaimsPrincipal user = null, string includes = null,
                                Dictionary<object, object> objects = null) {
            return Create(obj, user, includes, objects);
        }

        // Updates an object from the database to the state handed in by the DTO.
        public void Update(Person entity, ClaimsPrincipal user = null, string includes = null)
        {
            includes = includes ?? "";

            if (OnUpdate(entity, user, includes)) return;

            // Applicable includes for Person
            

            // Applicable excludes for Person
            

            // Applicable roles for Person
            bool isAdmin = false;
			if (user != null)
			{
				isAdmin = user.IsInRole("Admin");
			}

			entity.Title = (Titles)(Title ?? 0);
			entity.FirstName = FirstName;
          if ((isAdmin))
            {
                entity.LastName = LastName;
            }
			entity.Email = Email;
			entity.BirthDate = BirthDate;
			entity.LastBath = LastBath;
			entity.NextUpgrade = NextUpgrade;
			entity.PersonStatsId = PersonStatsId;
			entity.TimeZone = TimeZone;
			entity.CompanyId = (Int32)(CompanyId ?? 0);
        }

	}
}
