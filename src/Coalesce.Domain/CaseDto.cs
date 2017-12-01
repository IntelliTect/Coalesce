using IntelliTect.Coalesce.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using IntelliTect.Coalesce.Helpers.IncludeTree;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.DataAnnotations;

namespace Coalesce.Domain
{
    [Coalesce]
    public class CaseDto : IClassDto<Case, CaseDto>
    {
        [Key]
        public int CaseId { get; set; }
        public string Title { get; set; }
        [ReadOnly(true)]
        public string AssignedToName { get; set; }

        public void Update(Case obj, IMappingContext context)
        {
            obj.Title = Title;
        }

        public CaseDto CreateInstance(Case obj, IMappingContext context = null, IncludeTree tree = null)
        {
            var dto = new CaseDto();
            dto.CaseId = obj.CaseKey;
            dto.Title = obj.Title;
            if (obj.AssignedTo != null)
            {
                dto.AssignedToName = obj.AssignedTo.Name;
            }
            return dto;
        }
    }
}
