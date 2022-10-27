using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext
{
    [Coalesce]
    public class AppDbContext : DbContext
    {
        public DbSet<Person> People { get; set; }
        public DbSet<Sibling> Siblings { get; set; }
        public DbSet<Case> Cases { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<CaseProduct> CaseProducts { get; set; }

        public DbSet<ComplexModel> ComplexModels { get; set; }
        public DbSet<Test> Tests { get; set; }

        public DbSet<AbstractModel> AbstractModels { get; set; }

        public AppDbContext() : this(Guid.NewGuid().ToString()) { }

        public AppDbContext(string memoryDatabaseName)
            : base(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(memoryDatabaseName).ConfigureWarnings(w =>
            {
#if NET5_0_OR_GREATER
            w.Ignore(CoreEventId.NavigationBaseIncludeIgnored);
#endif
            }).Options)
        { }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AbstractModel>()
                .HasDiscriminator(b => b.Discriminatior)
                .HasValue<AbstractImpl>("impl");
        }
    }
}
