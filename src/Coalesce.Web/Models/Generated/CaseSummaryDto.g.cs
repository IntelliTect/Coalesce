using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public override void MapFrom(Coalesce.Domain.CaseSummary obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            // Fill the properties of the object.

            this.CaseSummaryId = obj.CaseSummaryId;
            this.OpenCases = obj.OpenCases;
            this.CaseCount = obj.CaseCount;
            this.CloseCases = obj.CloseCases;
            this.Description = obj.Description;
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public override void MapTo(Coalesce.Domain.CaseSummary entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (OnUpdate(entity, context)) return;

            entity.CaseSummaryId = (CaseSummaryId ?? entity.CaseSummaryId);
            entity.OpenCases = (OpenCases ?? entity.OpenCases);
            entity.CaseCount = (CaseCount ?? entity.CaseCount);
            entity.CloseCases = (CloseCases ?? entity.CloseCases);
            entity.Description = Description;
        }
    }
}
