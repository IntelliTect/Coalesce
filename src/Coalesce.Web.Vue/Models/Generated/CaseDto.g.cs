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

        private int? _CaseKey;
        private string _Title;
        private string _Description;
        private System.DateTimeOffset? _OpenedAt;
        private int? _AssignedToId;
        private Coalesce.Web.Vue.Models.PersonDtoGen _AssignedTo;
        private int? _ReportedById;
        private Coalesce.Web.Vue.Models.PersonDtoGen _ReportedBy;
        private byte[] _Image;
        private string _ImageName;
        private long? _ImageSize;
        private string _ImageHash;
        private byte[] _Attachment;
        private string _AttachmentName;
        private string _Severity;
        private Coalesce.Domain.Case.Statuses? _Status;
        private System.Collections.Generic.ICollection<Coalesce.Web.Vue.Models.CaseProductDtoGen> _CaseProducts;
        private int? _DevTeamAssignedId;
        private Coalesce.Web.Vue.Models.DevTeamDtoGen _DevTeamAssigned;
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
        public Coalesce.Web.Vue.Models.PersonDtoGen AssignedTo
        {
            get => _AssignedTo;
            set { _AssignedTo = value; Changed(nameof(AssignedTo)); }
        }
        public int? ReportedById
        {
            get => _ReportedById;
            set { _ReportedById = value; Changed(nameof(ReportedById)); }
        }
        public Coalesce.Web.Vue.Models.PersonDtoGen ReportedBy
        {
            get => _ReportedBy;
            set { _ReportedBy = value; Changed(nameof(ReportedBy)); }
        }
        public byte[] Image
        {
            get => _Image;
            set { _Image = value; Changed(nameof(Image)); }
        }
        public string ImageName
        {
            get => _ImageName;
            set { _ImageName = value; Changed(nameof(ImageName)); }
        }
        public long? ImageSize
        {
            get => _ImageSize;
            set { _ImageSize = value; Changed(nameof(ImageSize)); }
        }
        public string ImageHash
        {
            get => _ImageHash;
            set { _ImageHash = value; Changed(nameof(ImageHash)); }
        }
        public byte[] Attachment
        {
            get => _Attachment;
            set { _Attachment = value; Changed(nameof(Attachment)); }
        }
        public string AttachmentName
        {
            get => _AttachmentName;
            set { _AttachmentName = value; Changed(nameof(AttachmentName)); }
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
        public System.Collections.Generic.ICollection<Coalesce.Web.Vue.Models.CaseProductDtoGen> CaseProducts
        {
            get => _CaseProducts;
            set { _CaseProducts = value; Changed(nameof(CaseProducts)); }
        }
        public int? DevTeamAssignedId
        {
            get => _DevTeamAssignedId;
            set { _DevTeamAssignedId = value; Changed(nameof(DevTeamAssignedId)); }
        }
        public Coalesce.Web.Vue.Models.DevTeamDtoGen DevTeamAssigned
        {
            get => _DevTeamAssigned;
            set { _DevTeamAssigned = value; Changed(nameof(DevTeamAssigned)); }
        }
        public System.TimeSpan? Duration
        {
            get => _Duration;
            set { _Duration = value; Changed(nameof(Duration)); }
        }

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
            this.Image = obj.Image;
            this.ImageName = obj.ImageName;
            this.ImageSize = obj.ImageSize;
            this.ImageHash = obj.ImageHash;
            this.Attachment = obj.Attachment;
            this.AttachmentName = obj.AttachmentName;
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

            if (ShouldMapTo(nameof(CaseKey))) entity.CaseKey = (CaseKey ?? entity.CaseKey);
            if (ShouldMapTo(nameof(Title))) entity.Title = Title;
            if (ShouldMapTo(nameof(Description))) entity.Description = Description;
            if (ShouldMapTo(nameof(OpenedAt))) entity.OpenedAt = (OpenedAt ?? entity.OpenedAt);
            if (ShouldMapTo(nameof(AssignedToId))) entity.AssignedToId = AssignedToId;
            if (ShouldMapTo(nameof(ReportedById))) entity.ReportedById = ReportedById;
            if (ShouldMapTo(nameof(Image))) entity.Image = Image;
            if (ShouldMapTo(nameof(ImageSize))) entity.ImageSize = (ImageSize ?? entity.ImageSize);
            if (ShouldMapTo(nameof(ImageHash))) entity.ImageHash = ImageHash;
            if (ShouldMapTo(nameof(Attachment))) entity.Attachment = Attachment;
            if (ShouldMapTo(nameof(AttachmentName))) entity.AttachmentName = AttachmentName;
            if (ShouldMapTo(nameof(Severity))) entity.Severity = Severity;
            if (ShouldMapTo(nameof(Status))) entity.Status = (Status ?? entity.Status);
            if (ShouldMapTo(nameof(DevTeamAssignedId))) entity.DevTeamAssignedId = DevTeamAssignedId;
            if (ShouldMapTo(nameof(Duration))) entity.Duration = (Duration ?? entity.Duration);
        }
    }
}
