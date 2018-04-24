using System;
using IntelliTect.Coalesce.Models;

namespace IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext
{
    public class CaseDto : GeneratedDto<Case>
    {
        public int? CaseKey { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTimeOffset? OpenedAt { get; set; }
        public int? AssignedToId { get; set; }
        public int? ReportedById { get; set; }
        public byte[] Attachment { get; set; }
        public string Severity { get; set; }
        public int? DevTeamAssignedId { get; set; }
        public TimeSpan? Duration { get; set; }

        /// <summary>
        ///     Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(Case obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;

            // Fill the properties of the object.

            CaseKey = obj.CaseKey;
            Title = obj.Title;
            Description = obj.Description;
            OpenedAt = obj.OpenedAt;
            AssignedToId = obj.AssignedToId;
            ReportedById = obj.ReportedById;
            Attachment = obj.Attachment;
        }

        /// <summary>
        ///     Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(Case entity, IMappingContext context)
        {
            if (OnUpdate(entity, context)) return;

            entity.CaseKey = CaseKey ?? entity.CaseKey;
            entity.Title = Title;
            entity.Description = Description;
            entity.OpenedAt = OpenedAt ?? entity.OpenedAt;
            entity.AssignedToId = AssignedToId;
            entity.ReportedById = ReportedById;
            entity.Attachment = Attachment;
        }
    }
}