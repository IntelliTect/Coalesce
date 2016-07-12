
using System;
using System.Collections.Generic;
using Intellitect.ComponentModel.Mapping;
// Model Namespaces
using Coalesce.Domain;
using Coalesce.Domain.External;
using static Coalesce.Domain.Case;

namespace Coalesce.Web.TestArea.Models
{
    public partial class CaseDto : IClassDto
    {
        public CaseDto() { }

        public CaseDto(Case entity)
        {
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
            Case entity = (Case)obj;

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
