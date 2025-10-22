using IntelliTect.Coalesce;
using IntelliTect.Coalesce.AuditLogging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;

// [assembly: CoalesceConfiguration(NoAutoInclude = true)]

namespace Coalesce.Domain;


[Coalesce]
public class AppDbContext : DbContext, IAuditLogDbContext<AuditLog>
{
#nullable disable
    public DbSet<Person> People { get; set; }
    public DbSet<PersonStats> PeopleStats { get; set; }
    public DbSet<PersonLocation> PeopleLocations { get; set; }

    public DbSet<Case> Cases { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<CaseProduct> CaseProducts { get; set; }
    public DbSet<ZipCode> ZipCodes { get; set; }
    public DbSet<Log> Logs { get; set; }
    public DbSet<DateOnlyPk> DateOnlyPks { get; set; }

    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<AuditLogProperty> AuditLogProperties { get; set; }

    public DbSet<BaseClass> BaseClasses { get; set; }
    public DbSet<BaseClassDerived> BaseClassDerivations { get; set; }

    public DbSet<AbstractClass> AbstractClasses { get; set; }
    public DbSet<AbstractClassImpl> AbstractClassImpls { get; set; }
    public DbSet<AbstractClassPerson> AbstractClassPeople { get; set; }

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
                .ConfigureAudit(config =>
                {
                    static string ShaString(byte[]? bytes) => bytes is null ? "" : "SHA1:" + Convert.ToBase64String(SHA1.HashData(bytes));

                    config
                        .FormatType<byte[]>(ShaString)
                        // Just a random example of audit config:
                        .ExcludeProperty<Person>(p => p.ProfilePic);
                })
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
