
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
    public partial class CaseSummaryDtoGen : GeneratedDto<Coalesce.Domain.CaseSummary, CaseSummaryDtoGen>
        , IClassDto<Coalesce.Domain.CaseSummary, CaseSummaryDtoGen>
    {
        public CaseSummaryDtoGen() { }

        public int? CaseSummaryId { get; set; }
        public int? OpenCases { get; set; }
        public int? CaseCount { get; set; }
        public int? CloseCases { get; set; }
        public string Description { get; set; }

        // Create a new version of this object or use it from the lookup.
        public static CaseSummaryDtoGen Create(Coalesce.Domain.CaseSummary obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return null;
            var includes = context.Includes;

            // Applicable includes for CaseSummary


            // Applicable excludes for CaseSummary


            // Applicable roles for CaseSummary



            // See if the object is already created, but only if we aren't restricting by an includes tree.
            // If we do have an IncludeTree, we know the exact structure of our return data, so we don't need to worry about circular refs.
            if (tree == null && context.TryGetMapping(obj, out CaseSummaryDtoGen existing)) return existing;

            var newObject = new CaseSummaryDtoGen();
            if (tree == null) context.AddMapping(obj, newObject);
            // Fill the properties of the object.
            newObject.CaseSummaryId = obj.CaseSummaryId;
            newObject.OpenCases = obj.OpenCases;
            newObject.CaseCount = obj.CaseCount;
            newObject.CloseCases = obj.CloseCases;
            newObject.Description = obj.Description;
            return newObject;
        }

        // Instance constructor because there is no way to implement a static interface in C#. And generic constructors don't take arguments.
        public CaseSummaryDtoGen CreateInstance(Coalesce.Domain.CaseSummary obj, IMappingContext context, IncludeTree tree = null)
        {
            return Create(obj, context, tree);
        }

        // Updates an object from the database to the state handed in by the DTO.
        public void Update(Coalesce.Domain.CaseSummary entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            // Applicable includes for CaseSummary


            // Applicable excludes for CaseSummary


            // Applicable roles for CaseSummary


            entity.OpenCases = (OpenCases ?? 0);
            entity.CaseCount = (CaseCount ?? 0);
            entity.CloseCases = (CloseCases ?? 0);
            entity.Description = Description;
        }

    }
}
