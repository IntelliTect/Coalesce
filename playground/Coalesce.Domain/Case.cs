using Coalesce.Domain.External;
using IntelliTect.Coalesce;
using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Helpers;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.Utilities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Coalesce.Domain
{
    [Table("Case")]
    [Create(PermissionLevel = SecurityPermissionLevels.AllowAll)]
    public class Case
    {
        public enum Statuses
        {
            Open,

            [Display(Name = "In Progress")]
            InProgress,

            Resolved,

            [Display(Name = "Closed, No Solution", Description = "The case was closed without being solved.")]
            ClosedNoSolution,

            Cancelled
        }

#nullable disable

        /// <summary>
        /// The Primary key for the Case object
        /// </summary>
        [Key]
        [DefaultOrderBy(OrderByDirection = DefaultOrderByAttribute.OrderByDirections.Ascending, FieldOrder = 3)]
        public int CaseKey { get; set; }

        [ListText]
        [ClientValidation(IsRequired = true, ErrorMessage = "You must enter a title for the case.")]
        [Search(IsSplitOnSpaces = true, SearchMethod = SearchAttribute.SearchMethods.Contains)]
        public string Title { get; set; }

        [Search]
        [Display(Description = "User-provided description of the issue")]
        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        [DateType(), DefaultOrderBy(OrderByDirection = DefaultOrderByAttribute.OrderByDirections.Descending, FieldOrder = 1)]
        [Display(Description = "Date and time when the case was opened")]
        public DateTimeOffset OpenedAt { get; set; }

        public int? AssignedToId { get; set; }

        [ForeignKey("AssignedToId")]
        [DtoExcludes("PersonListGen")]
        [DefaultOrderBy(OrderByDirection = DefaultOrderByAttribute.OrderByDirections.Ascending, FieldName = nameof(Person.FirstName), FieldOrder = 2)]
        public Person AssignedTo { get; set; }

        public int? ReportedById { get; set; }

        [ForeignKey("ReportedById")]
        [DtoExcludes("PersonListGen")]
        [Display(Name = "Reported By", Description = "Person who originally reported the case")]
        public Person ReportedBy { get; set; }

        [Read]
        public long AttachmentSize { get; set; }
        [Read]
        public string AttachmentName { get; set; }
        public string AttachmentType { get; set; }
        [Read, MaxLength(32)]
        public byte[] AttachmentHash { get; set; }
        [InternalUse]
        public CaseAttachmentContent AttachmentContent { get; set; } 
        public class CaseAttachmentContent
        {
            public int CaseKey { get; set; }
            [Required]
            public byte[] Content { get; set; }
        }

        public string Severity { get; set; }

        public Statuses Status { get; set; }

        [ManyToMany("Products")]
        [Search]
        public ICollection<CaseProduct> CaseProducts { get; set; }

        public int? DevTeamAssignedId { get; set; }

        [NotMapped]
        // [ForeignKey("DevTeamAssignedId")]
        public DevTeam DevTeamAssigned { get; set; }

        // EF does support TimeSpans. Some of our projects also do.
        public TimeSpan Duration { get; set; }


#nullable restore

        [Coalesce]
        public static ICollection<string> GetCaseTitles(AppDbContext db, string search) => db.Cases
            .Select(p => p.Title)
            .Distinct()
            .Where(t => t.StartsWith(search))
            .OrderBy(t => t)
            .Take(100)
            .ToList();

        // Arbitrary endpoint to "test" method collection return types.
        [Coalesce]
        public static ICollection<Case> GetSomeCases(AppDbContext db) => db.Cases.Take(10).ToList();

        [Coalesce]
        public static int GetAllOpenCasesCount(AppDbContext db)
        {
            return db.Cases.Count(c => c.Status == Statuses.Open || c.Status == Statuses.InProgress);
        }

        [Coalesce]
        public static void RandomizeDatesAndStatus(AppDbContext db)
        {
            Random random = new Random();
            foreach (var c in db.Cases)
            {
                c.OpenedAt = DateTimeOffset.Now.AddSeconds(-random.Next(10, 50000000));
                c.Status = (Statuses)random.Next(0, (int)Statuses.Cancelled + 1);
            }

            db.SaveChanges();
        }

        [Coalesce]
        public async Task UploadImage(AppDbContext db, IFile file)
        {
            if (file.Content == null)
            {
                return;
            }

            var content = new byte[file.Length];
            await file.Content.ReadAsync(content.AsMemory());

            AttachmentContent = new CaseAttachmentContent() { CaseKey = CaseKey, Content = content };
            AttachmentName = file.Name;
            AttachmentSize = file.Length;
            AttachmentType = file.ContentType;
            AttachmentHash = SHA256.Create().ComputeHash(content);
        }

        [Coalesce]
        [ControllerAction(HttpMethod.Get, VaryByProperty = nameof(AttachmentHash))]
        public IFile DownloadImage(AppDbContext db)
        {
            return new IntelliTect.Coalesce.Models.File(db.Cases
                .WherePrimaryKeyIs(CaseKey)
                .Select(c => c.AttachmentContent.Content)
#if !NET5_0_OR_GREATER
                .First()
#endif
            )
            {
                Name = AttachmentName,
                ContentType = AttachmentType
            };
        }

        [Coalesce]
        public async Task<ItemResult<IFile>> UploadAndDownload(AppDbContext db, IFile file)
        {
            await UploadImage(db, file);
            return new ItemResult<IFile>(DownloadImage(db));
        }

        [Coalesce]
        public async Task UploadImages(AppDbContext db, ICollection<IFile> files)
        {
            foreach (var file in files)
            {
                await UploadImage(db, file);
            }
        }

        [Coalesce]
        public void UploadByteArray(byte[] file)
        {
            AttachmentContent = new CaseAttachmentContent() { Content = file };
        }

        //[DefaultDataSource]
        //public class DefaultSource : StandardDataSource<Case, AppDbContext>
        //{
        //    public DefaultSource(CrudContext<AppDbContext> context) : base(context) { }

        //    // Disabled due to presence of byte[]s and SqlClient's horrible async performance on large fields.
        //    protected override bool CanEvalQueryAsynchronously(IQueryable<Case> query) => false;
        //}

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

        /// <summary>
        /// Returns a list of summary information about Cases
        /// </summary>
        [Coalesce]
        public static CaseSummary GetCaseSummary(AppDbContext db)
        {
            return CaseSummary.GetCaseSummary(db);
        }
    }
}
