
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Intellitect.ComponentModel.Interfaces;
using System.Linq;
// Model Namespaces
using Coalesce.Domain;
using Coalesce.Domain.External;
using static Coalesce.Domain.External.DevTeam;

namespace Coalesce.Web.Models
{
    public partial class DevTeamDto : IClassDto
    {
        public DevTeamDto() { }

        public DevTeamDto(ClaimsPrincipal user, DevTeam entity)
        {
            User = user;
            List<string> roles;
                    DevTeamId = entity.DevTeamId;
                    Name = entity.Name;
        }

        public ClaimsPrincipal User { get; set; }
            
         public Int32? DevTeamId { get; set; }
         public String Name { get; set; }

        public void Update(object obj)
        {   
            if (User == null) throw new InvalidOperationException("Updating an entity requires the User property to be populated.");

            DevTeam entity = (DevTeam)obj;

            List<string> roles;
                    entity.Name = Name;
        }
    }
}