
using IntelliTect.Coalesce.Helpers.IncludeTree;
using IntelliTect.Coalesce.Interfaces;
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
    public partial class CaseDtoGen : GeneratedDto<Coalesce.Domain.Case, CaseDtoGen>
        , IClassDto<Coalesce.Domain.Case, CaseDtoGen>
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

        // Create a new version of this object or use it from the lookup.
        public static CaseDtoGen Create(Coalesce.Domain.Case obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return null;
            var includes = context.Includes;

            // Applicable includes for Case


            // Applicable excludes for Case
            bool excludePersonListGen = includes == "PersonListGen";

            // Applicable roles for Case



            // See if the object is already created, but only if we aren't restricting by an includes tree.
            // If we do have an IncludeTree, we know the exact structure of our return data, so we don't need to worry about circular refs.
            if (tree == null && context.TryGetMapping(obj, out CaseDtoGen existing)) return existing;

            var newObject = new CaseDtoGen();
            if (tree == null) context.AddMapping(obj, newObject);
            // Fill the properties of the object.
            newObject.CaseKey = obj.CaseKey;
            newObject.Title = obj.Title;
            newObject.Description = obj.Description;
            newObject.OpenedAt = obj.OpenedAt;
            newObject.AssignedToId = obj.AssignedToId;
            newObject.ReportedById = obj.ReportedById;
            newObject.Attachment = obj.Attachment;
            newObject.Severity = obj.Severity;
            newObject.Status = obj.Status;
            newObject.DevTeamAssignedId = obj.DevTeamAssignedId;
            if (!(excludePersonListGen))
            {
                if (tree == null || tree[nameof(newObject.AssignedTo)] != null)
                    newObject.AssignedTo = PersonDtoGen.Create(obj.AssignedTo, context, tree?[nameof(newObject.AssignedTo)]);

            }
            if (!(excludePersonListGen))
            {
                if (tree == null || tree[nameof(newObject.ReportedBy)] != null)
                    newObject.ReportedBy = PersonDtoGen.Create(obj.ReportedBy, context, tree?[nameof(newObject.ReportedBy)]);

            }
            var propValCaseProducts = obj.CaseProducts;
            if (propValCaseProducts != null && (tree == null || tree[nameof(newObject.CaseProducts)] != null))
            {
                newObject.CaseProducts = propValCaseProducts.AsQueryable().OrderBy("CaseProductId ASC").ToList().Select(f => CaseProductDtoGen.Create(f, context, tree?[nameof(newObject.CaseProducts)])).ToList();
            }
            else if (propValCaseProducts == null && tree?[nameof(newObject.CaseProducts)] != null)
            {
                newObject.CaseProducts = new CaseProductDtoGen[0];
            }


            newObject.DevTeamAssigned = DevTeamDtoGen.Create(obj.DevTeamAssigned, context, tree?[nameof(newObject.DevTeamAssigned)]);

            return newObject;
        }

        // Instance constructor because there is no way to implement a static interface in C#. And generic constructors don't take arguments.
        public CaseDtoGen CreateInstance(Coalesce.Domain.Case obj, IMappingContext context, IncludeTree tree = null)
        {
            return Create(obj, context, tree);
        }

        // Updates an object from the database to the state handed in by the DTO.
        public void Update(Coalesce.Domain.Case entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            // Applicable includes for Case


            // Applicable excludes for Case
            bool excludePersonListGen = includes == "PersonListGen";

            // Applicable roles for Case


            entity.Title = Title;
            entity.Description = Description;
            entity.OpenedAt = (OpenedAt ?? DateTime.Today);
            entity.AssignedToId = AssignedToId;
            entity.ReportedById = ReportedById;
            entity.Attachment = Attachment;
            entity.Severity = Severity;
            entity.Status = (Status ?? 0);
            entity.DevTeamAssignedId = DevTeamAssignedId;
        }

    }
}
