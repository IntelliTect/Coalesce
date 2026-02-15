using IntelliTect.Coalesce.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;

namespace IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;

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
    public DbSet<RequiredInternalUseModel> RequiredInternalUseModels { get; set; }
    public DbSet<Test> Tests { get; set; }

    public DbSet<AbstractModel> AbstractModels { get; set; }
    public DbSet<AbstractModelPerson> AbstractModelPeople { get; set; }
    public DbSet<AbstractImpl1> AbstractImpl1s { get; set; }
    public DbSet<AbstractImpl2> AbstractImpl2s { get; set; }

    public DbSet<EnumPk> EnumPks { get; set; }
    public DbSet<ZipCode> ZipCodes { get; set; }

    public DbSet<StringIdentity> StringIdentities { get; set; }

    public DbSet<RecursiveHierarchy> RecursiveHierarchies { get; set; }

    [InternalUse]
    public DbSet<DbSetIsInternalUse> Internals { get; set; }

    public DbSet<OneToOneParent> OneToOneParents { get; set; }
    public DbSet<OneToOneSharedKeyChild1> OneToOneSharedKeyChild1s { get; set; }
    public DbSet<OneToOneSharedKeyChild2> OneToOneSharedKeyChild2s { get; set; }
    public DbSet<OneToOneSeparateKeyChild> OneToOneSeparateKeyChildren { get; set; }
    public DbSet<OneToOneManyChildren> OneToOneManyChildren { get; set; }

    public DbSet<MultipleParents> MultipleParents { get; set; }
    public DbSet<Parent1> Parent1s { get; set; }
    public DbSet<Parent2> Parent2s { get; set; }

    public DbSet<DateTimePk> DateTimePks { get; set; }
    public DbSet<DateTimeOffsetPk> DateTimeOffsetPks { get; set; }
    public DbSet<DateOnlyPk> DateOnlyPks { get; set; }
    public DbSet<TimeOnlyPk> TimeOnlyPks { get; set; }

    public DbSet<SuppressedDefaultOrdering> SuppressedDefaultOrderings { get; set; }


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
            .HasDiscriminator(b => b.Discriminator)
            .HasValue<AbstractImpl1>("impl1")
            .HasValue<AbstractImpl2>("impl2");
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
