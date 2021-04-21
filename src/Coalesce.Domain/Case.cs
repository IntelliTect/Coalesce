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

            [Display(Name = "Closed, No Solution")]
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

        [File("image/jpeg", nameof(ImageName), nameof(ImageHash), nameof(ImageSize))]
        public byte[] Image { get; set; }
        public string ImageName
        {
            get
            {
                return $"Case{CaseKey}.jpg";
            }
        }
        public long ImageSize { get; set; }
        public string ImageHash { get; set; }

        public byte[] Attachment { get; set; }
        public string AttachmentName { get; set; }

        [Edit(PermissionLevel = SecurityPermissionLevels.AllowAuthorized)]
        [Read(SecurityPermissionLevels.AllowAll)]
        [File("text/plain")]
        public byte[] PlainAttachment { get; set; }

        [Edit(PermissionLevel = SecurityPermissionLevels.AllowAuthorized, Roles = "Admin, SuperUser")]
        [File]
        public byte[] RestrictedUploadAttachment { get; set; }

        [Read("Admin", "OtherRole")]
        [File]
        public byte[] RestrictedDownloadAttachment { get; set; }

        [File(NameProperty = nameof(InternalUseFileName),
            SizeProperty = nameof(InternalUseFileSize))]
        public byte[] RestrictedMetaAttachment { get; set; }
        [InternalUse]
        public string InternalUseFileName { get; set; }
        [InternalUse]
        public long InternalUseFileSize { get; set; }

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
        public async Task UploadAttachment(IFile file)
        {
            if (file.Content == null)
            {
                return;
            }

            var ms = new MemoryStream();
            await file.Content.CopyToAsync(ms);
            Attachment = ms.ToArray();
            AttachmentName = file.Name;
        }

        [Coalesce]
        public void UploadByteArray(byte[] file)
        {
            Attachment = file;
        }

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
