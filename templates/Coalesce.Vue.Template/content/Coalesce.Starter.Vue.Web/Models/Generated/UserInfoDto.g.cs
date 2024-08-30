using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Starter.Vue.Web.Models
{
    public partial class UserInfoParameter : GeneratedParameterDto<Coalesce.Starter.Vue.Data.Services.UserInfo>
    {
        public UserInfoParameter() { }

        private string _Id;
        private string _UserName;
        private System.Collections.Generic.ICollection<string> _Roles;
        private System.Collections.Generic.ICollection<string> _Permissions;

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
        public System.Collections.Generic.ICollection<string> Roles
        {
            get => _Roles;
            set { _Roles = value; Changed(nameof(Roles)); }
        }
        public System.Collections.Generic.ICollection<string> Permissions
        {
            get => _Permissions;
            set { _Permissions = value; Changed(nameof(Permissions)); }
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(Coalesce.Starter.Vue.Data.Services.UserInfo entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            if (ShouldMapTo(nameof(Id))) entity.Id = Id;
            if (ShouldMapTo(nameof(UserName))) entity.UserName = UserName;
            if (ShouldMapTo(nameof(Roles))) entity.Roles = Roles;
            if (ShouldMapTo(nameof(Permissions))) entity.Permissions = Permissions;
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public override Coalesce.Starter.Vue.Data.Services.UserInfo MapToNew(IMappingContext context)
        {
            var includes = context.Includes;

            var entity = new Coalesce.Starter.Vue.Data.Services.UserInfo()
            {
                Id = Id,
                UserName = UserName,
                Roles = Roles,
                Permissions = Permissions,
            };

            if (OnUpdate(entity, context)) return entity;

            return entity;
        }
    }

    public partial class UserInfoResponse : GeneratedResponseDto<Coalesce.Starter.Vue.Data.Services.UserInfo>
    {
        public UserInfoResponse() { }

        public string Id { get; set; }
        public string UserName { get; set; }
        public System.Collections.Generic.ICollection<string> Roles { get; set; }
        public System.Collections.Generic.ICollection<string> Permissions { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(Coalesce.Starter.Vue.Data.Services.UserInfo obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.Id = obj.Id;
            this.UserName = obj.UserName;
            this.Roles = obj.Roles;
            this.Permissions = obj.Permissions;
        }
    }
}
