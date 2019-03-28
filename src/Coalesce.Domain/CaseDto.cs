using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using IntelliTect.Coalesce;
using Microsoft.EntityFrameworkCore;
using IntelliTect.Coalesce.Models;

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

        [Coalesce]
        public async Task<string> AsyncMethodOnIClassDto(string input)
        {
            await Task.Delay(500);
            return input;
        }

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

    [Coalesce, DeclaredFor(typeof(CaseDto))]
    public class CaseDtoSource : StandardDataSource<Case, AppDbContext>
    {
        public CaseDtoSource(CrudContext<AppDbContext> context) : base(context)
        {
        }

        public override IQueryable<Case> GetQuery(IDataSourceParameters parameters)
        {
            return Db.Cases.Include(c => c.AssignedTo);
        }
    }

    [Coalesce, DeclaredFor(typeof(CaseDto))]
    public class CaseDtoBehaviors : StandardBehaviors<Case, AppDbContext>
    {
        public CaseDtoBehaviors(CrudContext<AppDbContext> context) : base(context)
        {
        }

        public override ItemResult BeforeSave(SaveKind kind, Case oldItem, Case item)
        {
            return true;
        }
    }
}
