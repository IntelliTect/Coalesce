using IntelliTect.Coalesce.DataAnnotations;
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
        public DbSet<ComplexModelDependent> ComplexModelDependents { get; set; }
        public DbSet<ReadOnlyEntityUsedAsMethodInput> ReadOnlyEntityUsedAsMethodInputs { get; set; }
        public DbSet<RequiredAndInitModel> RequiredAndInitModels { get; set; }
        public DbSet<Test> Tests { get; set; }

        public DbSet<AbstractModel> AbstractModels { get; set; }
        public DbSet<AbstractImpl> AbstractImpls { get; set; }

        public DbSet<EnumPk> EnumPks { get; set; }
        public DbSet<ZipCode> ZipCodes { get; set; }

        public DbSet<StringIdentity> StringIdentities { get; set; }

        public DbSet<RecursiveHierarchy> RecursiveHierarchies { get; set; }

        [InternalUse]
        public DbSet<DbSetIsInternalUse> Internals { get; set; }

        public DbSet<OneToOneParent> OneToOneParents { get; set; }
        public DbSet<OneToOneChild1> OneToOneChild1s { get; set; }
        public DbSet<OneToOneChild2> OneToOneChild2s { get; set; }
        public DbSet<OneToOneManyChildren> OneToOneManyChildren { get; set; }


        public AppDbContext() : this(Guid.NewGuid().ToString()) { }

        public AppDbContext(string memoryDatabaseName)
            : base(new DbContextOptionsBuilder<AppDbContext>().UseInMemoryDatabase(memoryDatabaseName).ConfigureWarnings(w =>
            {
                w.Ignore(CoreEventId.NavigationBaseIncludeIgnored);
            }).Options)
        { }

        public AppDbContext(DbContextOptions options)
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


    public class TestBehaviors<T, TContext> : StandardBehaviors<T, TContext>
        where T : class
        where TContext : DbContext
    {
        public TestBehaviors(CrudContext<TContext> context) : base(context)
        {
        }
    }

}
