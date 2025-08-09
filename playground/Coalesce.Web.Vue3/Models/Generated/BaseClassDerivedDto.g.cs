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
    public partial class BaseClassDerivedParameter : BaseClassParameter, IGeneratedParameterDto<Coalesce.Domain.BaseClassDerived>
    {
        public BaseClassDerivedParameter() { }

        private string _DerivedClassString;

        public string DerivedClassString
        {
            get => _DerivedClassString;
            set { _DerivedClassString = value; Changed(nameof(DerivedClassString)); }
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public void MapTo(Coalesce.Domain.BaseClassDerived entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (ShouldMapTo(nameof(Id))) entity.Id = (Id ?? entity.Id);
            if (ShouldMapTo(nameof(DerivedClassString))) entity.DerivedClassString = DerivedClassString;
            if (ShouldMapTo(nameof(BaseClassString))) entity.BaseClassString = BaseClassString;
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public new Coalesce.Domain.BaseClassDerived MapToNew(IMappingContext context)
        {
            var entity = new Coalesce.Domain.BaseClassDerived();
            MapTo(entity, context);
            return entity;
        }

        public Coalesce.Domain.BaseClassDerived MapToModelOrNew(Coalesce.Domain.BaseClassDerived obj, IMappingContext context)
        {
            if (obj is null) return MapToNew(context);
            MapTo(obj, context);
            return obj;
        }
    }

    public partial class BaseClassDerivedResponse : BaseClassResponse, IGeneratedResponseDto<Coalesce.Domain.BaseClassDerived>
    {
        public BaseClassDerivedResponse() { }

        public string DerivedClassString { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public void MapFrom(Coalesce.Domain.BaseClassDerived obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.Id = obj.Id;
            this.DerivedClassString = obj.DerivedClassString;
            this.BaseClassString = obj.BaseClassString;
        }
    }
}
