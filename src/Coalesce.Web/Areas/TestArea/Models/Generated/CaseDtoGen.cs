
using System;
using System.Collections.Generic;
using System.Security.Claims;
using IntelliTect.Coalesce.Interfaces;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.Mapping;
using System.Linq;
using Newtonsoft.Json;
// Model Namespaces
    using Coalesce.Domain;
    using Coalesce.Domain.External;
using static Coalesce.Domain.Case;

namespace Coalesce.Web.TestArea.Models
{
    public partial class CaseDtoGen : GeneratedDto<Case, CaseDtoGen>
        , IClassDto<Case, CaseDtoGen>
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
        public static CaseDtoGen Create(Case obj, ClaimsPrincipal user = null, string includes = null,
                                   Dictionary<object, object> objects = null) {
            // Return null of the object is null;
            if (obj == null) return null;
                        
            if (objects == null) objects = new Dictionary<object, object>();

            includes = includes ?? "";

            // Applicable includes for Case
            

            // Applicable excludes for Case
            

            // Applicable roles for Case
            if (user != null)
			{
			}



            // See if the object is already created.
            if (objects.ContainsKey(obj)) 
                return (CaseDtoGen)objects[obj];

            var newObject = new CaseDtoGen();
            objects.Add(obj, newObject);
            // Fill the properties of the object.
            newObject.CaseKey = obj.CaseKey;
            newObject.Title = obj.Title;
            newObject.Description = obj.Description;
            newObject.OpenedAt = obj.OpenedAt;
            newObject.AssignedToId = obj.AssignedToId;
            newObject.AssignedTo = PersonDtoGen.Create(obj.AssignedTo, user, includes, objects);
            newObject.ReportedById = obj.ReportedById;
            newObject.ReportedBy = PersonDtoGen.Create(obj.ReportedBy, user, includes, objects);
            newObject.Attachment = obj.Attachment;
            newObject.Severity = obj.Severity;
            newObject.Status = obj.Status;
            if (obj.CaseProducts != null) newObject.CaseProducts = obj.CaseProducts.Select(f => CaseProductDtoGen.Create(f, user, includes, objects)).ToList();
            newObject.DevTeamAssignedId = obj.DevTeamAssignedId;
            newObject.DevTeamAssigned = DevTeamDtoGen.Create(obj.DevTeamAssigned, user, includes, objects);
            return newObject;
        }

        // Instance constructor because there is no way to implement a static interface in C#. And generic constructors don't take arguments.
        public CaseDtoGen CreateInstance(Case obj, ClaimsPrincipal user = null, string includes = null,
                                Dictionary<object, object> objects = null) {
            return Create(obj, user, includes, objects);
        }

        // Updates an object from the database to the state handed in by the DTO.
        public void Update(Case entity, ClaimsPrincipal user = null, string includes = null)
        {
            includes = includes ?? "";

            if (OnUpdate(entity, user, includes)) return;

            // Applicable includes for Case
            

            // Applicable excludes for Case
            

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
