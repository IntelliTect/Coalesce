
using System;
using System.Collections.Generic;
using Intellitect.ComponentModel.Mapping;
// Model Namespaces
using Coalesce.Domain;
using Coalesce.Domain.External;
using static Coalesce.Domain.External.DevTeam;

namespace Coalesce.Web.TestArea.Models
{
    public partial class DevTeamDto : IClassDto
    {
        public DevTeamDto() { }

        public DevTeamDto(DevTeam entity)
        {
                DevTeamId = entity.DevTeamId;
                Name = entity.Name;
        }
        
         public Int32? DevTeamId { get; set; }
         public String Name { get; set; }

        public void Update(object obj)
        {
            DevTeam entity = (DevTeam)obj;

                entity.Name = Name;
        }
    }
}
