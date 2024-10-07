using IntelliTect.Coalesce;
using IntelliTect.Coalesce.AuditLogging;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

// [assembly: CoalesceConfiguration(NoAutoInclude = true)]

namespace Coalesce.Domain
{

    [Coalesce]
    public class AppDbContext : DbContext, IAuditLogDbContext<AuditLog>
    {
#nullable disable
        public DbSet<Person> People { get; set; }
        public DbSet<Case> Cases { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<CaseProduct> CaseProducts { get; set; }
        public DbSet<ZipCode> ZipCodes { get; set; }
        public DbSet<Log> Logs { get; set; }

        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<AuditLogProperty> AuditLogProperties { get; set; }

#nullable restore

        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseCoalesceAuditLogging<AuditLog>(x => x
                    .WithAugmentation<OperationContext>()
                    .WithMergeWindow(TimeSpan.FromSeconds(15))
                    .ConfigureAudit(x => x
                        // Just a random example of audit config:
                        .ExcludeProperty<Person>(p => p.ProfilePic)
                    )
                );
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Remove cascading deletes.
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }

            modelBuilder.Entity<Product>().OwnsOne(p => p.Details, cb =>
            {
                cb.OwnsOne(c => c.ManufacturingAddress);
                cb.OwnsOne(c => c.CompanyHqAddress);
            });

            //modelBuilder.Entity<Person>()
            //    .HasMany(p => p.CasesAssigned)
            //    .WithOne(p => p.AssignedTo)
            //    .HasForeignKey(p => p.AssignedToId)
            //    .HasPrincipalKey(p => p.PersonId)
            //    .OnDelete(DeleteBehavior.Restrict);

            //modelBuilder.Entity<Person>()
            //    .HasMany(p => p.CasesReported)
            //    .WithOne(p => p.ReportedBy)
            //    .HasForeignKey(p => p.ReportedById)
            //    .HasPrincipalKey(p => p.PersonId)
            //    .OnDelete(DeleteBehavior.Restrict);

            modelBuilder
                .Entity<Case>()
                .ToTable("Case")
                .HasOne(c => c.AttachmentContent)
                .WithOne()
                .HasForeignKey<Case.CaseAttachmentContent>(c => c.CaseKey);
            modelBuilder
                .Entity<Case.CaseAttachmentContent>()
                .ToTable("Case")
                .HasKey(d => d.CaseKey);

        }
    }
}
