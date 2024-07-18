﻿using IntelliTect.Coalesce;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext
{
    [Edit(PermissionLevel = SecurityPermissionLevels.AllowAll)]
    [Table("Person")]
#pragma warning disable CS0618 // Type or member is obsolete
    [TypeScriptPartial]
#pragma warning restore CS0618 // Type or member is obsolete
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
        [Search]
        public string LastName { get; set; }

        /// <summary>
        /// Email address of the person
        /// </summary>
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string SecretPhrase { get; set; }

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

        [ManyToMany("Siblings", FarNavigationProperty = nameof(Sibling.PersonTwo))]
        [InverseProperty(nameof(Sibling.Person))]
        public ICollection<Sibling> SiblingRelationships { get; set; }

        /// <summary>
        /// Sets the FirstName to the given text.
        /// </summary>
        [Coalesce]
        public Person Rename(string name)
        {
            FirstName = name;
            return this;
        }

        /// <summary>
        /// Removes spaces from the name and puts in dashes
        /// </summary>
        [Coalesce]
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
        [Coalesce]
        public static int Add(int numberOne, int numberTwo)
        {
            return numberOne + numberTwo;
        }

        /// <summary>
        /// Returns the user name
        /// </summary>
        [Coalesce,Execute(Roles = RoleNames.Admin)]
        public static string GetUser(ClaimsPrincipal user)
        {
            if (user!= null && user.Identity != null) return user.Identity.Name;
            return "Unknown";
        }


        /// <summary>
        /// Returns the user name
        /// </summary>
        [Coalesce]
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
        [Coalesce, Execute(HttpMethod = HttpMethod.Get)]
        public static IEnumerable<string> NamesStartingWith(string characters, AppDbContext db)
        {
            return db.People.Where(f => f.FirstName.StartsWith(characters)).Select(f => f.Name).ToList();
        }

        public async Task<ItemResult<Person>> MethodWithIncludeTreeOnItemResult(AppDbContext db)
        {
            var query = db.People.Include(p => p.Company);

            return new(
                await query.FindItemAsync(PersonId),
                includeTree: query.GetIncludeTree()
            );
        }

        [Coalesce, DefaultDataSource]
        public class WithoutCases : StandardDataSource<Person, AppDbContext>
        {
            public WithoutCases(CrudContext<AppDbContext> context) : base(context) { }

            public override IQueryable<Person> GetQuery(IDataSourceParameters parameters)
                => Db.People.Include(p => p.Company);
        }

        public class Behaviors : StandardBehaviors<Person, AppDbContext>
        {
            public Behaviors(CrudContext<AppDbContext> context) : base(context) { }

            public override ItemResult BeforeSave(SaveKind kind, Person originalItem, Person item)
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

        [Coalesce]
        public List<Case.Statuses> AllowedStatuses { get; set; }

        public override IQueryable<Person> GetQuery(IDataSourceParameters parameters)
        {
            Db.Cases
                .Include(c => c.CaseProducts).ThenInclude(cp => cp.Product)
                .Where(c => AllowedStatuses.Contains(c.Status))
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
    public class BorCPeople : StandardDataSource<Person, AppDbContext>
    {
        public BorCPeople(CrudContext<AppDbContext> context) : base(context) { }

        public override IQueryable<Person> GetQuery(IDataSourceParameters parameters) => 
            Db.People.Where(f => f.LastName.StartsWith("B") || f.LastName.StartsWith("c"));
    }
}
