using IntelliTect.Coalesce.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;

[Table("Case")]
[Create(PermissionLevel = SecurityPermissionLevels.AllowAll)]
public class Case
{
    public enum Statuses
    {
        Open,

        [Display(Name = "In Progress")]
        InProgress,

        [Description("Closed with a solution.")]
        Resolved,

        [Display(Name = "Closed, No Solution", Description = "Closed without any resolution.")]
        ClosedNoSolution,

        /// <summary>
        /// doc comment on enum member
        /// </summary>
        Cancelled = 99
    }
    
    [Key]
    public int CaseKey { get; set; }

    [ListText]
    [ClientValidation(IsRequired = true, ErrorMessage = "You must enter a title for the case.")]
    [Search(IsSplitOnSpaces = true, SearchMethod = SearchAttribute.SearchMethods.Contains)]
    public string Title { get; set; }

    [Search]
    [DataType(DataType.MultilineText)]
    public string Description { get; set; }
    
    public DateTimeOffset OpenedAt { get; set; }

    public int? AssignedToId { get; set; }

    [ForeignKey("AssignedToId")]
    public Person AssignedTo { get; set; }

    public int? ReportedById { get; set; }

    [ForeignKey("ReportedById")]
    [Display(Name = "Reported By")]
    public Person ReportedBy { get; set; }

    public byte[] Attachment { get; set; }

    [InternalUse]
    public string InternalProp { get; set; }

    public Statuses Status { get; set; }

    [Search]
    [ManyToMany("Products")]
    public ICollection<CaseProduct> CaseProducts { get; set; }

    [Coalesce]
    [SemanticKernel("MethodWithJsReservedParamName")]
    public string MethodWithJsReservedParamName(
        Case @case, string function, int var, bool async, string await,
        string[] arguments, string implements, bool delete, bool @true
    )
        => @case.ToString();
    
    public class AllOpenCases : StandardDataSource<Case, AppDbContext>
    {
        public AllOpenCases(CrudContext<AppDbContext> context) : base(context) { }

        [Coalesce]
        public DateTimeOffset? MinDate { get; set; }

        public override IQueryable<Case> GetQuery(IDataSourceParameters parameters) => Db.Cases
            .Where(c => c.Status == Statuses.Open || c.Status == Statuses.InProgress)
            .Where(c => MinDate == null || c.OpenedAt > MinDate)
            .IncludeChildren();
    }
}
