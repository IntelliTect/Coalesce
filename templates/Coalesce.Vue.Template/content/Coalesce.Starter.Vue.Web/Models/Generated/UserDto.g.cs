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
    public partial class UserParameter : SparseDto, IGeneratedParameterDto<Coalesce.Starter.Vue.Data.Models.User>
    {
        public UserParameter() { }

        private string _Id;
        private string _FullName;
        private string _UserName;
        private bool? _IsGlobalAdmin;

        public string Id
        {
            get => _Id;
            set { _Id = value; Changed(nameof(Id)); }
        }
        public string FullName
        {
            get => _FullName;
            set { _FullName = value; Changed(nameof(FullName)); }
        }
        public string UserName
        {
            get => _UserName;
            set { _UserName = value; Changed(nameof(UserName)); }
        }
        public bool? IsGlobalAdmin
        {
            get => _IsGlobalAdmin;
            set { _IsGlobalAdmin = value; Changed(nameof(IsGlobalAdmin)); }
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public void MapTo(Coalesce.Starter.Vue.Data.Models.User entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (ShouldMapTo(nameof(Id))) entity.Id = Id;
            if (ShouldMapTo(nameof(FullName))) entity.FullName = FullName;
            if (ShouldMapTo(nameof(UserName))) entity.UserName = UserName;
            if (ShouldMapTo(nameof(IsGlobalAdmin)) && (context.IsInRoleCached("GlobalAdmin"))) entity.IsGlobalAdmin = (IsGlobalAdmin ?? entity.IsGlobalAdmin);
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public Coalesce.Starter.Vue.Data.Models.User MapToNew(IMappingContext context)
        {
            var entity = new Coalesce.Starter.Vue.Data.Models.User();
            MapTo(entity, context);
            return entity;
        }

        public Coalesce.Starter.Vue.Data.Models.User MapToModelOrNew(Coalesce.Starter.Vue.Data.Models.User obj, IMappingContext context)
        {
            if (obj is null) return MapToNew(context);
            MapTo(obj, context);
            return obj;
        }
    }

    public partial class UserResponse : IGeneratedResponseDto<Coalesce.Starter.Vue.Data.Models.User>
    {
        public UserResponse() { }

        public string Id { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public bool? EmailConfirmed { get; set; }
        public byte[] PhotoHash { get; set; }
        public System.Collections.Generic.ICollection<string> RoleNames { get; set; }
        public bool? IsGlobalAdmin { get; set; }
        public System.Collections.Generic.ICollection<Coalesce.Starter.Vue.Web.Models.UserRoleResponse> UserRoles { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public void MapFrom(Coalesce.Starter.Vue.Data.Models.User obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.Id = obj.Id;
            this.FullName = obj.FullName;
            this.UserName = obj.UserName;
            this.Email = obj.Email;
            this.EmailConfirmed = obj.EmailConfirmed;
            this.PhotoHash = obj.PhotoHash;
            if ((context.IsInRoleCached("UserAdmin")))
            {
                this.RoleNames = obj.RoleNames?.ToList();
                var propValUserRoles = obj.UserRoles;
                if (propValUserRoles != null && (tree == null || tree[nameof(this.UserRoles)] != null))
                {
                    this.UserRoles = propValUserRoles
                        .OrderBy(f => f.TenantId).ThenBy(f => (f.User == null ? "" : f.User.Id)).ThenBy(f => (f.Role == null ? "" : f.Role.TenantId))
                        .Select(f => f.MapToDto<Coalesce.Starter.Vue.Data.Models.UserRole, UserRoleResponse>(context, tree?[nameof(this.UserRoles)])).ToList();
                }
                else if (propValUserRoles == null && tree?[nameof(this.UserRoles)] != null)
                {
                    this.UserRoles = new UserRoleResponse[0];
                }

            }

            if ((context.IsInRoleCached("GlobalAdmin"))) this.IsGlobalAdmin = obj.IsGlobalAdmin;
        }
    }
}
