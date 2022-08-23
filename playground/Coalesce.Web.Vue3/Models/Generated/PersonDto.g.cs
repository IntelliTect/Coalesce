using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Web.Vue3.Models
{
    public partial class PersonDtoGen : GeneratedDto<Coalesce.Domain.Person>
    {
        public PersonDtoGen() { }

        private int? _PersonId;
        private Coalesce.Domain.Person.Titles? _Title;
        private string _FirstName;
        private string _LastName;
        private string _Email;
        private Coalesce.Domain.Person.Genders? _Gender;
        private double? _Height;
        private System.Collections.Generic.ICollection<Coalesce.Web.Vue3.Models.CaseDtoGen> _CasesAssigned;
        private System.Collections.Generic.ICollection<Coalesce.Web.Vue3.Models.CaseDtoGen> _CasesReported;
        private System.DateTime? _BirthDate;
        private System.DateTime? _LastBath;
        private System.DateTimeOffset? _NextUpgrade;
        private Coalesce.Web.Vue3.Models.PersonStatsDtoGen _PersonStats;
        private string _Name;
        private int? _CompanyId;
        private Coalesce.Web.Vue3.Models.CompanyDtoGen _Company;
        private System.Collections.Generic.ICollection<string> _ArbitraryCollectionOfStrings;

        public int? PersonId
        {
            get => _PersonId;
            set { _PersonId = value; Changed(nameof(PersonId)); }
        }
        public Coalesce.Domain.Person.Titles? Title
        {
            get => _Title;
            set { _Title = value; Changed(nameof(Title)); }
        }
        public string FirstName
        {
            get => _FirstName;
            set { _FirstName = value; Changed(nameof(FirstName)); }
        }
        public string LastName
        {
            get => _LastName;
            set { _LastName = value; Changed(nameof(LastName)); }
        }
        public string Email
        {
            get => _Email;
            set { _Email = value; Changed(nameof(Email)); }
        }
        public Coalesce.Domain.Person.Genders? Gender
        {
            get => _Gender;
            set { _Gender = value; Changed(nameof(Gender)); }
        }
        public double? Height
        {
            get => _Height;
            set { _Height = value; Changed(nameof(Height)); }
        }
        public System.Collections.Generic.ICollection<Coalesce.Web.Vue3.Models.CaseDtoGen> CasesAssigned
        {
            get => _CasesAssigned;
            set { _CasesAssigned = value; Changed(nameof(CasesAssigned)); }
        }
        public System.Collections.Generic.ICollection<Coalesce.Web.Vue3.Models.CaseDtoGen> CasesReported
        {
            get => _CasesReported;
            set { _CasesReported = value; Changed(nameof(CasesReported)); }
        }
        public System.DateTime? BirthDate
        {
            get => _BirthDate;
            set { _BirthDate = value; Changed(nameof(BirthDate)); }
        }
        public System.DateTime? LastBath
        {
            get => _LastBath;
            set { _LastBath = value; Changed(nameof(LastBath)); }
        }
        public System.DateTimeOffset? NextUpgrade
        {
            get => _NextUpgrade;
            set { _NextUpgrade = value; Changed(nameof(NextUpgrade)); }
        }
        public Coalesce.Web.Vue3.Models.PersonStatsDtoGen PersonStats
        {
            get => _PersonStats;
            set { _PersonStats = value; Changed(nameof(PersonStats)); }
        }
        public string Name
        {
            get => _Name;
            set { _Name = value; Changed(nameof(Name)); }
        }
        public int? CompanyId
        {
            get => _CompanyId;
            set { _CompanyId = value; Changed(nameof(CompanyId)); }
        }
        public Coalesce.Web.Vue3.Models.CompanyDtoGen Company
        {
            get => _Company;
            set { _Company = value; Changed(nameof(Company)); }
        }
        public System.Collections.Generic.ICollection<string> ArbitraryCollectionOfStrings
        {
            get => _ArbitraryCollectionOfStrings;
            set { _ArbitraryCollectionOfStrings = value; Changed(nameof(ArbitraryCollectionOfStrings)); }
        }

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
            this.Height = obj.Height;
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

            if (ShouldMapTo(nameof(PersonId))) entity.PersonId = (PersonId ?? entity.PersonId);
            if (ShouldMapTo(nameof(Title))) entity.Title = Title;
            if (ShouldMapTo(nameof(FirstName))) entity.FirstName = FirstName;
            if (ShouldMapTo(nameof(LastName))) entity.LastName = LastName;
            if (ShouldMapTo(nameof(Email))) entity.Email = Email;
            if (ShouldMapTo(nameof(Gender))) entity.Gender = (Gender ?? entity.Gender);
            if (ShouldMapTo(nameof(Height))) entity.Height = (Height ?? entity.Height);
            if (ShouldMapTo(nameof(BirthDate))) entity.BirthDate = BirthDate;
            if (ShouldMapTo(nameof(LastBath))) entity.LastBath = LastBath;
            if (ShouldMapTo(nameof(NextUpgrade))) entity.NextUpgrade = NextUpgrade;
            if (ShouldMapTo(nameof(CompanyId))) entity.CompanyId = (CompanyId ?? entity.CompanyId);
            if (ShouldMapTo(nameof(ArbitraryCollectionOfStrings))) entity.ArbitraryCollectionOfStrings = ArbitraryCollectionOfStrings;
        }
    }
}
