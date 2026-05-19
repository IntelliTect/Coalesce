using Microsoft.EntityFrameworkCore;

namespace IntelliTect.Coalesce.MultiTenancy.Tests;

// ── Self-contained entity hierarchy ──────────────────────────────────────

interface ITenanted
{
    string TenantId { get; set; }
}

class TenantRoot : ITenanted
{
    public string TenantRootId { get; set; } = Guid.NewGuid().ToString("N");
    public string TenantId { get; set; } = Guid.NewGuid().ToString("N");
    public string Name { get; set; } = null!;
}

// Entity with an integer PK to test the SQLite AUTOINCREMENT workaround
class TenantedWithIntPk : ITenanted
{
    public int Id { get; set; }
    public string TenantId { get; set; } = null!;
    public string Value { get; set; } = null!;
}

// Base TPH class
class Animal : ITenanted
{
    public string AnimalId { get; set; } = Guid.NewGuid().ToString("N");
    public string TenantId { get; set; } = null!;
    public string Name { get; set; } = null!;
}

// Derived TPH class — shares the Animals table, inherits Animal's PK
class Dog : Animal
{
    public string Breed { get; set; } = null!;
}

// References the *derived* type Dog specifically.
// Its FK must be rewired to include TenantId even though Dog's PK is already
// composite when Dog is processed by ConfigureMultiTenancy.
class DogTag : ITenanted
{
    public string DogTagId { get; set; } = Guid.NewGuid().ToString("N");
    public string TenantId { get; set; } = null!;
    public string DogId { get; set; } = null!;
    public Dog? Dog { get; set; }
}

// Non-tenanted entity
class Untenanted
{
    public int Id { get; set; }
    public string Value { get; set; } = null!;
}

// Has a TenantId property but does NOT implement ITenanted — should be left alone
class HasTenantIdButNotITenanted
{
    public int Id { get; set; }
    public string TenantId { get; set; } = null!;
    public string Value { get; set; } = null!;
}

// Self-referencing tenanted entity (e.g., parent-child hierarchy)
class Category : ITenanted
{
    public string CategoryId { get; set; } = Guid.NewGuid().ToString("N");
    public string TenantId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? ParentCategoryId { get; set; }
    public Category? Parent { get; set; }
    public List<Category> Children { get; set; } = [];
}

// Entity with an explicit composite PK that already includes TenantId — should not be double-expanded
class AlreadyCompositeKey : ITenanted
{
    public string TenantId { get; set; } = null!;
    public string Key1 { get; set; } = Guid.NewGuid().ToString("N");
    public string Key2 { get; set; } = Guid.NewGuid().ToString("N");
    public string Value { get; set; } = null!;
}

// Entity where TenantId is the sole PK — should be left as-is
class TenantIdIsPk : ITenanted
{
    public string TenantId { get; set; } = null!;
    public string Value { get; set; } = null!;
}

// Entity with multiple FKs to the same principal
class AnimalRecord : ITenanted
{
    public string AnimalRecordId { get; set; } = Guid.NewGuid().ToString("N");
    public string TenantId { get; set; } = null!;
    public string SubjectAnimalId { get; set; } = null!;
    public Animal? SubjectAnimal { get; set; }
    public string? VetAnimalId { get; set; }
    public Animal? VetAnimal { get; set; }
    public string Notes { get; set; } = null!;
}

// Entity with an optional (nullable) FK to a tenanted entity
class OptionalRef : ITenanted
{
    public string OptionalRefId { get; set; } = Guid.NewGuid().ToString("N");
    public string TenantId { get; set; } = null!;
    public string? AnimalId { get; set; }
    public Animal? Animal { get; set; }
}

// ── Self-contained DbContext ──────────────────────────────────────────────

class TestDbContext : DbContext
{
    public string? CurrentTenantId { get; set; }
    public string TenantIdOrThrow => CurrentTenantId ?? throw new InvalidOperationException("TenantId not set");

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    public DbSet<TenantRoot> TenantRoots => Set<TenantRoot>();
    public DbSet<TenantedWithIntPk> TenantedWithIntPks => Set<TenantedWithIntPk>();
    public DbSet<Animal> Animals => Set<Animal>();
    public DbSet<Dog> Dogs => Set<Dog>();
    public DbSet<DogTag> DogTags => Set<DogTag>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<AlreadyCompositeKey> AlreadyCompositeKeys => Set<AlreadyCompositeKey>();
    public DbSet<TenantIdIsPk> TenantIdIsPks => Set<TenantIdIsPk>();
    public DbSet<AnimalRecord> AnimalRecords => Set<AnimalRecord>();
    public DbSet<OptionalRef> OptionalRefs => Set<OptionalRef>();
    public DbSet<Untenanted> Untenanted => Set<Untenanted>();
    public DbSet<HasTenantIdButNotITenanted> NotTenanted => Set<HasTenantIdButNotITenanted>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseCoalesceMultiTenancy<ITenanted>(t => t.TenantId, nameof(TenantIdOrThrow));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<AlreadyCompositeKey>().HasKey(e => new { e.TenantId, e.Key1, e.Key2 });
        modelBuilder.Entity<TenantIdIsPk>().HasKey(e => e.TenantId);
    }
}
