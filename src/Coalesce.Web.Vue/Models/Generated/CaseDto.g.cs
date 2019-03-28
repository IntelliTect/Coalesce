using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Web.Vue.Models
{
    public partial class CaseDtoGen : GeneratedDto<Coalesce.Domain.Case>
    {
        public CaseDtoGen() { }

        public int? CaseKey { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public System.DateTimeOffset? OpenedAt { get; set; }
        public int? AssignedToId { get; set; }
        public Coalesce.Web.Vue.Models.PersonDtoGen AssignedTo { get; set; }
        public int? ReportedById { get; set; }
        public Coalesce.Web.Vue.Models.PersonDtoGen ReportedBy { get; set; }
        public byte[] Attachment { get; set; }
        public string Severity { get; set; }
        public Coalesce.Domain.Case.Statuses? Status { get; set; }
        public System.Collections.Generic.ICollection<Coalesce.Web.Vue.Models.CaseProductDtoGen> CaseProducts { get; set; }
        public int? DevTeamAssignedId { get; set; }
        public Coalesce.Web.Vue.Models.DevTeamDtoGen DevTeamAssigned { get; set; }
        public System.TimeSpan? Duration { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(Coalesce.Domain.Case obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

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
            var propValCaseProducts = obj.CaseProducts;
            if (propValCaseProducts != null && (tree == null || tree[nameof(this.CaseProducts)] != null))
            {
                this.CaseProducts = propValCaseProducts
                    .OrderBy(f => f.CaseProductId)
                    .Select(f => f.MapToDto<Coalesce.Domain.CaseProduct, CaseProductDtoGen>(context, tree?[nameof(this.CaseProducts)])).ToList();
            }
            else if (propValCaseProducts == null && tree?[nameof(this.CaseProducts)] != null)
            {
                this.CaseProducts = new CaseProductDtoGen[0];
            }


            this.DevTeamAssigned = obj.DevTeamAssigned.MapToDto<Coalesce.Domain.External.DevTeam, DevTeamDtoGen>(context, tree?[nameof(this.DevTeamAssigned)]);

            if (!(includes == "PersonListGen"))
            {
                if (tree == null || tree[nameof(this.AssignedTo)] != null)
                    this.AssignedTo = obj.AssignedTo.MapToDto<Coalesce.Domain.Person, PersonDtoGen>(context, tree?[nameof(this.AssignedTo)]);

                if (tree == null || tree[nameof(this.ReportedBy)] != null)
                    this.ReportedBy = obj.ReportedBy.MapToDto<Coalesce.Domain.Person, PersonDtoGen>(context, tree?[nameof(this.ReportedBy)]);

            }
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(Coalesce.Domain.Case entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            entity.CaseKey = (CaseKey ?? entity.CaseKey);
            entity.Title = Title;
            entity.Description = Description;
            entity.OpenedAt = (OpenedAt ?? entity.OpenedAt);
            entity.AssignedToId = AssignedToId;
            entity.ReportedById = ReportedById;
            entity.Attachment = Attachment;
            entity.Severity = Severity;
            entity.Status = (Status ?? entity.Status);
            entity.DevTeamAssignedId = DevTeamAssignedId;
            entity.Duration = (Duration ?? entity.Duration);
        }
    }
}
