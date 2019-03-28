using IntelliTect.Coalesce;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Domain
{
    [Edit(PermissionLevel = SecurityPermissionLevels.AllowAll)]
    [Table("Person")]
    [TypeScriptPartial]
    public class Person
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
        public Titles? Title { get; set; }

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
        [Search]
        public string LastName { get; set; }

        /// <summary>
        /// Email address of the person
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Genetic Gender of the person. 
        /// </summary>
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

        [Hidden]
        public DateTime? LastBath { get; set; }

        [Hidden]
        public DateTimeOffset? NextUpgrade { get; set; }


        [Hidden]
        public PersonStats PersonStats => new PersonStats { Name = Name, Height = 10, Weight = 20 };

        [InternalUse]
        public byte[] ProfilePic { get; set; }

        /// <summary>
        /// Calculated name of the person. eg., Mr. Michael Stokesbary.
        /// </summary>
        [ListText]
        [NotMapped]
        public string Name => $"{Title} {FirstName} {LastName}";

        /// <summary>
        /// Company ID this person is employed by
        /// </summary>
        [ClientValidation(IsRequired = true, AllowSave = false)]
        public int CompanyId { get; set; }

        /// <summary>
        /// Company loaded from the Company ID
        /// </summary>
        public Company Company { get; set; }

        /// <summary>
        /// Sets the FirstName to the given text.
        /// </summary>
        [Coalesce]
        public Person Rename(string name)
        {
            FirstName = name;
            return this;
        }

        // Ad-hoc test case for https://github.com/IntelliTect/Coalesce/issues/109
        // Migrate to a real test case once real tests exist for the code gen.
        [NotMapped]
        public ICollection<string> ArbitraryCollectionOfStrings { get; set; }

        /// <summary>
        /// Removes spaces from the name and puts in dashes
        /// </summary>
        [Coalesce, LoadFromDataSource(typeof(WithoutCases))]
        public ItemResult ChangeSpacesToDashesInName()
        {
            FirstName = FirstName.Replace(" ", "-");
            return true;
        }

        /// <summary>
        /// Adds two numbers.
        /// </summary>
        /// <param name="numberOne"></param>
        /// <param name="numberTwo"></param>
        /// <returns></returns>
        [Coalesce]
        public static ItemResult<int> Add(int numberOne, int numberTwo)
        {
            try
            {
                return new ItemResult<int>(numberOne + numberTwo);
            }
            catch
            {
                return "Integers too large";
            }
        }

        /// <summary>
        /// Returns the user name
        /// </summary>
        [Coalesce,Execute(Roles = "Admin")]
        public static string GetUser(ClaimsPrincipal user)
        {
            return user?.Identity?.Name ?? "Unknown";
        }

        [Coalesce]
        [ControllerAction(Method = HttpMethod.Get)]
        public static long PersonCount(AppDbContext db, string lastNameStartsWith = "")
        {
            return db.People.Count(f=>f.LastName.StartsWith(lastNameStartsWith));
        }

        [Coalesce]
        [ControllerAction(Method = HttpMethod.Get)]
        public string FullNameAndAge(AppDbContext db)
        {
            return $"{FirstName} {LastName} {BirthDate?.ToShortDateString() ?? "None"}";
        }

        [Coalesce]
        [ControllerAction(Method = HttpMethod.Delete)]
        public static bool RemovePersonById(AppDbContext db, int id)
        {
            var person = db.People.FirstOrDefault(f => f.PersonId == id);
            if (person != null)
            {
                db.People.Remove(person);
                foreach (var c in db.Cases.Where(f => f.AssignedToId == id))
                {
                    c.AssignedToId = null;
                }
                foreach (var c in db.Cases.Where(f => f.ReportedById == id))
                {
                    c.ReportedById = null;
                }
                db.SaveChanges();
                return true;
            }
            return false;
        }

        [Coalesce]
        [ControllerAction(Method = HttpMethod.Put)]
        public string ObfuscateEmail(AppDbContext db)
        {
            var random = (new Random()).Next();
            this.Email = $"test{random}@test.com";
            return $"New Email is: {this.Email}";
        }

        [Coalesce]
        [ControllerAction(Method = HttpMethod.Patch)]
        public Person ChangeFirstName (string firstName)
        {
            this.FirstName = firstName;
            return this;
        }



        /// <summary>
        /// Returns the user name
        /// </summary>
        [Coalesce]
        public static string GetUserPublic(ClaimsPrincipal user)
        {
            return user?.Identity?.Name ?? "Unknown";
        }

        /// <summary>
        /// Gets all the first names starting with the characters.
        /// </summary>
        [Coalesce,Execute]
        public static IEnumerable<string> NamesStartingWith(AppDbContext db, string characters)
        {
            return db.People.Where(f => f.FirstName.StartsWith(characters)).Select(f => f.Name).ToList();
        }


        /// <summary>
        /// Gets people matching the criteria, paginated by parameter 'page'.
        /// </summary>
        [Coalesce]
        public static ListResult<Person> SearchPeople(AppDbContext db, PersonCriteria criteria, int page)
        {
            const int pageSize = 10;
            IQueryable<Person> query = db.People;

            if (!string.IsNullOrEmpty(criteria.Name))
            {
                query = query.Where(f => f.FirstName.StartsWith(criteria.Name) || f.LastName.StartsWith(criteria.Name));
            }
            if (criteria.BirthdayMonth >= 1 && criteria.BirthdayMonth >= 12)
            {
                query = query.Where(f => f.BirthDate != null && f.BirthDate.Value.Month == criteria.BirthdayMonth);
            }
            if (!string.IsNullOrWhiteSpace(criteria.EmailDomain))
            {
                query = query.Where(f => f.Email.Contains($"@{criteria.EmailDomain}"));
            }

            return new ListResult<Person>(query, page, pageSize);
        }

        [Coalesce, DefaultDataSource]
        public class WithoutCases : StandardDataSource<Person, AppDbContext>
        {
            public WithoutCases(CrudContext<AppDbContext> context) : base(context) { }

            public override IQueryable<Person> GetQuery(IDataSourceParameters parameters)
                //=> Db.People.Include(p => p.Company);
                => Db.People.IncludeChildren();
        }

        public class Behaviors : StandardBehaviors<Person, AppDbContext>
        {
            public Behaviors(CrudContext<AppDbContext> context) : base(context) { }

            public override ItemResult BeforeSave(SaveKind kind, Person oldItem, Person item)
            {
                if (kind == SaveKind.Update && item.FirstName != null && item.FirstName.Length < 2)
                {
                    return "First Name must be at least 2 characters.";
                }

                if (item.FirstName?.Contains("[user]") ?? false)
                {
                    item.FirstName = item.FirstName.Replace("[user]", User.Identity.Name);
                }
                return true;
            }
        }
    }


    [Coalesce]
    public class NamesStartingWithAWithCases : StandardDataSource<Person, AppDbContext>
    {
        public NamesStartingWithAWithCases(CrudContext<AppDbContext> context) : base(context) { }

        public override IQueryable<Person> GetQuery(IDataSourceParameters parameters)
        {
            Db.Cases
                .Include(c => c.CaseProducts).ThenInclude(cp => cp.Product)
                .Where(c => c.Status == Case.Statuses.Open || c.Status == Case.Statuses.InProgress)
                .Load();

            return Db.People
                .IncludedSeparately(f => f.CasesAssigned).ThenIncluded(c => c.CaseProducts).ThenIncluded(cp => cp.Product)
                .IncludedSeparately(f => f.CasesReported).ThenIncluded(c => c.CaseProducts).ThenIncluded(cp => cp.Product)
                .Where(f => f.FirstName.StartsWith("A"));
        }
    }

    /// <summary>
    /// People whose last name starts with B or c
    /// </summary>
    [Coalesce]
    public class BOrCPeople : StandardDataSource<Person, AppDbContext>
    {
        public BOrCPeople(CrudContext<AppDbContext> context) : base(context) { }

        public override IQueryable<Person> GetQuery(IDataSourceParameters parameters) => 
            Db.People.Where(f => f.LastName.StartsWith("B") || f.LastName.StartsWith("c"));
    }

    public class PersonCriteria
    {
        public string Name { get; set; }
        public int? BirthdayMonth { get; set; }
        public string EmailDomain { get; set; }
    }
}
