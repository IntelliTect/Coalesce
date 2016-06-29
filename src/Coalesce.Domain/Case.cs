using Coalesce.Domain.External;
using Intellitect.ComponentModel.Data;
using Intellitect.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Coalesce.Domain
{
    [Table("Case")]
    public class Case : IIncludable<Case>, IIncludeExternal<Case>
    {
        public enum Statuses
        {
            Open,
            [Display(Name = "In Progress")]
            InProgress,
            Resolved,
            [Display(Name = "Closed, No Solution")]
            ClosedNoSolution,
            Cancelled
        }
        /// <summary>
        /// The Primary key for the Case object
        /// </summary>
        [Key]
        public int CaseKey { get; set; }
        [ClientValidation(IsRequired = true, ErrorMessage = "You must enter a title for the case.")]
        public string Title { get; set; }
        public string Description { get; set; }
        [DateType()]
        public DateTimeOffset OpenedAt { get; set; }

        public int? AssignedToId { get; set; }
        [ForeignKey("AssignedToId")]
        public Person AssignedTo { get; set; }

        public int? ReportedById { get; set; }
        [ForeignKey("ReportedById")]
        [Display(Name = "Reported By")]
        public Person ReportedBy { get; set; }

        [FileDownload]
        public byte[] Attachment { get; set; }

        [ListGroup("Severity")]
        public string Severity { get; set; }
        public Statuses Status { get; set; }
        [ManyToMany("Products")]
        public ICollection<CaseProduct> CaseProducts { get; set; }


        public int? DevTeamAssignedId { get; set; }
        //[NotMapped]
        [ForeignKey("DevTeamAssignedId")]
        public DevTeam DevTeamAssigned { get; set; }


        public static int GetAllOpenCasesCount(AppDbContext db)
        {
            return db.Cases.Count(c => c.Status == Statuses.Open || c.Status == Statuses.InProgress);
        }

        public static IQueryable<Case> GetAllOpenCases(int x, int y, AppDbContext db)
        {
            return db.Cases.Where(c => c.Status == Statuses.Open || c.Status == Statuses.InProgress).Include(c => c.AssignedTo).Include(c => c.ReportedBy);
        }

        public IQueryable<Case> Include(IQueryable<Case> entities, string include = null)
        {
            return entities.Include(c => c.ReportedBy)
                .Include(c => c.AssignedTo)
                .Include(c => c.CaseProducts).ThenInclude(cp => cp.Product);
            //return entities.Include(c => c.CaseProducts);
        }

        public IEnumerable<Case> IncludeExternal(IEnumerable<Case> entities, string include = null)
        {
            return entities.IncludeExternal(f => f.DevTeamAssigned);
        }
    }
}
