using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Web.Ko.Models
{
    public partial class StandaloneReadonlyDtoGen : GeneratedDto<Coalesce.Domain.StandaloneReadonly>
    {
        public StandaloneReadonlyDtoGen() { }

        private int? _Id;
        private string _Name;
        private string _Description;

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
        public string Description
        {
            get => _Description;
            set { _Description = value; Changed(nameof(Description)); }
        }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(Coalesce.Domain.StandaloneReadonly obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            // Fill the properties of the object.

            this.Id = obj.Id;
            this.Name = obj.Name;
            this.Description = obj.Description;
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(Coalesce.Domain.StandaloneReadonly entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            if (ShouldMapTo(nameof(Id))) entity.Id = (Id ?? entity.Id);
            if (ShouldMapTo(nameof(Name))) entity.Name = Name;
            if (ShouldMapTo(nameof(Description))) entity.Description = Description;
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public override Coalesce.Domain.StandaloneReadonly MapToNew(IMappingContext context)
        {
            var entity = new Coalesce.Domain.StandaloneReadonly();
            MapTo(entity, context);
            return entity;
        }
    }
}
