using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Starter.Vue.Web.Models
{
    public partial class AppUserParameter : GeneratedParameterDto<Coalesce.Starter.Vue.Data.Models.AppUser>
    {
        public AppUserParameter() { }

        private string _Id;
        private string _UserName;

        public string Id
        {
            get => _Id;
            set { _Id = value; Changed(nameof(Id)); }
        }
        public string UserName
        {
            get => _UserName;
            set { _UserName = value; Changed(nameof(UserName)); }
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(Coalesce.Starter.Vue.Data.Models.AppUser entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            if (ShouldMapTo(nameof(Id))) entity.Id = Id;
            if (ShouldMapTo(nameof(UserName))) entity.UserName = UserName;
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public override Coalesce.Starter.Vue.Data.Models.AppUser MapToNew(IMappingContext context)
        {
            var entity = new Coalesce.Starter.Vue.Data.Models.AppUser();
            MapTo(entity, context);
            return entity;
        }
    }

    public partial class AppUserResponse : GeneratedResponseDto<Coalesce.Starter.Vue.Data.Models.AppUser>
    {
        public AppUserResponse() { }

        public string Id { get; set; }
        public string UserName { get; set; }
        public int? AccessFailedCount { get; set; }
        public System.DateTimeOffset? LockoutEnd { get; set; }
        public bool? LockoutEnabled { get; set; }
        public System.Collections.Generic.ICollection<Coalesce.Starter.Vue.Web.Models.AppUserRoleResponse> UserRoles { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(Coalesce.Starter.Vue.Data.Models.AppUser obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.Id = obj.Id;
            this.UserName = obj.UserName;
            if ((context.IsInRoleCached("UserAdmin")))
            {
                this.AccessFailedCount = obj.AccessFailedCount;
                this.LockoutEnd = obj.LockoutEnd;
                this.LockoutEnabled = obj.LockoutEnabled;
                var propValUserRoles = obj.UserRoles;
                if (propValUserRoles != null && (tree == null || tree[nameof(this.UserRoles)] != null))
                {
                    this.UserRoles = propValUserRoles
                        .OrderBy(f => (f.User == null ? "" : f.User.Id)).ThenBy(f => (f.Role == null ? "" : f.Role.Name))
                        .Select(f => f.MapToDto<Coalesce.Starter.Vue.Data.Models.AppUserRole, AppUserRoleResponse>(context, tree?[nameof(this.UserRoles)])).ToList();
                }
                else if (propValUserRoles == null && tree?[nameof(this.UserRoles)] != null)
                {
                    this.UserRoles = new AppUserRoleResponse[0];
                }

            }

        }
    }
}
