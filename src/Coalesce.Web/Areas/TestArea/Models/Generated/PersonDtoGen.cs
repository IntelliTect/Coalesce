
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
using static Coalesce.Domain.Person;

namespace Coalesce.Web.TestArea.Models
{
    public partial class PersonDtoGen : GeneratedDto<Person, PersonDtoGen>, IClassDto
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
         public Int32? PersonStatsId { get; set; }
         public PersonStats PersonStats { get; set; }
         public TimeZoneInfo TimeZone { get; set; }
         public Byte[] ProfilePic { get; set; }
         public String Name { get; set; }
         public Int32? CompanyId { get; set; }
         public CompanyDtoGen Company { get; set; }

        public void Update(object obj, ClaimsPrincipal user = null, string includes = null)
        {
            if (user == null) throw new InvalidOperationException("Updating an entity requires the User property to be populated.");

            includes = includes ?? "";

            Person entity = (Person)obj;

            if (OnUpdate(entity, user, includes)) return;

            // Applicable includes for Person
            bool includeEmailList = includes == "EmailList";
			bool includeDocumentList = includes == "DocumentList";

            // Applicable excludes for Person
            

            // Applicable roles for Person
            bool isAdmin = false;
			if (user != null)
			{
				isAdmin = user.IsInRole("Admin");
			}
    
			entity.Title = (Titles)Title;
			entity.FirstName = FirstName;
          if ((isAdmin))
            {
                entity.LastName = LastName;
            }
          if ((includeEmailList || includeDocumentList))
            {
                entity.Email = Email;
            }
			entity.BirthDate = BirthDate;
			entity.LastBath = LastBath;
			entity.NextUpgrade = NextUpgrade;
			entity.PersonStatsId = (Int32)PersonStatsId;
			entity.TimeZone = TimeZone;
			entity.CompanyId = (Int32)CompanyId;
        }

        public void SecurityTrim(ClaimsPrincipal user = null, string includes = null)
        {
            if (OnSecurityTrim(user, includes)) return;

            // Applicable includes for Person
            bool includeEmailList = includes == "EmailList";
			bool includeDocumentList = includes == "DocumentList";

            // Applicable excludes for Person
            

            // Applicable roles for Person
            bool isAdmin = false;
			if (user != null)
			{
				isAdmin = user.IsInRole("Admin");
			}

          if (!(includeEmailList || includeDocumentList))
            {
                Email = null;
            }
          if (!(isAdmin))
            {
                Gender = null;
            }
        }
    }
}
