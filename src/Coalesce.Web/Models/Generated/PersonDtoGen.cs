
using IntelliTect.Coalesce.Helpers.IncludeTree;
using IntelliTect.Coalesce.Interfaces;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;

namespace Coalesce.Web.Models
{
    public partial class PersonDtoGen : GeneratedDto<Coalesce.Domain.Person, PersonDtoGen>
        , IClassDto<Coalesce.Domain.Person, PersonDtoGen>
    {
        public PersonDtoGen() { }

        public int? PersonId { get; set; }
        public Coalesce.Domain.Person.Titles? Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Coalesce.Domain.Person.Genders? Gender { get; set; }
        public System.Collections.Generic.ICollection<Coalesce.Web.Models.CaseDtoGen> CasesAssigned { get; set; }
        public System.Collections.Generic.ICollection<Coalesce.Web.Models.CaseDtoGen> CasesReported { get; set; }
        public System.DateTime? BirthDate { get; set; }
        public System.DateTime? LastBath { get; set; }
        public System.DateTimeOffset? NextUpgrade { get; set; }
        public Coalesce.Web.Models.PersonStatsDtoGen PersonStats { get; set; }
        public string Name { get; set; }
        public int? CompanyId { get; set; }
        public Coalesce.Web.Models.CompanyDtoGen Company { get; set; }

        // Create a new version of this object or use it from the lookup.
        public static PersonDtoGen Create(Coalesce.Domain.Person obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return null;
            var includes = context.Includes;

            // Applicable includes for Person


            // Applicable excludes for Person


            // Applicable roles for Person



            // See if the object is already created, but only if we aren't restricting by an includes tree.
            // If we do have an IncludeTree, we know the exact structure of our return data, so we don't need to worry about circular refs.
            if (tree == null && context.TryGetMapping(obj, out PersonDtoGen existing)) return existing;

            var newObject = new PersonDtoGen();
            if (tree == null) context.AddMapping(obj, newObject);
            // Fill the properties of the object.
            newObject.PersonId = obj.PersonId;
            newObject.Title = obj.Title;
            newObject.FirstName = obj.FirstName;
            newObject.LastName = obj.LastName;
            newObject.Email = obj.Email;
            newObject.Gender = obj.Gender;
            newObject.BirthDate = obj.BirthDate;
            newObject.LastBath = obj.LastBath;
            newObject.NextUpgrade = obj.NextUpgrade;
            newObject.Name = obj.Name;
            newObject.CompanyId = obj.CompanyId;
            var propValCasesAssigned = obj.CasesAssigned;
            if (propValCasesAssigned != null && (tree == null || tree[nameof(newObject.CasesAssigned)] != null))
            {
                newObject.CasesAssigned = propValCasesAssigned.AsQueryable().OrderBy("CaseKey ASC").ToList().Select(f => CaseDtoGen.Create(f, context, tree?[nameof(newObject.CasesAssigned)])).ToList();
            }
            else if (propValCasesAssigned == null && tree?[nameof(newObject.CasesAssigned)] != null)
            {
                newObject.CasesAssigned = new CaseDtoGen[0];
            }

            var propValCasesReported = obj.CasesReported;
            if (propValCasesReported != null && (tree == null || tree[nameof(newObject.CasesReported)] != null))
            {
                newObject.CasesReported = propValCasesReported.AsQueryable().OrderBy("CaseKey ASC").ToList().Select(f => CaseDtoGen.Create(f, context, tree?[nameof(newObject.CasesReported)])).ToList();
            }
            else if (propValCasesReported == null && tree?[nameof(newObject.CasesReported)] != null)
            {
                newObject.CasesReported = new CaseDtoGen[0];
            }


            newObject.PersonStats = PersonStatsDtoGen.Create(obj.PersonStats, context, tree?[nameof(newObject.PersonStats)]);

            if (tree == null || tree[nameof(newObject.Company)] != null)
                newObject.Company = CompanyDtoGen.Create(obj.Company, context, tree?[nameof(newObject.Company)]);

            return newObject;
        }

        // Instance constructor because there is no way to implement a static interface in C#. And generic constructors don't take arguments.
        public PersonDtoGen CreateInstance(Coalesce.Domain.Person obj, IMappingContext context, IncludeTree tree = null)
        {
            return Create(obj, context, tree);
        }

        // Updates an object from the database to the state handed in by the DTO.
        public void Update(Coalesce.Domain.Person entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            // Applicable includes for Person


            // Applicable excludes for Person


            // Applicable roles for Person


            entity.Title = (Title ?? 0);
            entity.FirstName = FirstName;
            entity.LastName = LastName;
            entity.Email = Email;
            entity.Gender = (Gender ?? 0);
            entity.BirthDate = BirthDate;
            entity.LastBath = LastBath;
            entity.NextUpgrade = NextUpgrade;
            entity.CompanyId = (CompanyId ?? 0);
        }

    }
}
