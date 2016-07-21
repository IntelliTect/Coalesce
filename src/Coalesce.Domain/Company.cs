using System.Collections.Generic;
using IntelliTect.Coalesce.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Coalesce.Domain
{
    [Table("Company")]
    public class Company
    {
        public int CompanyId { get; set; }
        public string Name { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }

        [InverseProperty("Company")]
        public ICollection<Person> Employees { get; set; }

        [NotMapped]
        [ListText]
        public string AltName { get
            {
                return Name + ": " + City;
            }
        }
    }
}
