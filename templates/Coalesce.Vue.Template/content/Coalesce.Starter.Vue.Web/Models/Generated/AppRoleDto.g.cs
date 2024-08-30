using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Starter.Vue.Web.Models
{
    public partial class AppRoleParameter : GeneratedParameterDto<Coalesce.Starter.Vue.Data.Models.AppRole>
    {
        public AppRoleParameter() { }

        private string _Id;
        private string _Name;

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

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(Coalesce.Starter.Vue.Data.Models.AppRole entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            if (ShouldMapTo(nameof(Id))) entity.Id = Id;
            if (ShouldMapTo(nameof(Name))) entity.Name = Name;
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public override Coalesce.Starter.Vue.Data.Models.AppRole MapToNew(IMappingContext context)
        {
            var entity = new Coalesce.Starter.Vue.Data.Models.AppRole();
            MapTo(entity, context);
            return entity;
        }
    }

    public partial class AppRoleResponse : GeneratedResponseDto<Coalesce.Starter.Vue.Data.Models.AppRole>
    {
        public AppRoleResponse() { }

        public string Id { get; set; }
        public string Name { get; set; }
        public Coalesce.Starter.Vue.Data.Permission[] Permissions { get; set; }
        public System.Collections.Generic.ICollection<AppRoleClaimResponse> RoleClaims { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(Coalesce.Starter.Vue.Data.Models.AppRole obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.Id = obj.Id;
            this.Name = obj.Name;
            this.Permissions = obj.Permissions;
            var propValRoleClaims = obj.RoleClaims;
            if (propValRoleClaims != null && (tree == null || tree[nameof(this.RoleClaims)] != null))
            {
                this.RoleClaims = propValRoleClaims
                    .OrderBy(f => f.Id)
                    .Select(f => f.MapToDto<AppRoleClaim, AppRoleClaimResponse>(context, tree?[nameof(this.RoleClaims)])).ToList();
            }
            else if (propValRoleClaims == null && tree?[nameof(this.RoleClaims)] != null)
            {
                this.RoleClaims = new AppRoleClaimResponse[0];
            }

        }
    }
}
