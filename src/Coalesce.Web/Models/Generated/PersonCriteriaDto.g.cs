using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Web.Models
{
    public partial class PersonCriteriaDtoGen : GeneratedDto<Coalesce.Domain.PersonCriteria>
    {
        public PersonCriteriaDtoGen() { }

        public string Name { get; set; }
        public int? BirthdayMonth { get; set; }
        public string EmailDomain { get; set; }

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

            entity.Name = Name;
            entity.BirthdayMonth = BirthdayMonth;
            entity.EmailDomain = EmailDomain;
        }
    }
}
