using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Starter.Vue.Web.Models
{
    public partial class RoleClaimParameter : GeneratedParameterDto<RoleClaim>
    {
        public RoleClaimParameter() { }

        private int? _Id;
        private string _RoleId;
        private string _ClaimType;
        private string _ClaimValue;

        public int? Id
        {
            get => _Id;
            set { _Id = value; Changed(nameof(Id)); }
        }
        public string RoleId
        {
            get => _RoleId;
            set { _RoleId = value; Changed(nameof(RoleId)); }
        }
        public string ClaimType
        {
            get => _ClaimType;
            set { _ClaimType = value; Changed(nameof(ClaimType)); }
        }
        public string ClaimValue
        {
            get => _ClaimValue;
            set { _ClaimValue = value; Changed(nameof(ClaimValue)); }
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(RoleClaim entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            if (ShouldMapTo(nameof(Id))) entity.Id = (Id ?? entity.Id);
            if (ShouldMapTo(nameof(RoleId))) entity.RoleId = RoleId;
            if (ShouldMapTo(nameof(ClaimType))) entity.ClaimType = ClaimType;
            if (ShouldMapTo(nameof(ClaimValue))) entity.ClaimValue = ClaimValue;
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public override RoleClaim MapToNew(IMappingContext context)
        {
            var entity = new RoleClaim();
            MapTo(entity, context);
            return entity;
        }
    }

    public partial class RoleClaimResponse : GeneratedResponseDto<RoleClaim>
    {
        public RoleClaimResponse() { }

        public int? Id { get; set; }
        public string RoleId { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
        public Coalesce.Starter.Vue.Web.Models.RoleResponse Role { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(RoleClaim obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.Id = obj.Id;
            this.RoleId = obj.RoleId;
            this.ClaimType = obj.ClaimType;
            this.ClaimValue = obj.ClaimValue;
            if (tree == null || tree[nameof(this.Role)] != null)
                this.Role = obj.Role.MapToDto<Coalesce.Starter.Vue.Data.Models.Role, RoleResponse>(context, tree?[nameof(this.Role)]);

        }
    }
}
