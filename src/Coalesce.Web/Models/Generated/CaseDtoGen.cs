
using System;
using System.Collections.Generic;
using System.Security.Claims;
using Intellitect.ComponentModel.Interfaces;
using System.Linq;
// Model Namespaces
using Coalesce.Domain;
using Coalesce.Domain.External;
using static Coalesce.Domain.Case;

namespace Coalesce.Web.Models
{
    public partial class CaseDto : IClassDto
    {
        public CaseDto() { }

        public CaseDto(ClaimsPrincipal user, Case entity)
        {
            User = user;
            List<string> roles;
                    CaseKey = entity.CaseKey;
                    Title = entity.Title;
                    Description = entity.Description;
                    OpenedAt = entity.OpenedAt;
                    AssignedToId = entity.AssignedToId;
                    AssignedTo = entity.AssignedTo;
                    ReportedById = entity.ReportedById;
                    ReportedBy = entity.ReportedBy;
                    Attachment = entity.Attachment;
                    Severity = entity.Severity;
                    Status = entity.Status;
                    CaseProducts = entity.CaseProducts;
                    DevTeamAssignedId = entity.DevTeamAssignedId;
                    DevTeamAssigned = entity.DevTeamAssigned;
        }

        public ClaimsPrincipal User { get; set; }
            
         public Int32? CaseKey { get; set; }
         public String Title { get; set; }
         public String Description { get; set; }
         public DateTimeOffset? OpenedAt { get; set; }
         public Nullable<Int32> AssignedToId { get; set; }
         public Person AssignedTo { get; set; }
         public Nullable<Int32> ReportedById { get; set; }
         public Person ReportedBy { get; set; }
         public Byte[] Attachment { get; set; }
         public String Severity { get; set; }
         public Statuses? Status { get; set; }
         public ICollection<CaseProduct> CaseProducts { get; set; }
         public Nullable<Int32> DevTeamAssignedId { get; set; }
         public DevTeam DevTeamAssigned { get; set; }

        public void Update(object obj)
        {   
            if (User == null) throw new InvalidOperationException("Updating an entity requires the User property to be populated.");

            Case entity = (Case)obj;

            List<string> roles;
                    entity.Title = Title;
                    entity.Description = Description;
                    entity.OpenedAt = (DateTimeOffset)OpenedAt;
                    entity.AssignedToId = AssignedToId;
                    entity.AssignedTo = AssignedTo;
                    entity.ReportedById = ReportedById;
                    entity.ReportedBy = ReportedBy;
                    entity.Attachment = Attachment;
                    entity.Severity = Severity;
                    entity.Status = (Statuses)Status;
                    entity.CaseProducts = CaseProducts;
                    entity.DevTeamAssignedId = DevTeamAssignedId;
                    entity.DevTeamAssigned = DevTeamAssigned;
        }
    }
}