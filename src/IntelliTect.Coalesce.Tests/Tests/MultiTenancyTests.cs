using IntelliTect.Coalesce.MultiTenancy;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#nullable enable

namespace IntelliTect.Coalesce.Tests.Tests;

/// <summary>
/// Tests for <see cref="MultiTenancyConvention{TTenanted}"/>
/// and <see cref="MultiTenancyDbContextOptionsBuilderExtensions.UseMultiTenancy{TTenanted}"/>.
/// All entity types and DbContext are self-contained within this file.
/// </summary>
public class MultiTenancyTests
{
    // ── Self-contained entity hierarchy ──────────────────────────────────────

    interface ITenanted
    {
        string TenantId { get; set; }
    }

    class TenantRoot
    {
        public string TenantRootId { get; set; } = Guid.NewGuid().ToString("N");
        public string TenantId { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = null!;
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

    // ── Self-contained DbContext ──────────────────────────────────────────────

    class TestDbContext : DbContext
    {
        public string? CurrentTenantId { get; set; }
        public string TenantIdOrThrow => CurrentTenantId ?? throw new InvalidOperationException("TenantId not set");

        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

        public DbSet<TenantRoot> TenantRoots => Set<TenantRoot>();
        public DbSet<Animal> Animals => Set<Animal>();
        public DbSet<Dog> Dogs => Set<Dog>();
        public DbSet<DogTag> DogTags => Set<DogTag>();
        public DbSet<Untenanted> Untenanted => Set<Untenanted>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseCoalesceMultiTenancy<ITenanted, string>(t => t.TenantId, () => TenantIdOrThrow);
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    static DbContextOptions<TestDbContext> BuildOptions(string? dbName = null) =>
        new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

    static TestDbContext OpenDb(DbContextOptions<TestDbContext> opts, string tenantId)
    {
        var db = new TestDbContext(opts);
        db.CurrentTenantId = tenantId;
        return db;
    }

    // ── Model structure tests ─────────────────────────────────────────────────

    [Test]
    public async Task TenantedEntity_PrimaryKey_IsComposite()
    {
        using var db = new TestDbContext(BuildOptions());
        var animalType = db.Model.FindEntityType(typeof(Animal))!;
        var pk = animalType.FindPrimaryKey()!;

        await Assert.That(pk.Properties.Count).IsEqualTo(2);
        await Assert.That(pk.Properties.Select(p => p.Name)).Contains("TenantId");
        await Assert.That(pk.Properties.Select(p => p.Name)).Contains("AnimalId");
        // TenantId must be first so rows are clustered per tenant
        await Assert.That(pk.Properties[0].Name).IsEqualTo("TenantId");
    }

    [Test]
    public async Task NonTenantedEntity_PrimaryKey_IsNotExpanded()
    {
        using var db = new TestDbContext(BuildOptions());
        var type = db.Model.FindEntityType(typeof(Untenanted))!;
        var pk = type.FindPrimaryKey()!;

        await Assert.That(pk.Properties.Count).IsEqualTo(1);
        await Assert.That(pk.Properties[0].Name).IsEqualTo("Id");
    }

    [Test]
    public async Task TenantedEntity_HasQueryFilter()
    {
        using var db = new TestDbContext(BuildOptions());
        var animalType = db.Model.FindEntityType(typeof(Animal))!;
#if NET10_0_OR_GREATER
        var filters = animalType.GetDeclaredQueryFilters().ToList();
        await Assert.That(filters.Count).IsGreaterThanOrEqualTo(1);
#else
        var filter = animalType.GetQueryFilter();
        await Assert.That(filter).IsNotNull();
#endif
    }

    [Test]
    public async Task DerivedTphType_PrimaryKey_InheritedFromBase()
    {
        using var db = new TestDbContext(BuildOptions());
        var dogType = db.Model.FindEntityType(typeof(Dog))!;
        var pk = dogType.FindPrimaryKey()!;

        // Dog shares Animal's composite PK
        await Assert.That(pk.Properties.Count).IsEqualTo(2);
        await Assert.That(pk.Properties.Select(p => p.Name)).Contains("TenantId");
    }

    [Test]
    public async Task DerivedTphType_ReferencingFk_HasTwoProperties_IncludingTenantId()
    {
        // DogTag.DogId → Dog.AnimalId must be rewired to
        // (DogTag.TenantId, DogTag.DogId) → (Dog.TenantId, Dog.AnimalId)
        // This only happens via the else-if branch for derived TPH types whose PK
        // was already composite when they were processed.
        using var db = new TestDbContext(BuildOptions());
        var dogTagType = db.Model.FindEntityType(typeof(DogTag))!;
        var fk = dogTagType.GetForeignKeys()
            .Single(fk => fk.PrincipalEntityType.ClrType == typeof(Dog)
                       || fk.PrincipalEntityType.ClrType == typeof(Animal));

        await Assert.That(fk.Properties.Count).IsEqualTo(2);
        await Assert.That(fk.Properties.Select(p => p.Name)).Contains("TenantId");
    }

    [Test]
    public async Task DerivedTphType_ReferencingFk_PropertyCountMatchesPrincipalKeyArity()
    {
        // Without the TPH fix the FK has 1 property but the PK has 2, which is an EF model error.
        using var db = new TestDbContext(BuildOptions());
        var dogTagType = db.Model.FindEntityType(typeof(DogTag))!;
        var fk = dogTagType.GetForeignKeys()
            .Single(fk => fk.PrincipalEntityType.ClrType == typeof(Dog)
                       || fk.PrincipalEntityType.ClrType == typeof(Animal));

        await Assert.That(fk.Properties.Count).IsEqualTo(fk.PrincipalKey.Properties.Count);
    }

    // ── Runtime behavior tests ────────────────────────────────────────────────

    [Test]
    public async Task AddedEntity_TenantId_IsSetByInterceptor()
    {
        const string tenantId = "tenant-abc";
        var opts = BuildOptions();

        using (var db = OpenDb(opts, tenantId))
        {
            db.Animals.Add(new Animal { Name = "Rex" });
            db.SaveChanges();
        }

        using (var db = OpenDb(opts, tenantId))
        {
            var animal = db.Animals.IgnoreQueryFilters().Single();
            await Assert.That(animal.TenantId).IsEqualTo(tenantId);
        }
    }

    [Test]
    public async Task QueryFilter_RestrictsResultsToCurrentTenant()
    {
        var opts = BuildOptions();
        const string t1 = "tenant-1";
        const string t2 = "tenant-2";

        using (var db = OpenDb(opts, t1))
        {
            db.Animals.Add(new Animal { Name = "Tenant1Dog" });
            db.SaveChanges();
        }
        using (var db = OpenDb(opts, t2))
        {
            db.Animals.Add(new Animal { Name = "Tenant2Dog" });
            db.SaveChanges();
        }

        using (var db = OpenDb(opts, t1))
        {
            var names = db.Animals.Select(a => a.Name).ToList();
            await Assert.That(names).Contains("Tenant1Dog");
            await Assert.That(names).DoesNotContain("Tenant2Dog");
        }
    }

    [Test]
    public async Task ModifyingTenantId_ThrowsInvalidOperation()
    {
        var opts = BuildOptions();
        const string t1 = "tenant-1";

        Animal saved;
        using (var db = OpenDb(opts, t1))
        {
            saved = new Animal { Name = "Rex" };
            db.Animals.Add(saved);
            db.SaveChanges();
        }

        using (var db = OpenDb(opts, t1))
        {
            var animal = db.Animals.IgnoreQueryFilters().Single();
            animal.TenantId = "other-tenant";

            await Assert.That(db.SaveChanges).Throws<InvalidOperationException>();
        }
    }

    [Test]
    public async Task SameTenant_DogTagReferencingDog_CanBeSaved()
    {
        var opts = BuildOptions();
        const string t1 = "tenant-1";

        string dogId;
        using (var db = OpenDb(opts, t1))
        {
            var dog = new Dog { Name = "Rex", Breed = "Labrador" };
            db.Animals.Add(dog);
            db.SaveChanges();
            dogId = dog.AnimalId;
        }

        using (var db = OpenDb(opts, t1))
        {
            db.DogTags.Add(new DogTag { DogId = dogId });
            db.SaveChanges(); // must not throw
        }

        using (var db = OpenDb(opts, t1))
        {
            await Assert.That(db.DogTags.Count()).IsEqualTo(1);
        }
    }
}
