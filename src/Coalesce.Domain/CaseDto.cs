using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using IntelliTect.Coalesce;

namespace Coalesce.Domain
{
    [Coalesce]
    public class CaseDto : IClassDto<Case>
    {
        [Key]
        public int CaseId { get; set; }
        public string Title { get; set; }
        [ReadOnly(true)]
        public string AssignedToName { get; set; }

        public void MapTo(Case obj, IMappingContext context)
        {
            obj.Title = Title;
        }

        public void MapFrom(Case obj, IMappingContext context = null, IncludeTree tree = null)
        {
            CaseId = obj.CaseKey;
            Title = obj.Title;
            if (obj.AssignedTo != null)
            {
                AssignedToName = obj.AssignedTo.Name;
            }
        }
    }
}
