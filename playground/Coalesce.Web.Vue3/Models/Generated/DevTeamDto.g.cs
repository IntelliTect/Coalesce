using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Web.Vue3.Models
{
    public partial class DevTeamParameter : GeneratedParameterDto<Coalesce.Domain.External.DevTeam>
    {
        public DevTeamParameter() { }

        private int? _DevTeamId;
        private string _Name;

        public int? DevTeamId
        {
            get => _DevTeamId;
            set { _DevTeamId = value; Changed(nameof(DevTeamId)); }
        }
        public string Name
        {
            get => _Name;
            set { _Name = value; Changed(nameof(Name)); }
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(Coalesce.Domain.External.DevTeam entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            if (ShouldMapTo(nameof(DevTeamId))) entity.DevTeamId = (DevTeamId ?? entity.DevTeamId);
            if (ShouldMapTo(nameof(Name))) entity.Name = Name;
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public override Coalesce.Domain.External.DevTeam MapToNew(IMappingContext context)
        {
            var entity = new Coalesce.Domain.External.DevTeam();
            MapTo(entity, context);
            return entity;
        }
    }

    public partial class DevTeamResponse : GeneratedResponseDto<Coalesce.Domain.External.DevTeam>
    {
        public DevTeamResponse() { }

        public int? DevTeamId { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(Coalesce.Domain.External.DevTeam obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.DevTeamId = obj.DevTeamId;
            this.Name = obj.Name;
        }
    }
}
