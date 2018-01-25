
using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Security.Claims;

namespace Coalesce.Web.Models
{
    public partial class DevTeamDtoGen : GeneratedDto<Coalesce.Domain.External.DevTeam>
    {
        public DevTeamDtoGen() { }

        public int? DevTeamId { get; set; }
        public string Name { get; set; }

        public override void MapFrom(Coalesce.Domain.External.DevTeam obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;





            // Fill the properties of the object.
            this.DevTeamId = obj.DevTeamId;
            this.Name = obj.Name;
        }

        // Updates an object from the database to the state handed in by the DTO.
        public override void MapTo(Coalesce.Domain.External.DevTeam entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;





            entity.Name = Name;
        }

    }
}
