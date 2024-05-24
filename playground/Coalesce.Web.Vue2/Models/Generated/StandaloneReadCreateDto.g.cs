using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Web.Vue2.Models
{
    public partial class StandaloneReadCreateParameter : GeneratedParameterDto<Coalesce.Domain.StandaloneReadCreate>
    {
        public StandaloneReadCreateParameter() { }

        private int? _Id;
        private string _Name;
        private System.DateTimeOffset? _Date;

        public int? Id
        {
            get => _Id;
            set { _Id = value; Changed(nameof(Id)); }
        }
        public string Name
        {
            get => _Name;
            set { _Name = value; Changed(nameof(Name)); }
        }
        public System.DateTimeOffset? Date
        {
            get => _Date;
            set { _Date = value; Changed(nameof(Date)); }
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(Coalesce.Domain.StandaloneReadCreate entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            if (ShouldMapTo(nameof(Id))) entity.Id = (Id ?? entity.Id);
            if (ShouldMapTo(nameof(Name))) entity.Name = Name;
            if (ShouldMapTo(nameof(Date))) entity.Date = (Date ?? entity.Date);
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public override Coalesce.Domain.StandaloneReadCreate MapToNew(IMappingContext context)
        {
            var entity = new Coalesce.Domain.StandaloneReadCreate();
            MapTo(entity, context);
            return entity;
        }
    }

    public partial class StandaloneReadCreateResponse : GeneratedResponseDto<Coalesce.Domain.StandaloneReadCreate>
    {
        public StandaloneReadCreateResponse() { }

        public int? Id { get; set; }
        public string Name { get; set; }
        public System.DateTimeOffset? Date { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(Coalesce.Domain.StandaloneReadCreate obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.Id = obj.Id;
            this.Name = obj.Name;
            this.Date = obj.Date;
        }
    }
}
