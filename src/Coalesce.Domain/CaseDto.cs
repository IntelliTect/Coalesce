using IntelliTect.Coalesce.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using IntelliTect.Coalesce.Helpers.IncludeTree;

namespace Coalesce.Domain
{
    public class CaseDto: IClassDto<Case, CaseDto>
    {
        [Key]
        public int CaseId { get; set; }
        public string Title { get; set; }
        [ReadOnly(true)]
        public string AssignedToName { get; set; }

        public void Update(Case obj, ClaimsPrincipal user, string includes)
        {
            obj.Title = Title;
        }

        public CaseDto CreateInstance(Case obj, ClaimsPrincipal user = null, string includes = null, Dictionary<object, object> objects = null, IncludeTree tree = null)
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
