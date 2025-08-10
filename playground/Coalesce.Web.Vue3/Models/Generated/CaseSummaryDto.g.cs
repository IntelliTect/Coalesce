using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace Coalesce.Web.Vue3.Models
{
    public partial class CaseSummaryParameter : SparseDto, IGeneratedParameterDto<Coalesce.Domain.CaseSummary>
    {
        public CaseSummaryParameter() { }

        private int? _CaseSummaryId;
        private int? _OpenCases;
        private int? _CaseCount;
        private int? _CloseCases;
        private string _Description;
        private System.Collections.Generic.IDictionary<string, int> _TestDict;

        public int? CaseSummaryId
        {
            get => _CaseSummaryId;
            set { _CaseSummaryId = value; Changed(nameof(CaseSummaryId)); }
        }
        public int? OpenCases
        {
            get => _OpenCases;
            set { _OpenCases = value; Changed(nameof(OpenCases)); }
        }
        public int? CaseCount
        {
            get => _CaseCount;
            set { _CaseCount = value; Changed(nameof(CaseCount)); }
        }
        public int? CloseCases
        {
            get => _CloseCases;
            set { _CloseCases = value; Changed(nameof(CloseCases)); }
        }
        public string Description
        {
            get => _Description;
            set { _Description = value; Changed(nameof(Description)); }
        }
        public System.Collections.Generic.IDictionary<string, int> TestDict
        {
            get => _TestDict;
            set { _TestDict = value; Changed(nameof(TestDict)); }
        }

        /// <summary>
        /// Map from the current DTO instance to the domain object.
        /// </summary>
        public void MapTo(Coalesce.Domain.CaseSummary entity, IMappingContext context)
        {
            var includes = context.Includes;

            if (ShouldMapTo(nameof(CaseSummaryId))) entity.CaseSummaryId = (CaseSummaryId ?? entity.CaseSummaryId);
            if (ShouldMapTo(nameof(OpenCases))) entity.OpenCases = (OpenCases ?? entity.OpenCases);
            if (ShouldMapTo(nameof(CaseCount))) entity.CaseCount = (CaseCount ?? entity.CaseCount);
            if (ShouldMapTo(nameof(CloseCases))) entity.CloseCases = (CloseCases ?? entity.CloseCases);
            if (ShouldMapTo(nameof(Description))) entity.Description = Description;
            if (ShouldMapTo(nameof(TestDict))) entity.TestDict = TestDict?.ToDictionary(k => k.Key, v => v.Value);
        }

        /// <summary>
        /// Map from the current DTO instance to a new instance of the domain object.
        /// </summary>
        public Coalesce.Domain.CaseSummary MapToNew(IMappingContext context)
        {
            var entity = new Coalesce.Domain.CaseSummary();
            MapTo(entity, context);
            return entity;
        }

        public Coalesce.Domain.CaseSummary MapToModelOrNew(Coalesce.Domain.CaseSummary obj, IMappingContext context)
        {
            if (obj is null) return MapToNew(context);
            MapTo(obj, context);
            return obj;
        }
    }

    public partial class CaseSummaryResponse : IGeneratedResponseDto<Coalesce.Domain.CaseSummary>
    {
        public CaseSummaryResponse() { }

        public int? CaseSummaryId { get; set; }
        public int? OpenCases { get; set; }
        public int? CaseCount { get; set; }
        public int? CloseCases { get; set; }
        public string Description { get; set; }
        public System.Collections.Generic.IDictionary<string, int> TestDict { get; set; }

        /// <summary>
        /// Map from the domain object to the properties of the current DTO instance.
        /// </summary>
        public void MapFrom(Coalesce.Domain.CaseSummary obj, IMappingContext context, IncludeTree tree = null)
        {
            if (obj == null) return;
            var includes = context.Includes;

            this.CaseSummaryId = obj.CaseSummaryId;
            this.OpenCases = obj.OpenCases;
            this.CaseCount = obj.CaseCount;
            this.CloseCases = obj.CloseCases;
            this.Description = obj.Description;
            this.TestDict = obj.TestDict;
        }
    }
}
