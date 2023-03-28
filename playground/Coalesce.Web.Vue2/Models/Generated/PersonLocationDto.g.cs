using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Web.Vue2.Models
{
    public partial class PersonLocationDtoGen : GeneratedDto<Coalesce.Domain.PersonLocation>
    {
        public PersonLocationDtoGen() { }

        private double? _Latitude;
        private double? _Longitude;

        public double? Latitude
        {
            get => _Latitude;
            set { _Latitude = value; Changed(nameof(Latitude)); }
        }
        public double? Longitude
        {
            get => _Longitude;
            set { _Longitude = value; Changed(nameof(Longitude)); }
        }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(Coalesce.Domain.PersonLocation obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.Latitude = obj.Latitude;
            this.Longitude = obj.Longitude;
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(Coalesce.Domain.PersonLocation entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            if (ShouldMapTo(nameof(Latitude))) entity.Latitude = (Latitude ?? entity.Latitude);
            if (ShouldMapTo(nameof(Longitude))) entity.Longitude = (Longitude ?? entity.Longitude);
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public override Coalesce.Domain.PersonLocation MapToNew(IMappingContext context)
        {
            var entity = new Coalesce.Domain.PersonLocation();
            MapTo(entity, context);
            return entity;
        }
    }
}
