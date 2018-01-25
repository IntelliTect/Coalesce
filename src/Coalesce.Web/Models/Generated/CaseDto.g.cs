
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
    public partial class CaseDtoGen : GeneratedDto<Coalesce.Domain.Case>
    {
        public CaseDtoGen() { }

        public int? CaseKey { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public System.DateTimeOffset? OpenedAt { get; set; }
        public int? AssignedToId { get; set; }
        public Coalesce.Web.Models.PersonDtoGen AssignedTo { get; set; }
        public int? ReportedById { get; set; }
        public Coalesce.Web.Models.PersonDtoGen ReportedBy { get; set; }
        public byte[] Attachment { get; set; }
        public string Severity { get; set; }
        public Coalesce.Domain.Case.Statuses? Status { get; set; }
        public System.Collections.Generic.ICollection<Coalesce.Web.Models.CaseProductDtoGen> CaseProducts { get; set; }
        public int? DevTeamAssignedId { get; set; }
        public Coalesce.Web.Models.DevTeamDtoGen DevTeamAssigned { get; set; }
        public System.TimeSpan? Duration { get; set; }

        public override void MapFrom(Coalesce.Domain.Case obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;


            bool excludePersonListGen = includes == "PersonListGen";



            // Fill the properties of the object.
            this.CaseKey = obj.CaseKey;
            this.Title = obj.Title;
            this.Description = obj.Description;
            this.OpenedAt = obj.OpenedAt;
            this.AssignedToId = obj.AssignedToId;
            this.ReportedById = obj.ReportedById;
            this.Attachment = obj.Attachment;
            this.Severity = obj.Severity;
            this.Status = obj.Status;
            this.DevTeamAssignedId = obj.DevTeamAssignedId;
            this.Duration = obj.Duration;
            if (!(excludePersonListGen))
            {
                if (tree == null || tree[nameof(this.AssignedTo)] != null)
                    this.AssignedTo = obj.AssignedTo.MapToDto<Coalesce.Domain.Person, PersonDtoGen>(context, tree?[nameof(this.AssignedTo)]);

            }
            if (!(excludePersonListGen))
            {
                if (tree == null || tree[nameof(this.ReportedBy)] != null)
                    this.ReportedBy = obj.ReportedBy.MapToDto<Coalesce.Domain.Person, PersonDtoGen>(context, tree?[nameof(this.ReportedBy)]);

            }
            var propValCaseProducts = obj.CaseProducts;
            if (propValCaseProducts != null && (tree == null || tree[nameof(this.CaseProducts)] != null))
            {
                this.CaseProducts = propValCaseProducts.AsQueryable().OrderBy("CaseProductId ASC").ToList().Select(f => f.MapToDto<Coalesce.Domain.CaseProduct, CaseProductDtoGen>(context, tree?[nameof(this.CaseProducts)])).ToList();
            }
            else if (propValCaseProducts == null && tree?[nameof(this.CaseProducts)] != null)
            {
                this.CaseProducts = new CaseProductDtoGen[0];
            }


            this.DevTeamAssigned = obj.DevTeamAssigned.MapToDto<Coalesce.Domain.External.DevTeam, DevTeamDtoGen>(context, tree?[nameof(this.DevTeamAssigned)]);

        }

        // Updates an object from the database to the state handed in by the DTO.
        public override void MapTo(Coalesce.Domain.Case entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;


            bool excludePersonListGen = includes == "PersonListGen";



            entity.Title = Title;
            entity.Description = Description;
            entity.OpenedAt = (OpenedAt ?? DateTime.Today);
            entity.AssignedToId = AssignedToId;
            entity.ReportedById = ReportedById;
            entity.Attachment = Attachment;
            entity.Severity = Severity;
            entity.Status = (Status ?? 0);
            entity.DevTeamAssignedId = DevTeamAssignedId;
            entity.Duration = (Duration ?? default(System.TimeSpan));
        }

    }
}
