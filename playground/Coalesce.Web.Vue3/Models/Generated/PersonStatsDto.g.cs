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
    public partial class PersonStatsParameter : SparseDto, IGeneratedParameterDto<Coalesce.Domain.PersonStats>
    {
        public PersonStatsParameter() { }

        private int? _PersonId;
        private double? _Height;
        private double? _Weight;

        public int? PersonId
        {
            get => _PersonId;
            set { _PersonId = value; Changed(nameof(PersonId)); }
        }
        public double? Height
        {
            get => _Height;
            set { _Height = value; Changed(nameof(Height)); }
        }
        public double? Weight
        {
            get => _Weight;
            set { _Weight = value; Changed(nameof(Weight)); }
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public void MapTo(Coalesce.Domain.PersonStats entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (ShouldMapTo(nameof(PersonId))) entity.PersonId = (PersonId ?? entity.PersonId);
            if (ShouldMapTo(nameof(Height))) entity.Height = (Height ?? entity.Height);
            if (ShouldMapTo(nameof(Weight))) entity.Weight = (Weight ?? entity.Weight);
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public Coalesce.Domain.PersonStats MapToNew(IMappingContext context)
        {
            var entity = new Coalesce.Domain.PersonStats();
            MapTo(entity, context);
            return entity;
        }

        public Coalesce.Domain.PersonStats MapToModelOrNew(Coalesce.Domain.PersonStats obj, IMappingContext context)
        {
            if (obj is null) return MapToNew(context);
            MapTo(obj, context);
            return obj;
        }
    }

    public partial class PersonStatsResponse : IGeneratedResponseDto<Coalesce.Domain.PersonStats>
    {
        public PersonStatsResponse() { }

        public int? PersonId { get; set; }
        public double? Height { get; set; }
        public double? Weight { get; set; }
        public Coalesce.Web.Vue3.Models.PersonResponse Person { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public void MapFrom(Coalesce.Domain.PersonStats obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.PersonId = obj.PersonId;
            this.Height = obj.Height;
            this.Weight = obj.Weight;
            if (tree == null || tree[nameof(this.Person)] != null)
                this.Person = obj.Person.MapToDto<Coalesce.Domain.Person, PersonResponse>(context, tree?[nameof(this.Person)]);

        }
    }
}
