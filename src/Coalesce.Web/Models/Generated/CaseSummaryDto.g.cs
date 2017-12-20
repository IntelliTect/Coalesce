
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
    public partial class CaseSummaryDtoGen : GeneratedDto<Coalesce.Domain.CaseSummary>
    {
        public CaseSummaryDtoGen() { }

        public int? CaseSummaryId { get; set; }
        public int? OpenCases { get; set; }
        public int? CaseCount { get; set; }
        public int? CloseCases { get; set; }
        public string Description { get; set; }

        public override void MapFrom(Coalesce.Domain.CaseSummary obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            // Applicable includes for CaseSummary


            // Applicable excludes for CaseSummary


            // Applicable roles for CaseSummary


            // Fill the properties of the object.
            this.CaseSummaryId = obj.CaseSummaryId;
            this.OpenCases = obj.OpenCases;
            this.CaseCount = obj.CaseCount;
            this.CloseCases = obj.CloseCases;
            this.Description = obj.Description;
        }

        // Updates an object from the database to the state handed in by the DTO.
        public override void MapTo(Coalesce.Domain.CaseSummary entity, IMappingContext context)
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
