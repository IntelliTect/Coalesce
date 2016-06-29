using Intellitect.ComponentModel.Data;
using Intellitect.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Coalesce.Domain
{
    [Edit(AllowAnonymous = true)]
    [Table("Person")]
    public class Person : IIncludable<Person>
    {
        public enum Genders
        {
            NonSpecified = 0,
            Male = 1,
            Female = 2
        }

        public enum Titles
        {
            Mr = 0,
            Ms = 1,
            Mrs = 2,
            Miss = 4
        }

        public Person()
        {
            //Address = new Address();
        }
        /// <summary>
        /// ID for the person object.
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// Title of the person, Mr. Mrs, etc.
        /// </summary>
        [Display(Order = 1)]
        public Titles Title { get; set; }

        /// <summary>
        /// First name of the person.
        /// </summary>
        [Display(Order = 2)]
        [MinLength(2)]
        [MaxLength(length: 75)]
        [Search]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name of the person
        /// </summary>
        [Display(Order = 3)]
        [MinLength(length: 3)]
        [MaxLength(100)]
        [Edit(Roles = "Admin")]
        [Search]
        public string LastName { get; set; }

        /// <summary>
        /// Email address of the person
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Genetic Gender of the person. 
        /// </summary>
        [Read(Roles = "Admin")]
        public Genders Gender { get; set; }

        /// <summary>
        /// List of cases assigned to the person
        /// </summary>
        [InverseProperty("AssignedTo")]
        public ICollection<Case> CasesAssigned { get; set; }

        /// <summary>
        /// List of cases reported by the person.
        /// </summary>
        [InverseProperty("ReportedBy")]
        public ICollection<Case> CasesReported { get; set; }

        [DateType(DateTypeAttribute.DateTypes.DateOnly)]
        public DateTime? BirthDate { get; set; }
        public DateTime? LastBath { get; set; }
        public DateTimeOffset? NextUpgrade { get; set; }

        public int PersonStatsId { get; set; }
        public PersonStats PersonStats {get
            {
                return new PersonStats { Height = 10, Weight = 20, PersonStatsId = 1 };
            }
        }

        [NotMapped]
        public TimeZoneInfo TimeZone { get; set; }


        //public Address Address { get; set; }

        [InternalUse]
        [FileDownload]
        public byte[] ProfilePic { get; set; }

        /// <summary>
        /// Calculated name of the person. eg., Mr. Michael Stokesbary.
        /// </summary>
        [ListText]
        [NotMapped]
        public string Name
        {
            get
            {
                return String.Format("{0} {1} {2}", Title, FirstName, LastName);
            }
        }

        /// <summary>
        /// Company ID this person is employed by
        /// </summary>
        public int CompanyId { get; set; }
        /// <summary>
        /// Company loaded from the Company ID
        /// </summary>
        public Company Company { get; set; }

        /// <summary>
        /// Adds the text to the first name.
        /// </summary>
        /// <param name="addition"></param>
        /// <returns></returns>
        public Person Rename(string addition)
        {
            FirstName += addition;
            return this;
        }


        /// <summary>
        /// Removes spaces from the name and puts in dashes
        /// </summary>
        /// <param name="addition"></param>
        /// <returns></returns>
        public void ChangeSpacesToDashesInName()
        {
            FirstName = FirstName.Replace(" ", "-");
        }

        /// <summary>
        /// Adds two numbers.
        /// </summary>
        /// <param name="numberOne"></param>
        /// <param name="numberTwo"></param>
        /// <returns></returns>
        public static int Add(int numberOne, int numberTwo)
        {
            return numberOne + numberTwo;
        }

        /// <summary>
        /// Returns the user name
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        [Execute(Roles = "Admin")]
        public static string GetUser(ClaimsPrincipal user)
        {
            if (user!= null && user.Identity != null) return user.Identity.Name;
            return "Unknown";
        }


        /// <summary>
        /// Returns the user name
        /// </summary>
        /// <param name="User"></param>
        /// <returns></returns>
        public static string GetUserPublic(ClaimsPrincipal user)
        {
            if (user != null && user.Identity != null) return user.Identity.Name;
            return "Unknown";
        }

        /// <summary>
        /// Gets all the first names starting with the characters.
        /// </summary>
        /// <param name="characters"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        [Execute]
        public static IEnumerable<string> NamesStartingWith(string characters, AppDbContext db)
        {
            return db.People.Where(f => f.FirstName.StartsWith(characters)).Select(f => f.FirstName).ToList();
        }


        /// <summary>
        /// Gets all the first names starting with the characters.
        /// </summary>
        /// <param name="characters"></param>
        /// <param name="db"></param>
        /// <returns></returns>
        public static IEnumerable<string> NamesStartingWithPublic(string characters, AppDbContext db)
        {
            return db.People.Where(f => f.FirstName.StartsWith(characters)).Select(f => f.FirstName).ToList();
        }

        /// <summary>
        /// People whose last name starts with B or c
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        public static IQueryable<Person> BorCPeople(AppDbContext db)
        {
            return db.People.Where(f => f.LastName.StartsWith("B") || f.LastName.StartsWith("c"));
        }
 

        public IQueryable<Person> Include(IQueryable<Person> entities, string include = null)
        {
            return entities.Include(f => f.CasesAssigned)
                .Include(f => f.CasesReported)
                .Include(f => f.Company);
        }
    }
}
