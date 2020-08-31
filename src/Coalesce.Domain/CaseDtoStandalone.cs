using System.ComponentModel.DataAnnotations;
using IntelliTect.Coalesce;

namespace Coalesce.Domain
{
    [Coalesce]
    public class CaseDtoStandalone : IClassDto<Case, AppDbContext>
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
