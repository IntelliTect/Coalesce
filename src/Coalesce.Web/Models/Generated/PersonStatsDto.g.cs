
using IntelliTect.Coalesce;
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
    public partial class PersonStatsDtoGen : GeneratedDto<Coalesce.Domain.PersonStats>
    {
        public PersonStatsDtoGen() { }

        public double? Height { get; set; }
        public double? Weight { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(Coalesce.Domain.PersonStats obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            // Fill the properties of the object.

            this.Height = obj.Height;
            this.Weight = obj.Weight;
            this.Name = obj.Name;
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(Coalesce.Domain.PersonStats entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            entity.Height = (Height ?? 0);
            entity.Weight = (Weight ?? 0);
            entity.Name = Name;
        }

    }
}
