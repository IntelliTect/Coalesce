using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Web.Vue.Models
{
    public partial class PersonCriteriaDtoGen : GeneratedDto<Coalesce.Domain.PersonCriteria>
    {
        public PersonCriteriaDtoGen() { }

        private string _Name;
        private int? _BirthdayMonth;
        private string _EmailDomain;

        public string Name
        {
            get => _Name;
            set { _Name = value; Changed(nameof(Name)); }
        }
        public int? BirthdayMonth
        {
            get => _BirthdayMonth;
            set { _BirthdayMonth = value; Changed(nameof(BirthdayMonth)); }
        }
        public string EmailDomain
        {
            get => _EmailDomain;
            set { _EmailDomain = value; Changed(nameof(EmailDomain)); }
        }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(Coalesce.Domain.PersonCriteria obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            // Fill the properties of the object.

            this.Name = obj.Name;
            this.BirthdayMonth = obj.BirthdayMonth;
            this.EmailDomain = obj.EmailDomain;
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(Coalesce.Domain.PersonCriteria entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            if (ShouldMapTo(nameof(Name))) entity.Name = Name;
            if (ShouldMapTo(nameof(BirthdayMonth))) entity.BirthdayMonth = BirthdayMonth;
            if (ShouldMapTo(nameof(EmailDomain))) entity.EmailDomain = EmailDomain;
        }
    }
}
