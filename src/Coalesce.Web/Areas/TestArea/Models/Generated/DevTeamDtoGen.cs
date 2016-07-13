
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Intellitect.ComponentModel.Interfaces;
using System.Linq;
// Model Namespaces
using Coalesce.Domain;
using Coalesce.Domain.External;
using static Coalesce.Domain.External.DevTeam;

namespace Coalesce.Web.TestArea.Models
{
    public partial class DevTeamDto : IClassDto
    {
        public DevTeamDto() { }

        public DevTeamDto(DevTeam entity, ClaimsPrincipal user = null, string includes = null)
        {
            User = user;
            Includes = includes ?? "";

            // Applicable includes for DevTeam
            

            // Applicable excludes for DevTeam
            

            // Applicable roles for DevTeam
            if (User != null)
			{
			}

			DevTeamId = entity.DevTeamId;
			Name = entity.Name;
        }

        public ClaimsPrincipal User { get; set; }
        public string Includes { get; set; }
            
         public Int32? DevTeamId { get; set; }
         public String Name { get; set; }

        public void Update(object obj)
        {   
            if (User == null) throw new InvalidOperationException("Updating an entity requires the User property to be populated.");

            // Applicable includes for DevTeam
            

            // Applicable excludes for DevTeam
            

            // Applicable roles for DevTeam
            if (User != null)
			{
			}


            DevTeam entity = (DevTeam)obj;

			entity.Name = Name;
        }
    }
}