using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Web.Vue.Models
{
    public partial class DevTeamDtoGen : GeneratedDto<Coalesce.Domain.External.DevTeam>
    {
        public DevTeamDtoGen() { }

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
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(Coalesce.Domain.External.DevTeam obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            // Fill the properties of the object.

            this.DevTeamId = obj.DevTeamId;
            this.Name = obj.Name;
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
    }
}
