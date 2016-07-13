
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
using static Coalesce.Domain.Case;

namespace Coalesce.Web.TestArea.Models
{
    public partial class CaseDtoGen : GeneratedDto<Case, CaseDtoGen>, IClassDto
    {
        public CaseDtoGen() { }

         public Int32? CaseKey { get; set; }
         public String Title { get; set; }
         public String Description { get; set; }
         public DateTimeOffset? OpenedAt { get; set; }
         public Nullable<Int32> AssignedToId { get; set; }
         public PersonDtoGen AssignedTo { get; set; }
         public Nullable<Int32> ReportedById { get; set; }
         public PersonDtoGen ReportedBy { get; set; }
         public Byte[] Attachment { get; set; }
         public String Severity { get; set; }
         public Statuses? Status { get; set; }
         public ICollection<CaseProductDtoGen> CaseProducts { get; set; }
         public Nullable<Int32> DevTeamAssignedId { get; set; }
         public DevTeamDtoGen DevTeamAssigned { get; set; }

        public void Update(object obj, ClaimsPrincipal user = null, string includes = null)
        {
            if (user == null) throw new InvalidOperationException("Updating an entity requires the User property to be populated.");

            includes = includes ?? "";

            Case entity = (Case)obj;

            if (OnUpdate(entity, user, includes)) return;

            // Applicable includes for Case
            

            // Applicable excludes for Case
            

            // Applicable roles for Case
            if (user != null)
			{
			}
    
			entity.Title = Title;
			entity.Description = Description;
			entity.OpenedAt = (DateTimeOffset)OpenedAt;
			entity.AssignedToId = AssignedToId;
			entity.ReportedById = ReportedById;
			entity.Attachment = Attachment;
			entity.Severity = Severity;
			entity.Status = (Statuses)Status;
			entity.DevTeamAssignedId = DevTeamAssignedId;
        }

        public void SecurityTrim(ClaimsPrincipal user = null, string includes = null)
        {
            if (OnSecurityTrim(user, includes)) return;

            // Applicable includes for Case
            

            // Applicable excludes for Case
            

            // Applicable roles for Case
            if (user != null)
			{
			}

        }
    }
}
