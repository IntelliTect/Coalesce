using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Coalesce.Starter.Vue.Web.Models
{
    public partial class RoleParameter : SparseDto, IGeneratedParameterDto<Coalesce.Starter.Vue.Data.Models.Role>
    {
        public RoleParameter() { }

        private string _Id;
        private string _Name;
        private System.Collections.Generic.ICollection<Coalesce.Starter.Vue.Data.Permission> _Permissions;

        public string Id
        {
            get => _Id;
            set { _Id = value; Changed(nameof(Id)); }
        }
        public string Name
        {
            get => _Name;
            set { _Name = value; Changed(nameof(Name)); }
        }
        public System.Collections.Generic.ICollection<Coalesce.Starter.Vue.Data.Permission> Permissions
        {
            get => _Permissions;
            set { _Permissions = value; Changed(nameof(Permissions)); }
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public void MapTo(Coalesce.Starter.Vue.Data.Models.Role entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (ShouldMapTo(nameof(Id))) entity.Id = Id;
            if (ShouldMapTo(nameof(Name))) entity.Name = Name;
            if (ShouldMapTo(nameof(Permissions))) entity.Permissions = Permissions?.ToList();
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public Coalesce.Starter.Vue.Data.Models.Role MapToNew(IMappingContext context)
        {
            var entity = new Coalesce.Starter.Vue.Data.Models.Role();
            MapTo(entity, context);
            return entity;
        }

        public Coalesce.Starter.Vue.Data.Models.Role MapToModelOrNew(Coalesce.Starter.Vue.Data.Models.Role obj, IMappingContext context)
        {
            if (obj is null) return MapToNew(context);
            MapTo(obj, context);
            return obj;
        }
    }

    public partial class RoleResponse : IGeneratedResponseDto<Coalesce.Starter.Vue.Data.Models.Role>
    {
        public RoleResponse() { }

        public string Id { get; set; }
        public string Name { get; set; }
        public System.Collections.Generic.ICollection<Coalesce.Starter.Vue.Data.Permission> Permissions { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public void MapFrom(Coalesce.Starter.Vue.Data.Models.Role obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.Id = obj.Id;
            this.Name = obj.Name;
            this.Permissions = obj.Permissions;
        }
    }
}
