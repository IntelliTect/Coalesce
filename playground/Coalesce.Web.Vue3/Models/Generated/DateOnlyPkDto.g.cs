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
    public partial class DateOnlyPkParameter : SparseDto, IGeneratedParameterDto<Coalesce.Domain.DateOnlyPk>
    {
        public DateOnlyPkParameter() { }

        private System.DateOnly? _DateOnlyPkId;
        private string _Name;

        public System.DateOnly? DateOnlyPkId
        {
            get => _DateOnlyPkId;
            set { _DateOnlyPkId = value; Changed(nameof(DateOnlyPkId)); }
        }
        public string Name
        {
            get => _Name;
            set { _Name = value; Changed(nameof(Name)); }
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public void MapTo(Coalesce.Domain.DateOnlyPk entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (ShouldMapTo(nameof(DateOnlyPkId))) entity.DateOnlyPkId = (DateOnlyPkId ?? entity.DateOnlyPkId);
            if (ShouldMapTo(nameof(Name))) entity.Name = Name;
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public Coalesce.Domain.DateOnlyPk MapToNew(IMappingContext context)
        {
            var entity = new Coalesce.Domain.DateOnlyPk();
            MapTo(entity, context);
            return entity;
        }

        public Coalesce.Domain.DateOnlyPk MapToModelOrNew(Coalesce.Domain.DateOnlyPk obj, IMappingContext context)
        {
            if (obj is null) return MapToNew(context);
            MapTo(obj, context);
            return obj;
        }
    }

    public partial class DateOnlyPkResponse : IGeneratedResponseDto<Coalesce.Domain.DateOnlyPk>
    {
        public DateOnlyPkResponse() { }

        public System.DateOnly? DateOnlyPkId { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public void MapFrom(Coalesce.Domain.DateOnlyPk obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.DateOnlyPkId = obj.DateOnlyPkId;
            this.Name = obj.Name;
        }
    }
}
