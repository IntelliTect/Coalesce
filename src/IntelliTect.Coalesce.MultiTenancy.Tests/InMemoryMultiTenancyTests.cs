using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace IntelliTect.Coalesce.MultiTenancy.Tests;

/// <summary>
/// Tests using the InMemory provider — validates model structure and interceptor behavior.
/// </summary>
public class InMemoryMultiTenancyTests
{
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

        using (var db = OpenDb(opts, t1))
        {
            db.Animals.Add(new Animal { Name = "Rex" });
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

    // ── Null/throwing TenantId ────────────────────────────────────────────────

    [Test]
    public async Task AddedEntity_WhenTenantIdNotSet_ThrowsFromContextProperty()
    {
        var opts = BuildOptions();
        // CurrentTenantId is null, so TenantIdOrThrow will throw
        using var db = new TestDbContext(opts);

        var ex = Assert.Throws<InvalidOperationException>(() => db.Animals.Add(new Animal { Name = "Rex" }));
        await Assert.That(ex.Message).Contains("TenantId");
    }

    // ── Multiple entities in one SaveChanges ──────────────────────────────────

    [Test]
    public async Task MultipleEntities_AllGetTenantIdSet()
    {
        const string tenantId = "tenant-bulk";
        var opts = BuildOptions();

        using (var db = OpenDb(opts, tenantId))
        {
            db.Animals.Add(new Animal { Name = "A" });
            db.Animals.Add(new Animal { Name = "B" });
            db.Animals.Add(new Animal { Name = "C" });
            db.SaveChanges();
        }

        using (var db = OpenDb(opts, tenantId))
        {
            var animals = db.Animals.IgnoreQueryFilters().ToList();
            await Assert.That(animals.Count).IsEqualTo(3);
            await Assert.That(animals.All(a => a.TenantId == tenantId)).IsTrue();
        }
    }

    // ── Mixed tenanted + non-tenanted in one SaveChanges ──────────────────────

    [Test]
    public async Task MixedSave_NonTenantedEntityUnaffected()
    {
        const string tenantId = "tenant-mixed";
        var opts = BuildOptions();

        using (var db = OpenDb(opts, tenantId))
        {
            db.Animals.Add(new Animal { Name = "Rex" });
            db.Set<Untenanted>().Add(new Untenanted { Value = "hello" });
            db.SaveChanges();
        }

        using (var db = OpenDb(opts, tenantId))
        {
            await Assert.That(db.Animals.IgnoreQueryFilters().Count()).IsEqualTo(1);
            await Assert.That(db.Set<Untenanted>().Count()).IsEqualTo(1);
        }
    }

    // ── IgnoreQueryFilters bypasses tenancy ───────────────────────────────────

    [Test]
    public async Task IgnoreQueryFilters_ShowsAllTenants()
    {
        var opts = BuildOptions();
        const string t1 = "tenant-1";
        const string t2 = "tenant-2";

        using (var db = OpenDb(opts, t1))
        {
            db.Animals.Add(new Animal { Name = "T1Dog" });
            db.SaveChanges();
        }
        using (var db = OpenDb(opts, t2))
        {
            db.Animals.Add(new Animal { Name = "T2Dog" });
            db.SaveChanges();
        }

        using (var db = OpenDb(opts, t1))
        {
            var all = db.Animals.IgnoreQueryFilters().Select(a => a.Name).ToList();
            await Assert.That(all).Contains("T1Dog");
            await Assert.That(all).Contains("T2Dog");
        }
    }

    // ── Named query filter (NET10+) ──────────────────────────────────────────

#if NET10_0_OR_GREATER
    [Test]
    public async Task NamedQueryFilter_CanBeSelectivelyIgnored()
    {
        var opts = BuildOptions();
        const string t1 = "tenant-1";
        const string t2 = "tenant-2";

        using (var db = OpenDb(opts, t1))
        {
            db.Animals.Add(new Animal { Name = "T1Dog" });
            db.SaveChanges();
        }
        using (var db = OpenDb(opts, t2))
        {
            db.Animals.Add(new Animal { Name = "T2Dog" });
            db.SaveChanges();
        }

        using (var db = OpenDb(opts, t1))
        {
            var all = db.Animals
                .IgnoreTenantFilter()
                .Select(a => a.Name).ToList();
            await Assert.That(all).Contains("T1Dog");
            await Assert.That(all).Contains("T2Dog");
        }
    }
#endif

    // ── LambdaExpression overload ─────────────────────────────────────────────

    [Test]
    public async Task LambdaExpressionOverload_WorksIdenticallyToStringOverload()
    {
        Expression<Func<TestDbContext, string>> contextPropExpr = db => db.TenantIdOrThrow;

        var builder = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());
        builder.UseCoalesceMultiTenancy<ITenanted>(t => t.TenantId, contextPropExpr);
        var opts = builder.Options;

        const string tenantId = "tenant-lambda";
        using var db = new TestDbContext(opts) { CurrentTenantId = tenantId };

        db.Animals.Add(new Animal { Name = "Lambda" });
        db.SaveChanges();

        var animal = db.Animals.IgnoreQueryFilters().Single();
        await Assert.That(animal.TenantId).IsEqualTo(tenantId);
    }

    // ── Async SaveChanges ─────────────────────────────────────────────────────

    [Test]
    public async Task AsyncSaveChanges_InterceptorSetsTenantId()
    {
        const string tenantId = "tenant-async";
        var opts = BuildOptions();

        using (var db = OpenDb(opts, tenantId))
        {
            db.Animals.Add(new Animal { Name = "AsyncDog" });
            await db.SaveChangesAsync();
        }

        using (var db = OpenDb(opts, tenantId))
        {
            var animal = db.Animals.IgnoreQueryFilters().Single();
            await Assert.That(animal.TenantId).IsEqualTo(tenantId);
        }
    }

    [Test]
    public async Task AsyncSaveChanges_ModifyingTenantId_Throws()
    {
        var opts = BuildOptions();
        const string t1 = "tenant-1";

        using (var db = OpenDb(opts, t1))
        {
            db.Animals.Add(new Animal { Name = "Rex" });
            await db.SaveChangesAsync();
        }

        using (var db = OpenDb(opts, t1))
        {
            var animal = db.Animals.IgnoreQueryFilters().Single();
            animal.TenantId = "other-tenant";

            await Assert.That(() => db.SaveChangesAsync()).Throws<InvalidOperationException>();
        }
    }

    // ── Entity not implementing ITenanted is not configured ───────────────────

    [Test]
    public async Task EntityWithTenantId_NotImplementingITenanted_IsNotConfigured()
    {
        using var db = new TestDbContext(BuildOptions());
        var entityType = db.Model.FindEntityType(typeof(HasTenantIdButNotITenanted))!;
        var pk = entityType.FindPrimaryKey()!;

        // Should have its original single-column PK, not expanded
        await Assert.That(pk.Properties.Count).IsEqualTo(1);
        await Assert.That(pk.Properties[0].Name).IsEqualTo("Id");

#if NET10_0_OR_GREATER
        var filters = entityType.GetDeclaredQueryFilters().ToList();
        await Assert.That(filters.Count).IsEqualTo(0);
#else
        var filter = entityType.GetQueryFilter();
        await Assert.That(filter).IsNull();
#endif
    }

    // ── Self-referencing FK model structure ───────────────────────────────────

    [Test]
    public async Task SelfReferencingFk_IsRewiredToCompositeKey()
    {
        using var db = new TestDbContext(BuildOptions());
        var categoryType = db.Model.FindEntityType(typeof(Category))!;
        var fk = categoryType.GetForeignKeys()
            .Single(fk => fk.PrincipalEntityType.ClrType == typeof(Category));

        // FK should be (TenantId, ParentCategoryId) → (TenantId, CategoryId)
        await Assert.That(fk.Properties.Count).IsEqualTo(2);
        await Assert.That(fk.Properties.Select(p => p.Name)).Contains("TenantId");
        await Assert.That(fk.Properties.Select(p => p.Name)).Contains("ParentCategoryId");
    }

    // ── Explicit composite PK already includes TenantId ──────────────────────

    [Test]
    public async Task ExplicitCompositeKey_WithTenantId_IsNotDoubleExpanded()
    {
        using var db = new TestDbContext(BuildOptions());
        var entityType = db.Model.FindEntityType(typeof(AlreadyCompositeKey))!;
        var pk = entityType.FindPrimaryKey()!;

        // Should remain as configured: (TenantId, Key1, Key2) — not expanded further
        await Assert.That(pk.Properties.Count).IsEqualTo(3);
        await Assert.That(pk.Properties.Select(p => p.Name)).Contains("TenantId");
        await Assert.That(pk.Properties.Select(p => p.Name)).Contains("Key1");
        await Assert.That(pk.Properties.Select(p => p.Name)).Contains("Key2");
    }

    // ── TenantId is the sole PK ──────────────────────────────────────────────

    [Test]
    public async Task TenantIdIsPk_IsNotExpanded()
    {
        using var db = new TestDbContext(BuildOptions());
        var entityType = db.Model.FindEntityType(typeof(TenantIdIsPk))!;
        var pk = entityType.FindPrimaryKey()!;

        // TenantId is already the PK — should not try to expand
        await Assert.That(pk.Properties.Count).IsEqualTo(1);
        await Assert.That(pk.Properties[0].Name).IsEqualTo("TenantId");
    }

    // ── Multiple FKs to same principal ───────────────────────────────────────

    [Test]
    public async Task MultipleFksToSamePrincipal_AllRewired()
    {
        using var db = new TestDbContext(BuildOptions());
        var entityType = db.Model.FindEntityType(typeof(AnimalRecord))!;
        var fks = entityType.GetForeignKeys()
            .Where(fk => fk.PrincipalEntityType.ClrType == typeof(Animal))
            .ToList();

        await Assert.That(fks.Count).IsEqualTo(2);
        foreach (var fk in fks)
        {
            await Assert.That(fk.Properties.Count).IsEqualTo(2);
            await Assert.That(fk.Properties.Select(p => p.Name)).Contains("TenantId");
        }
    }

    // ── Optional (nullable) FK ───────────────────────────────────────────────

    [Test]
    public async Task OptionalFk_IsRewiredToCompositeKey()
    {
        using var db = new TestDbContext(BuildOptions());
        var entityType = db.Model.FindEntityType(typeof(OptionalRef))!;
        var fk = entityType.GetForeignKeys()
            .Single(fk => fk.PrincipalEntityType.ClrType == typeof(Animal));

        // FK should be (TenantId, AnimalId) — even though AnimalId is nullable
        await Assert.That(fk.Properties.Count).IsEqualTo(2);
        await Assert.That(fk.Properties.Select(p => p.Name)).Contains("TenantId");
        await Assert.That(fk.Properties.Select(p => p.Name)).Contains("AnimalId");
    }

    [Test]
    public async Task OptionalFk_CanSaveWithNullReference()
    {
        const string tenantId = "tenant-opt";
        var opts = BuildOptions();

        using (var db = OpenDb(opts, tenantId))
        {
            db.Set<OptionalRef>().Add(new OptionalRef { AnimalId = null });
            db.SaveChanges();
        }

        using (var db = OpenDb(opts, tenantId))
        {
            var item = db.Set<OptionalRef>().IgnoreQueryFilters().Single();
            await Assert.That(item.AnimalId).IsNull();
            await Assert.That(item.TenantId).IsEqualTo(tenantId);
        }
    }
}
