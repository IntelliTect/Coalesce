using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Coalesce.Web.Vue3.Models
{
    public partial class PersonLocationParameter : SparseDto, IGeneratedParameterDto<Coalesce.Domain.PersonLocation>
    {
        public PersonLocationParameter() { }

        private int? _PersonLocationId;
        private int? _PersonId;
        private double? _Latitude;
        private double? _Longitude;

        public int? PersonLocationId
        {
            get => _PersonLocationId;
            set { _PersonLocationId = value; Changed(nameof(PersonLocationId)); }
        }
        public int? PersonId
        {
            get => _PersonId;
            set { _PersonId = value; Changed(nameof(PersonId)); }
        }
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
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public void MapTo(Coalesce.Domain.PersonLocation entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (ShouldMapTo(nameof(PersonLocationId))) entity.PersonLocationId = (PersonLocationId ?? entity.PersonLocationId);
            if (ShouldMapTo(nameof(PersonId))) entity.PersonId = PersonId;
            if (ShouldMapTo(nameof(Latitude))) entity.Latitude = (Latitude ?? entity.Latitude);
            if (ShouldMapTo(nameof(Longitude))) entity.Longitude = (Longitude ?? entity.Longitude);
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public Coalesce.Domain.PersonLocation MapToNew(IMappingContext context)
        {
            var entity = new Coalesce.Domain.PersonLocation();
            MapTo(entity, context);
            return entity;
        }

        public Coalesce.Domain.PersonLocation MapToModelOrNew(Coalesce.Domain.PersonLocation obj, IMappingContext context)
        {
            if (obj is null) return MapToNew(context);
            MapTo(obj, context);
            return obj;
        }
    }

    public partial class PersonLocationResponse : IGeneratedResponseDto<Coalesce.Domain.PersonLocation>
    {
        public PersonLocationResponse() { }

        public int? PersonLocationId { get; set; }
        public int? PersonId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public Coalesce.Web.Vue3.Models.PersonResponse Person { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public void MapFrom(Coalesce.Domain.PersonLocation obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.PersonLocationId = obj.PersonLocationId;
            this.PersonId = obj.PersonId;
            this.Latitude = obj.Latitude;
            this.Longitude = obj.Longitude;
            if (tree == null || tree[nameof(this.Person)] != null)
                this.Person = obj.Person.MapToDto<Coalesce.Domain.Person, PersonResponse>(context, tree?[nameof(this.Person)]);

        }
    }
}
