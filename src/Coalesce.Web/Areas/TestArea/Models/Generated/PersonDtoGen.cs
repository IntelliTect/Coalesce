
using System;
using System.Collections.Generic;
using Intellitect.ComponentModel.Mapping;
// Model Namespaces
using Coalesce.Domain;
using Coalesce.Domain.External;
using static Coalesce.Domain.Person;

namespace Coalesce.Web.TestArea.Models
{
    public partial class PersonDto : IClassDto
    {
        public PersonDto() { }

        public PersonDto(Person entity)
        {
                PersonId = entity.PersonId;
                Title = entity.Title;
                FirstName = entity.FirstName;
                LastName = entity.LastName;
                Email = entity.Email;
                Gender = entity.Gender;
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
            Person entity = (Person)obj;

                entity.Title = (Titles)Title;
                entity.FirstName = FirstName;
                entity.LastName = LastName;
                entity.Email = Email;
                entity.Gender = (Genders)Gender;
                entity.CasesAssigned = CasesAssigned;
                entity.CasesReported = CasesReported;
                entity.BirthDate = BirthDate;
                entity.LastBath = LastBath;
                entity.NextUpgrade = NextUpgrade;
                entity.PersonStatsId = (Int32)PersonStatsId;
                entity.TimeZone = TimeZone;
                entity.ProfilePic = ProfilePic;
                entity.CompanyId = (Int32)CompanyId;
                entity.Company = Company;
        }
    }
}
