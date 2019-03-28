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
    public partial class LocationDtoGen : GeneratedDto<Coalesce.Domain.Services.Location>
    {
        public LocationDtoGen() { }

        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(Coalesce.Domain.Services.Location obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            // Fill the properties of the object.

            this.City = obj.City;
            this.State = obj.State;
            this.Zip = obj.Zip;
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(Coalesce.Domain.Services.Location entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            entity.City = City;
            entity.State = State;
            entity.Zip = Zip;
        }
    }
}
