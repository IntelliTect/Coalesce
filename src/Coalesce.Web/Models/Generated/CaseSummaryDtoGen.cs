    using IntelliTect.Coalesce.Interfaces;
    using IntelliTect.Coalesce.Mapping;
    using IntelliTect.Coalesce.Models;
    using Newtonsoft.Json;
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Security.Claims;
    using Coalesce.Web.Models;
    using Coalesce.Domain;
    using Coalesce.Domain.External;

using static Coalesce.Domain.CaseSummary;

namespace Coalesce.Web.Models
{
    public partial class CaseSummaryDtoGen : GeneratedDto<CaseSummary, CaseSummaryDtoGen>
        , IClassDto<CaseSummary, CaseSummaryDtoGen>
        {
        public CaseSummaryDtoGen() { }

             public Int32? CaseSummaryId { get; set; }
             public Int32? OpenCases { get; set; }
             public Int32? CaseCount { get; set; }
             public Int32? CloseCases { get; set; }
             public String Description { get; set; }

        // Create a new version of this object or use it from the lookup.
        public static CaseSummaryDtoGen Create(CaseSummary obj, ClaimsPrincipal user = null, string includes = null,
                                   Dictionary<object, object> objects = null) {
            // Return null of the object is null;
            if (obj == null) return null;
                        
            if (objects == null) objects = new Dictionary<object, object>();

            includes = includes ?? "";

            // Applicable includes for CaseSummary
            

            // Applicable excludes for CaseSummary
            

            // Applicable roles for CaseSummary
            if (user != null)
			{
			}



            // See if the object is already created.
            if (objects.ContainsKey(obj)) 
                return (CaseSummaryDtoGen)objects[obj];

            var newObject = new CaseSummaryDtoGen();
            objects.Add(obj, newObject);
            // Fill the properties of the object.
            newObject.CaseSummaryId = obj.CaseSummaryId;
            newObject.OpenCases = obj.OpenCases;
            newObject.CaseCount = obj.CaseCount;
            newObject.CloseCases = obj.CloseCases;
            newObject.Description = obj.Description;
            return newObject;
        }

        // Instance constructor because there is no way to implement a static interface in C#. And generic constructors don't take arguments.
        public CaseSummaryDtoGen CreateInstance(CaseSummary obj, ClaimsPrincipal user = null, string includes = null,
                                Dictionary<object, object> objects = null) {
            return Create(obj, user, includes, objects);
        }

        // Updates an object from the database to the state handed in by the DTO.
        public void Update(CaseSummary entity, ClaimsPrincipal user = null, string includes = null)
        {
            includes = includes ?? "";

            if (OnUpdate(entity, user, includes)) return;

            // Applicable includes for CaseSummary
            

            // Applicable excludes for CaseSummary
            

            // Applicable roles for CaseSummary
            if (user != null)
			{
			}

			entity.OpenCases = (Int32)(OpenCases ?? 0);
			entity.CaseCount = (Int32)(CaseCount ?? 0);
			entity.CloseCases = (Int32)(CloseCases ?? 0);
			entity.Description = Description;
        }

	}
}
