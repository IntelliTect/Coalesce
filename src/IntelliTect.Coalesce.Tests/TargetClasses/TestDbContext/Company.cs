using System.Collections.Generic;
using IntelliTect.Coalesce.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using IntelliTect.Coalesce.Helpers;
using System.ComponentModel.DataAnnotations;
using System;

namespace IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext
{
    [Table("Company")]
    [Create(PermissionLevel = SecurityPermissionLevels.DenyAll)]
    [Read(NoAutoInclude = true)]
    public class Company
    {
        public int CompanyId { get; set; }
        [DefaultOrderBy]
        public string Name { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }

        [DataType(DataType.PhoneNumber)]
        public string Phone { get; set; }

        [DataType(DataType.Url)]
        public string WebsiteUrl { get; set; }

        [DataType(DataType.ImageUrl)]
        [Search(SearchMethod = SearchAttribute.SearchMethods.BeginsWith)]
        public Uri LogoUrl { get; set; }

        [InverseProperty("Company")]
        public ICollection<Person> Employees { get; set; }

        [NotMapped]
        [ListText]
        public string AltName => Name + ": " + City;
    }
}