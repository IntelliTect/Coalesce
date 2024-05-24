using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Coalesce.Web.Vue3.Models
{
    public partial class CaseParameter : GeneratedParameterDto<Coalesce.Domain.Case>
    {
        public CaseParameter() { }

        private int? _CaseKey;
        private string _Title;
        private string _Description;
        private System.DateTimeOffset? _OpenedAt;
        private int? _AssignedToId;
        private int? _ReportedById;
        private string _AttachmentType;
        private string _Severity;
        private Coalesce.Domain.Case.Statuses? _Status;
        private int? _DevTeamAssignedId;
        private System.TimeSpan? _Duration;

        public int? CaseKey
        {
            get => _CaseKey;
            set { _CaseKey = value; Changed(nameof(CaseKey)); }
        }
        public string Title
        {
            get => _Title;
            set { _Title = value; Changed(nameof(Title)); }
        }
        public string Description
        {
            get => _Description;
            set { _Description = value; Changed(nameof(Description)); }
        }
        public System.DateTimeOffset? OpenedAt
        {
            get => _OpenedAt;
            set { _OpenedAt = value; Changed(nameof(OpenedAt)); }
        }
        public int? AssignedToId
        {
            get => _AssignedToId;
            set { _AssignedToId = value; Changed(nameof(AssignedToId)); }
        }
        public int? ReportedById
        {
            get => _ReportedById;
            set { _ReportedById = value; Changed(nameof(ReportedById)); }
        }
        public string AttachmentType
        {
            get => _AttachmentType;
            set { _AttachmentType = value; Changed(nameof(AttachmentType)); }
        }
        public string Severity
        {
            get => _Severity;
            set { _Severity = value; Changed(nameof(Severity)); }
        }
        public Coalesce.Domain.Case.Statuses? Status
        {
            get => _Status;
            set { _Status = value; Changed(nameof(Status)); }
        }
        public int? DevTeamAssignedId
        {
            get => _DevTeamAssignedId;
            set { _DevTeamAssignedId = value; Changed(nameof(DevTeamAssignedId)); }
        }
        public System.TimeSpan? Duration
        {
            get => _Duration;
            set { _Duration = value; Changed(nameof(Duration)); }
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(Coalesce.Domain.Case entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            if (ShouldMapTo(nameof(CaseKey))) entity.CaseKey = (CaseKey ?? entity.CaseKey);
            if (ShouldMapTo(nameof(Title))) entity.Title = Title;
            if (ShouldMapTo(nameof(Description))) entity.Description = Description;
            if (ShouldMapTo(nameof(OpenedAt))) entity.OpenedAt = (OpenedAt ?? entity.OpenedAt);
            if (ShouldMapTo(nameof(AssignedToId))) entity.AssignedToId = AssignedToId;
            if (ShouldMapTo(nameof(ReportedById))) entity.ReportedById = ReportedById;
            if (ShouldMapTo(nameof(AttachmentType)) && context.GetPropertyRestriction<Coalesce.Domain.Case.TestRestriction>().UserCanWrite(context, nameof(AttachmentType), entity, AttachmentType)) entity.AttachmentType = AttachmentType;
            if (ShouldMapTo(nameof(Severity))) entity.Severity = Severity;
            if (ShouldMapTo(nameof(Status))) entity.Status = (Status ?? entity.Status);
            if (ShouldMapTo(nameof(DevTeamAssignedId))) entity.DevTeamAssignedId = DevTeamAssignedId;
            if (ShouldMapTo(nameof(Duration))) entity.Duration = (Duration ?? entity.Duration);
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public override Coalesce.Domain.Case MapToNew(IMappingContext context)
        {
            var entity = new Coalesce.Domain.Case();
            MapTo(entity, context);
            return entity;
        }
    }

    public partial class CaseResponse : GeneratedResponseDto<Coalesce.Domain.Case>
    {
        public CaseResponse() { }

        public int? CaseKey { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public System.DateTimeOffset? OpenedAt { get; set; }
        public int? AssignedToId { get; set; }
        public int? ReportedById { get; set; }
        public long? AttachmentSize { get; set; }
        public string AttachmentName { get; set; }
        public string AttachmentType { get; set; }
        public byte[] AttachmentHash { get; set; }
        public string Severity { get; set; }
        public Coalesce.Domain.Case.Statuses? Status { get; set; }
        public int? DevTeamAssignedId { get; set; }
        public System.TimeSpan? Duration { get; set; }
        public Coalesce.Web.Vue3.Models.PersonResponse AssignedTo { get; set; }
        public Coalesce.Web.Vue3.Models.PersonResponse ReportedBy { get; set; }
        public System.Collections.Generic.ICollection<Coalesce.Web.Vue3.Models.CaseProductResponse> CaseProducts { get; set; }
        public Coalesce.Web.Vue3.Models.DevTeamResponse DevTeamAssigned { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(Coalesce.Domain.Case obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.CaseKey = obj.CaseKey;
            this.Title = obj.Title;
            this.Description = obj.Description;
            this.OpenedAt = obj.OpenedAt;
            this.AssignedToId = obj.AssignedToId;
            this.ReportedById = obj.ReportedById;
            this.AttachmentSize = obj.AttachmentSize;
            this.AttachmentHash = obj.AttachmentHash;
            this.Severity = obj.Severity;
            this.Status = obj.Status;
            this.DevTeamAssignedId = obj.DevTeamAssignedId;
            this.Duration = obj.Duration;
            var propValCaseProducts = obj.CaseProducts;
            if (propValCaseProducts != null && (tree == null || tree[nameof(this.CaseProducts)] != null))
            {
                this.CaseProducts = propValCaseProducts
                    .OrderBy(f => f.CaseProductId)
                    .Select(f => f.MapToDto<Coalesce.Domain.CaseProduct, CaseProductResponse>(context, tree?[nameof(this.CaseProducts)])).ToList();
            }
            else if (propValCaseProducts == null && tree?[nameof(this.CaseProducts)] != null)
            {
                this.CaseProducts = new CaseProductResponse[0];
            }


            this.DevTeamAssigned = obj.DevTeamAssigned.MapToDto<Coalesce.Domain.External.DevTeam, DevTeamResponse>(context, tree?[nameof(this.DevTeamAssigned)]);

            if (context.GetPropertyRestriction<Coalesce.Domain.Case.TestRestriction>().UserCanRead(context, nameof(AttachmentName), obj)) this.AttachmentName = obj.AttachmentName;
            if (context.GetPropertyRestriction<Coalesce.Domain.Case.TestRestriction>().UserCanRead(context, nameof(AttachmentType), obj)) this.AttachmentType = obj.AttachmentType;
            if (!(includes == "PersonListGen"))
            {
                if (tree == null || tree[nameof(this.AssignedTo)] != null)
                    this.AssignedTo = obj.AssignedTo.MapToDto<Coalesce.Domain.Person, PersonResponse>(context, tree?[nameof(this.AssignedTo)]);

                if (tree == null || tree[nameof(this.ReportedBy)] != null)
                    this.ReportedBy = obj.ReportedBy.MapToDto<Coalesce.Domain.Person, PersonResponse>(context, tree?[nameof(this.ReportedBy)]);

            }

        }
    }
}
