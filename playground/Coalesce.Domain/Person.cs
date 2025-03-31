using IntelliTect.Coalesce;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Domain
{
    [Edit(PermissionLevel = SecurityPermissionLevels.AllowAll)]
    [Table("Person")]
    public class Person
    {
#nullable disable
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
        [ClientValidation(IsEmail = true)]
        [DataType(DataType.EmailAddress)]
        [EmailAddress]
        public string Email { get; set; }

        /// <summary>
        /// Genetic Gender of the person. 
        /// </summary>
        [DefaultValue(Genders.NonSpecified)]
        public Genders Gender { get; set; }

        [NotMapped]
        public double Height { get; set; }

        /// <summary>
        /// List of cases assigned to the person
        /// </summary>
        [InverseProperty("AssignedTo")]
        public ICollection<Case> CasesAssigned { get; set; }

        /// <summary>
        /// List of cases reported by the person.
        /// </summary>
        [InverseProperty("ReportedBy")]
        public List<Case> CasesReported { get; set; }

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
        public string Name => $"{Title} {FirstName} {LastName}".Trim();

        /// <summary>
        /// Company ID this person is employed by
        /// </summary>
        [ClientValidation(IsRequired = true)]
        public int CompanyId { get; set; }

        /// <summary>
        /// Company loaded from the Company ID
        /// </summary>
        public Company Company { get; set; }

#if NET8_0_OR_GREATER
        // https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew#primitive-collections
#else
        [NotMapped]
#endif
        public List<string> ArbitraryCollectionOfStrings { get; set; }


#nullable restore

        /// <summary>
        /// Sets the FirstName to the given text.
        /// </summary>
        [Coalesce]
        public Person Rename(AppDbContext db, string name, out IncludeTree includeTree)
        {
            FirstName = name;
            db.SaveChanges();
            includeTree = IncludeTree.For<Person>(p => p.IncludedSeparately(x => x.Company));
            return this;
        }

        /// <summary>
        /// Removes spaces from the name and puts in dashes
        /// </summary>
        [Coalesce, Execute(DataSource = typeof(WithoutCases))]
        public ItemResult ChangeSpacesToDashesInName(AppDbContext db)
        {
            var old = FirstName;
            FirstName = FirstName.Replace(" ", "-");
            db.SaveChanges();
            return new ItemResult(true, $"Changed name from {old} to {FirstName}");
        }

        /// <summary>
        /// Adds two numbers.
        /// 
        /// This comment also includes multiple lines so I can test multi-line xmldoc comments.
        /// </summary>
        /// <param name="numberOne"></param>
        /// <param name="numberTwo"></param>
        /// <returns></returns>
        [Coalesce]
        public static ItemResult<int> Add(
            int numberOne, 
            [Range(0, 10000)] int numberTwo = 42
        )
        {
            try
            {
                checked
                {
                    return numberOne + numberTwo;
                }
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
        public DateTime GetBirthdate()
        {
            return BirthDate ?? DateTime.Now;
        }

        /// <summary>
        /// Returns the user name
        /// </summary>
        [Coalesce]
        public void SetBirthDate(AppDbContext db, DateOnly date, TimeOnly time)
        {
            BirthDate = date.ToDateTime(time);
            db.SaveChanges();
        }

        [Coalesce]
        [Execute(HttpMethod = HttpMethod.Get)]
        public static long PersonCount(AppDbContext db, string lastNameStartsWith = "")
        {
            return db.People.Count(f=>f.LastName.StartsWith(lastNameStartsWith));
        }

        [Coalesce]
        [Execute(HttpMethod = HttpMethod.Get)]
        public string FullNameAndAge(AppDbContext db)
        {
            return $"{FirstName} {LastName} {BirthDate?.ToShortDateString() ?? "None"}";
        }

        [Coalesce]
        [Execute(HttpMethod = HttpMethod.Delete)]
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
        [Execute(HttpMethod = HttpMethod.Put)]
        public string ObfuscateEmail(AppDbContext db)
        {
            var random = (new Random()).Next();
            this.Email = $"test{random}@test.com";
            db.SaveChanges();
            return $"New Email is: {this.Email}";
        }

        [Coalesce]
        [Execute(HttpMethod = HttpMethod.Patch)]
        public Person ChangeFirstName (AppDbContext db, string firstName, Titles? title)
        {
            this.FirstName = firstName;
            Title ??= title;
            db.SaveChanges();
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

        [Coalesce, Execute]
        public static string[] MethodWithStringArrayParameter(AppDbContext db, string[] strings)
        {
            return strings;
        }

        [Coalesce, Execute]
        public static Person MethodWithEntityParameter(AppDbContext db, Person person, Person[] people)
        {
            return person;
        }

        [Coalesce, Execute]
        public static Person? MethodWithOptionalEntityParameter(AppDbContext db, Person? person)
        {
            return person;
        }


        [Coalesce, Execute]
        public static Person? MethodWithExplicitlyInjectedDataSource(AppDbContext db, [Inject] Person.WithoutCases dataSource)
        {
            // This method is an ad-hoc test for CoalesceApiDescriptionProvider
            // to check that it handles DS parameters that aren't bound with our custom model binder correctly.
            return null;
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
        public class WithoutCases(CrudContext<AppDbContext> context) : StandardDataSource<Person, AppDbContext>(context)
        {
            [Coalesce]
            public PersonCriteria? PersonCriteria { get; set; }

            public override IQueryable<Person> GetQuery(IDataSourceParameters parameters)
                //=> Db.People.Include(p => p.Company);
                => Db.People.IncludeChildren();

            // https://stackoverflow.com/questions/11276964/how-to-slow-down-a-sql-query
            //=> Db.People.FromSql("SELECT top 1000000 T1.* FROM Person T1,Person T2, Person T3, Person T4 ORDER BY RAND(T1.PersonId)");
        }

        public class Behaviors : StandardBehaviors<Person, AppDbContext>
        {
            public Behaviors(CrudContext<AppDbContext> context) : base(context) { }

            public override ItemResult BeforeSave(SaveKind kind, Person? oldItem, Person item)
            {
                if (kind == SaveKind.Update && item.FirstName != null && item.FirstName.Length < 2)
                {
                    return "First Name must be at least 2 characters.";
                }

                if (item.FirstName?.Contains("[user]") ?? false)
                {
                    item.FirstName = item.FirstName.Replace("[user]", User?.Identity?.Name);
                }
                return true;
            }
        }
    }


    [Coalesce]
    public class NamesStartingWithAWithCases : StandardDataSource<Person, AppDbContext>
    {
        public NamesStartingWithAWithCases(CrudContext<AppDbContext> context) : base(context) { }

        [Coalesce]
        public List<Case.Statuses>? AllowedStatuses { get; set; }

        public override IQueryable<Person> GetQuery(IDataSourceParameters parameters)
        {
            Db.Cases
                .Include(c => c.CaseProducts).ThenInclude(cp => cp.Product)
                .Where(c => AllowedStatuses != null && AllowedStatuses.Contains(c.Status))
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
        public string? Name { get; set; }
        public int? BirthdayMonth { get; set; }
        public string? EmailDomain { get; set; }
    }
}
