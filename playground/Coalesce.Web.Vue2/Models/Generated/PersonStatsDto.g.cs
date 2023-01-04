using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Web.Vue2.Models
{
    public partial class PersonStatsDtoGen : GeneratedDto<Coalesce.Domain.PersonStats>
    {
        public PersonStatsDtoGen() { }

        private double? _Height;
        private double? _Weight;
        private string _Name;
        private System.Collections.Generic.ICollection<System.DateTimeOffset?> _NullableValueTypeCollection;
        private System.Collections.Generic.ICollection<System.DateTimeOffset> _ValueTypeCollection;

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
        public string Name
        {
            get => _Name;
            set { _Name = value; Changed(nameof(Name)); }
        }
        public System.Collections.Generic.ICollection<System.DateTimeOffset?> NullableValueTypeCollection
        {
            get => _NullableValueTypeCollection;
            set { _NullableValueTypeCollection = value; Changed(nameof(NullableValueTypeCollection)); }
        }
        public System.Collections.Generic.ICollection<System.DateTimeOffset> ValueTypeCollection
        {
            get => _ValueTypeCollection;
            set { _ValueTypeCollection = value; Changed(nameof(ValueTypeCollection)); }
        }

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
            this.NullableValueTypeCollection = obj.NullableValueTypeCollection;
            this.ValueTypeCollection = obj.ValueTypeCollection;
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(Coalesce.Domain.PersonStats entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            if (ShouldMapTo(nameof(Height))) entity.Height = (Height ?? entity.Height);
            if (ShouldMapTo(nameof(Weight))) entity.Weight = (Weight ?? entity.Weight);
            if (ShouldMapTo(nameof(Name))) entity.Name = Name;
            if (ShouldMapTo(nameof(NullableValueTypeCollection))) entity.NullableValueTypeCollection = NullableValueTypeCollection;
            if (ShouldMapTo(nameof(ValueTypeCollection))) entity.ValueTypeCollection = ValueTypeCollection;
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public override Coalesce.Domain.PersonStats MapToNew(IMappingContext context)
        {
            var entity = new Coalesce.Domain.PersonStats();
            MapTo(entity, context);
            return entity;
        }
    }
}
