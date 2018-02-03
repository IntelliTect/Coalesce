
using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
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

        public override void MapFrom(Coalesce.Domain.Person obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            // Applicable includes for Person


            // Applicable excludes for Person


            // Applicable roles for Person


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
            var propValCasesAssigned = obj.CasesAssigned;
            if (propValCasesAssigned != null && (tree == null || tree[nameof(this.CasesAssigned)] != null))
            {
                this.CasesAssigned = propValCasesAssigned.AsQueryable().OrderBy("CaseKey ASC").ToList().Select(f => f.MapToDto<Coalesce.Domain.Case, CaseDtoGen>(context, tree?[nameof(this.CasesAssigned)])).ToList();
            }
            else if (propValCasesAssigned == null && tree?[nameof(this.CasesAssigned)] != null)
            {
                this.CasesAssigned = new CaseDtoGen[0];
            }

            var propValCasesReported = obj.CasesReported;
            if (propValCasesReported != null && (tree == null || tree[nameof(this.CasesReported)] != null))
            {
                this.CasesReported = propValCasesReported.AsQueryable().OrderBy("CaseKey ASC").ToList().Select(f => f.MapToDto<Coalesce.Domain.Case, CaseDtoGen>(context, tree?[nameof(this.CasesReported)])).ToList();
            }
            else if (propValCasesReported == null && tree?[nameof(this.CasesReported)] != null)
            {
                this.CasesReported = new CaseDtoGen[0];
            }


            this.PersonStats = obj.PersonStats.MapToDto<Coalesce.Domain.PersonStats, PersonStatsDtoGen>(context, tree?[nameof(this.PersonStats)]);

            if (tree == null || tree[nameof(this.Company)] != null)
                this.Company = obj.Company.MapToDto<Coalesce.Domain.Company, CompanyDtoGen>(context, tree?[nameof(this.Company)]);

        }

        // Updates an object from the database to the state handed in by the DTO.
        public override void MapTo(Coalesce.Domain.Person entity, IMappingContext context)
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
