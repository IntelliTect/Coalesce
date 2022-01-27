using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using IntelliTect.Coalesce;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;

namespace IntelliTect.Coalesce.Tests.TargetClasses
{
    [Coalesce]
    public class CaseDtoStandalone : IClassDto<Case, TestDbContext.TestDbContext>
    {
        [Key]
        public int CaseId { get; set; }

        public string? Title { get; set; }

        public void MapTo(Case obj, IMappingContext context)
        {
            obj.Title = Title;
        }

        public void MapFrom(Case obj, IMappingContext context, IncludeTree? tree = null)
        {
            CaseId = obj.CaseKey;
            Title = obj.Title;
        }
    }
}
