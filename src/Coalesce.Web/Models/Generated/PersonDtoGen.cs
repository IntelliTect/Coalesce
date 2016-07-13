
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Intellitect.ComponentModel.Interfaces;
using System.Linq;
// Model Namespaces
using Coalesce.Domain;
using Coalesce.Domain.External;
using static Coalesce.Domain.Person;

namespace Coalesce.Web.Models
{
    public partial class PersonDto : IClassDto
    {
        public PersonDto() { }

        public PersonDto(Person entity, ClaimsPrincipal user = null, string includes = null)
        {
            User = user;
            Includes = includes ?? "";

            // Applicable includes for Person
            bool includeEmailList = Includes == "EmailList";
			bool includeDocumentList = Includes == "DocumentList";

            // Applicable excludes for Person
            

            // Applicable roles for Person
            bool isAdmin = false;
			if (User != null)
			{
				isAdmin = User.IsInRole("Admin");
			}

			PersonId = entity.PersonId;
			Title = entity.Title;
			FirstName = entity.FirstName;
			LastName = entity.LastName;
          if ((includeEmailList || includeDocumentList))
            {
                Email = entity.Email;
            }
          if ((isAdmin))
            {
                Gender = entity.Gender;
            }
			CasesAssigned = entity.CasesAssigned;
			CasesReported = entity.CasesReported;
			BirthDate = entity.BirthDate;
			LastBath = entity.LastBath;
			NextUpgrade = entity.NextUpgrade;
			PersonStatsId = entity.PersonStatsId;
			PersonStats = entity.PersonStats;
			TimeZone = entity.TimeZone;
			ProfilePic = entity.ProfilePic;
			Name = entity.Name;
			CompanyId = entity.CompanyId;
			Company = entity.Company;
        }

        public ClaimsPrincipal User { get; set; }
        public string Includes { get; set; }
            
         public Int32? PersonId { get; set; }
         public Titles? Title { get; set; }
         public String FirstName { get; set; }
         public String LastName { get; set; }
         public String Email { get; set; }
         public Genders? Gender { get; set; }
         public ICollection<Case> CasesAssigned { get; set; }
         public ICollection<Case> CasesReported { get; set; }
         public Nullable<DateTime> BirthDate { get; set; }
         public Nullable<DateTime> LastBath { get; set; }
         public Nullable<DateTimeOffset> NextUpgrade { get; set; }
         public Int32? PersonStatsId { get; set; }
         public PersonStats PersonStats { get; set; }
         public TimeZoneInfo TimeZone { get; set; }
         public Byte[] ProfilePic { get; set; }
         public String Name { get; set; }
         public Int32? CompanyId { get; set; }
         public Company Company { get; set; }

        public void Update(object obj)
        {   
            if (User == null) throw new InvalidOperationException("Updating an entity requires the User property to be populated.");

            // Applicable includes for Person
            bool includeEmailList = Includes == "EmailList";
			bool includeDocumentList = Includes == "DocumentList";

            // Applicable excludes for Person
            

            // Applicable roles for Person
            bool isAdmin = false;
			if (User != null)
			{
				isAdmin = User.IsInRole("Admin");
			}


            Person entity = (Person)obj;

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
    }
}