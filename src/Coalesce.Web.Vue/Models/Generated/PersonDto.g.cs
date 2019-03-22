using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Web.Vue.Models
{
    public partial class PersonDtoGen : GeneratedDto<Coalesce.Domain.Person>
    {
        public PersonDtoGen() { }

        public int? PersonId { get; set; }
        public Coalesce.Domain.Person.Titles? Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public Coalesce.Domain.Person.Genders? Gender { get; set; }
        public System.Collections.Generic.ICollection<Coalesce.Web.Vue.Models.CaseDtoGen> CasesAssigned { get; set; }
        public System.Collections.Generic.ICollection<Coalesce.Web.Vue.Models.CaseDtoGen> CasesReported { get; set; }
        public System.DateTime? BirthDate { get; set; }
        public System.DateTime? LastBath { get; set; }
        public System.DateTimeOffset? NextUpgrade { get; set; }
        public Coalesce.Web.Vue.Models.PersonStatsDtoGen PersonStats { get; set; }
        public string Name { get; set; }
        public int? CompanyId { get; set; }
        public Coalesce.Web.Vue.Models.CompanyDtoGen Company { get; set; }
        public System.Collections.Generic.ICollection<string> ArbitraryCollectionOfStrings { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(Coalesce.Domain.Person obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            // Fill the properties of the object.

            this.PersonId = obj.PersonId;
            this.Title = obj.Title;
            this.FirstName = obj.FirstName;
            this.LastName = obj.LastName;
            this.Email = obj.Email;
            this.Gender = obj.Gender;
            this.BirthDate = obj.BirthDate;
            this.LastBath = obj.LastBath;
            this.NextUpgrade = obj.NextUpgrade;
            this.Name = obj.Name;
            this.CompanyId = obj.CompanyId;
            this.ArbitraryCollectionOfStrings = obj.ArbitraryCollectionOfStrings;
            var propValCasesAssigned = obj.CasesAssigned;
            if (propValCasesAssigned != null && (tree == null || tree[nameof(this.CasesAssigned)] != null))
            {
                this.CasesAssigned = propValCasesAssigned
                    .OrderByDescending(f => f.OpenedAt).ThenBy(f => (f.AssignedTo == null ? "" : f.AssignedTo.FirstName)).ThenBy(f => f.CaseKey)
                    .Select(f => f.MapToDto<Coalesce.Domain.Case, CaseDtoGen>(context, tree?[nameof(this.CasesAssigned)])).ToList();
            }
            else if (propValCasesAssigned == null && tree?[nameof(this.CasesAssigned)] != null)
            {
                this.CasesAssigned = new CaseDtoGen[0];
            }

            var propValCasesReported = obj.CasesReported;
            if (propValCasesReported != null && (tree == null || tree[nameof(this.CasesReported)] != null))
            {
                this.CasesReported = propValCasesReported
                    .OrderByDescending(f => f.OpenedAt).ThenBy(f => (f.AssignedTo == null ? "" : f.AssignedTo.FirstName)).ThenBy(f => f.CaseKey)
                    .Select(f => f.MapToDto<Coalesce.Domain.Case, CaseDtoGen>(context, tree?[nameof(this.CasesReported)])).ToList();
            }
            else if (propValCasesReported == null && tree?[nameof(this.CasesReported)] != null)
            {
                this.CasesReported = new CaseDtoGen[0];
            }


            this.PersonStats = obj.PersonStats.MapToDto<Coalesce.Domain.PersonStats, PersonStatsDtoGen>(context, tree?[nameof(this.PersonStats)]);

            if (tree == null || tree[nameof(this.Company)] != null)
                this.Company = obj.Company.MapToDto<Coalesce.Domain.Company, CompanyDtoGen>(context, tree?[nameof(this.Company)]);

        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(Coalesce.Domain.Person entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            entity.PersonId = (PersonId ?? entity.PersonId);
            entity.Title = (Title ?? entity.Title);
            entity.FirstName = FirstName;
            entity.LastName = LastName;
            entity.Email = Email;
            entity.Gender = (Gender ?? entity.Gender);
            entity.BirthDate = BirthDate;
            entity.LastBath = LastBath;
            entity.NextUpgrade = NextUpgrade;
            entity.CompanyId = (CompanyId ?? entity.CompanyId);
        }
    }
}
