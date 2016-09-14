    using IntelliTect.Coalesce.Interfaces;
    using IntelliTect.Coalesce.Mapping;
    using IntelliTect.Coalesce.Models;
    using IntelliTect.Coalesce.Helpers.IncludeTree;
    using Newtonsoft.Json;
    using System;
    using System.Linq;
    using System.Linq.Dynamic;
    using System.Collections.Generic;
    using System.Security.Claims;
    using Coalesce.Web.Models;
    using Coalesce.Domain;
    using Coalesce.Domain.External;

using static Coalesce.Domain.Case;

namespace Coalesce.Web.Models
{
    public partial class CaseDtoGen : GeneratedDto<Coalesce.Domain.Case, CaseDtoGen>
        , IClassDto<Coalesce.Domain.Case, CaseDtoGen>
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

        // Create a new version of this object or use it from the lookup.
        public static CaseDtoGen Create(Coalesce.Domain.Case obj, ClaimsPrincipal user = null, string includes = null,
                                   Dictionary<object, object> objects = null, IncludeTree tree = null) {
            // Return null of the object is null;
            if (obj == null) return null;
                        
            if (objects == null) objects = new Dictionary<object, object>();

            includes = includes ?? "";

            // Applicable includes for Case
            

            // Applicable excludes for Case
            bool excludePersonListGen = includes == "PersonListGen";

            // Applicable roles for Case
            if (user != null)
			{
			}



            // See if the object is already created, but only if we aren't restricting by an includes tree.
            // If we do have an IncludeTree, we know the exact structure of our return data, so we don't need to worry about circular refs.
            if (tree == null && objects.ContainsKey(obj)) 
                return (CaseDtoGen)objects[obj];

            var newObject = new CaseDtoGen();
            if (tree == null) objects.Add(obj, newObject);
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
                newObject.AssignedTo = PersonDtoGen.Create(obj.AssignedTo, user, includes, objects, tree?[nameof(newObject.AssignedTo)]);

            }
            if (!(excludePersonListGen))
            {
                if (tree == null || tree[nameof(newObject.ReportedBy)] != null)
                newObject.ReportedBy = PersonDtoGen.Create(obj.ReportedBy, user, includes, objects, tree?[nameof(newObject.ReportedBy)]);

            }
            if (obj.CaseProducts != null && (tree == null || tree[nameof(newObject.CaseProducts)] != null))
                newObject.CaseProducts = obj.CaseProducts.OrderBy("CaseProductId ASC").Select(f => CaseProductDtoGen.Create(f, user, includes, objects, tree?[nameof(newObject.CaseProducts)])).ToList();

            
                newObject.DevTeamAssigned = DevTeamDtoGen.Create(obj.DevTeamAssigned, user, includes, objects, tree?[nameof(newObject.DevTeamAssigned)]);

            return newObject;
        }

        // Instance constructor because there is no way to implement a static interface in C#. And generic constructors don't take arguments.
        public CaseDtoGen CreateInstance(Coalesce.Domain.Case obj, ClaimsPrincipal user = null, string includes = null,
                                Dictionary<object, object> objects = null, IncludeTree tree = null) {
            return Create(obj, user, includes, objects, tree);
        }

        // Updates an object from the database to the state handed in by the DTO.
        public void Update(Coalesce.Domain.Case entity, ClaimsPrincipal user = null, string includes = null)
        {
            includes = includes ?? "";

            if (OnUpdate(entity, user, includes)) return;

            // Applicable includes for Case
            

            // Applicable excludes for Case
            bool excludePersonListGen = includes == "PersonListGen";

            // Applicable roles for Case
            if (user != null)
			{
			}

			entity.Title = Title;
			entity.Description = Description;
			entity.OpenedAt = (DateTimeOffset)(OpenedAt ?? DateTime.Today);
			entity.AssignedToId = AssignedToId;
			entity.ReportedById = ReportedById;
			entity.Attachment = Attachment;
			entity.Severity = Severity;
			entity.Status = (Statuses)(Status ?? 0);
			entity.DevTeamAssignedId = DevTeamAssignedId;
        }

	}
}
