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
    [JsonDerivedType(typeof(BaseClassParameter), typeDiscriminator: "BaseClass")]
    [JsonDerivedType(typeof(BaseClassDerivedParameter), typeDiscriminator: "BaseClassDerived")]
    public partial class BaseClassParameter : SparseDto, IGeneratedParameterDto<Coalesce.Domain.BaseClass>
    {
        public BaseClassParameter() { }

        private int? _Id;
        private string _BaseClassString;

        public int? Id
        {
            get => _Id;
            set { _Id = value; Changed(nameof(Id)); }
        }
        public string BaseClassString
        {
            get => _BaseClassString;
            set { _BaseClassString = value; Changed(nameof(BaseClassString)); }
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public void MapTo(Coalesce.Domain.BaseClass entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (ShouldMapTo(nameof(Id))) entity.Id = (Id ?? entity.Id);
            if (ShouldMapTo(nameof(BaseClassString))) entity.BaseClassString = BaseClassString;
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public Coalesce.Domain.BaseClass MapToNew(IMappingContext context)
        {
            var entity = new Coalesce.Domain.BaseClass();
            MapTo(entity, context);
            return entity;
        }

        public Coalesce.Domain.BaseClass MapToModelOrNew(Coalesce.Domain.BaseClass obj, IMappingContext context)
        {
            if (obj is null) return MapToNew(context);
            MapTo(obj, context);
            return obj;
        }
    }

    [JsonDerivedType(typeof(BaseClassResponse), typeDiscriminator: "BaseClass")]
    [JsonDerivedType(typeof(BaseClassDerivedResponse), typeDiscriminator: "BaseClassDerived")]
    public partial class BaseClassResponse : IGeneratedResponseDto<Coalesce.Domain.BaseClass>
    {
        public BaseClassResponse() { }

        public int? Id { get; set; }
        public string BaseClassString { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public void MapFrom(Coalesce.Domain.BaseClass obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.Id = obj.Id;
            this.BaseClassString = obj.BaseClassString;
        }
    }
}
