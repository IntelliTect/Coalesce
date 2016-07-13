
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Intellitect.ComponentModel.Interfaces;
using Intellitect.ComponentModel.Models;
using Intellitect.ComponentModel.Mapping;
using System.Linq;
using Newtonsoft.Json;
// Model Namespaces
using Coalesce.Domain;
using Coalesce.Domain.External;
using static Coalesce.Domain.External.DevTeam;

namespace Coalesce.Web.Models
{
    public partial class DevTeamDtoGen : GeneratedDto<DevTeam, DevTeamDtoGen>, IClassDto
    {
        public DevTeamDtoGen() { }

         public Int32? DevTeamId { get; set; }
         public String Name { get; set; }

        public void Update(object obj, ClaimsPrincipal user = null, string includes = null)
        {
            if (user == null) throw new InvalidOperationException("Updating an entity requires the User property to be populated.");

            includes = includes ?? "";

            DevTeam entity = (DevTeam)obj;

            if (OnUpdate(entity, user, includes)) return;

            // Applicable includes for DevTeam
            

            // Applicable excludes for DevTeam
            

            // Applicable roles for DevTeam
            if (user != null)
			{
			}
    
			entity.Name = Name;
        }

        public void SecurityTrim(ClaimsPrincipal user = null, string includes = null)
        {
            if (OnSecurityTrim(user, includes)) return;

            // Applicable includes for DevTeam
            

            // Applicable excludes for DevTeam
            

            // Applicable roles for DevTeam
            if (user != null)
			{
			}

        }
    }
}
