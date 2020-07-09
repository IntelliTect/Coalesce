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

#nullable disable

        [Key]
        public int CaseId { get; set; }

        public string Title { get; set; }

        [ReadOnly(true)]
        public string AssignedToName { get; set; }

#nullable restore

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

        public void MapFrom(Case obj, IMappingContext context, IncludeTree? tree = null)
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
    public class CaseDtoSource : ProjectedDtoDataSource<Case, CaseDto, AppDbContext>
    {
        public CaseDtoSource(CrudContext<AppDbContext> context) : base(context)
        {
        }

        public override IQueryable<Case> GetQuery(IDataSourceParameters parameters)
        {
            return Db.Cases.Include(c => c.AssignedTo);
        }

        public override IQueryable<CaseDto> ApplyProjection(IQueryable<Case> query, IDataSourceParameters parameters)
        {
            return query.Select(c => new CaseDto
            {
                CaseId = c.CaseKey,
                Title = c.Title,
                AssignedToName = c.AssignedTo == null ? null : c.AssignedTo.Name
            });
        }
    }

    [Coalesce, DeclaredFor(typeof(CaseDto))]
    public class CaseDtoBehaviors : StandardBehaviors<Case, AppDbContext>
    {
        public CaseDtoBehaviors(CrudContext<AppDbContext> context) : base(context)
        {
        }

        public override ItemResult BeforeSave(SaveKind kind, Case? oldItem, Case item)
        {
            return true;
        }
    }
}
