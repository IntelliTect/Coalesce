
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

        public override void MapFrom(Coalesce.Domain.PersonStats obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;





            // Fill the properties of the object.
            this.Height = obj.Height;
            this.Weight = obj.Weight;
            this.Name = obj.Name;
        }

        // Updates an object from the database to the state handed in by the DTO.
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
