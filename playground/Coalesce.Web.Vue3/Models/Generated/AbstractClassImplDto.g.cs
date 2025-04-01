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
    public partial class AbstractClassImplParameter : AbstractClassParameter, IGeneratedParameterDto<Coalesce.Domain.AbstractClassImpl>
    {
        public AbstractClassImplParameter() { }

        private string _ImplString;

        public string ImplString
        {
            get => _ImplString;
            set { _ImplString = value; Changed(nameof(ImplString)); }
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public new void MapTo(Coalesce.Domain.AbstractClassImpl entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (ShouldMapTo(nameof(Id))) entity.Id = (Id ?? entity.Id);
            if (ShouldMapTo(nameof(ImplString))) entity.ImplString = ImplString;
            if (ShouldMapTo(nameof(AbstractClassString))) entity.AbstractClassString = AbstractClassString;
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public new Coalesce.Domain.AbstractClassImpl MapToNew(IMappingContext context)
        {
            var entity = new Coalesce.Domain.AbstractClassImpl();
            MapTo(entity, context);
            return entity;
        }

        public Coalesce.Domain.AbstractClassImpl MapToModelOrNew(Coalesce.Domain.AbstractClassImpl obj, IMappingContext context)
        {
            if (obj is null) return MapToNew(context);
            MapTo(obj, context);
            return obj;
        }
    }

    public partial class AbstractClassImplResponse : AbstractClassResponse, IGeneratedResponseDto<Coalesce.Domain.AbstractClassImpl>
    {
        public AbstractClassImplResponse() { }

        public string ImplString { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public void MapFrom(Coalesce.Domain.AbstractClassImpl obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.Id = obj.Id;
            this.ImplString = obj.ImplString;
            this.AbstractClassString = obj.AbstractClassString;
        }
    }
}
