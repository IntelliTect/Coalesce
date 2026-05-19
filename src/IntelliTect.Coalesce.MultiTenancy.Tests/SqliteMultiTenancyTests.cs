using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace IntelliTect.Coalesce.MultiTenancy.Tests;

/// <summary>
/// Tests using SQLite — validates real database behavior including the AUTOINCREMENT workaround.
/// </summary>
public class SqliteMultiTenancyTests : IDisposable
{
    private readonly SqliteConnection _connection;

    public SqliteMultiTenancyTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();
    }

    public void Dispose() => _connection.Dispose();

    private TestDbContext BuildDbContext(string tenantId)
    {
        var opts = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(_connection)
            .Options;

        var db = new TestDbContext(opts) { CurrentTenantId = tenantId };
        db.Database.EnsureCreated();
        return db;
    }

    [Test]
    public async Task IntegerPk_UsesAlternateKey_InsteadOfCompositePk()
    {
        // SQLite does not support composite PKs with AUTOINCREMENT.
        // The convention should keep the single-column PK and add a composite alternate key.
        using var db = BuildDbContext("t1");
        var entityType = db.Model.FindEntityType(typeof(TenantedWithIntPk))!;

        var pk = entityType.FindPrimaryKey()!;
        // PK should remain single-column (Id) for AUTOINCREMENT support
        await Assert.That(pk.Properties.Count).IsEqualTo(1);
        await Assert.That(pk.Properties[0].Name).IsEqualTo("Id");

        // There should be a composite alternate key (TenantId, Id)
        var keys = entityType.GetKeys().ToList();
        var alternateKey = keys.FirstOrDefault(k =>
            k != pk && k.Properties.Count == 2
            && k.Properties.Any(p => p.Name == "TenantId")
            && k.Properties.Any(p => p.Name == "Id"));
        await Assert.That(alternateKey).IsNotNull();
    }

    [Test]
    public async Task StringPk_UsesCompositePk()
    {
        // String PKs don't need AUTOINCREMENT, so they should get a normal composite PK.
        using var db = BuildDbContext("t1");
        var entityType = db.Model.FindEntityType(typeof(Animal))!;
        var pk = entityType.FindPrimaryKey()!;

        await Assert.That(pk.Properties.Count).IsEqualTo(2);
        await Assert.That(pk.Properties[0].Name).IsEqualTo("TenantId");
        await Assert.That(pk.Properties[1].Name).IsEqualTo("AnimalId");
    }

    [Test]
    public async Task AddedEntity_TenantId_IsSetByInterceptor()
    {
        const string tenantId = "tenant-abc";
        using var db = BuildDbContext(tenantId);

        db.Animals.Add(new Animal { Name = "Rex" });
        db.SaveChanges();

        var animal = db.Animals.IgnoreQueryFilters().Single();
        await Assert.That(animal.TenantId).IsEqualTo(tenantId);
    }

    [Test]
    public async Task QueryFilter_RestrictsResultsToCurrentTenant()
    {
        const string t1 = "tenant-1";
        const string t2 = "tenant-2";

        using var db = BuildDbContext(t1);

        db.Animals.Add(new Animal { Name = "Tenant1Dog" });
        db.SaveChanges();

        db.CurrentTenantId = t2;
        db.Animals.Add(new Animal { Name = "Tenant2Dog" });
        db.SaveChanges();

        db.CurrentTenantId = t1;
        var names = db.Animals.Select(a => a.Name).ToList();
        await Assert.That(names).Contains("Tenant1Dog");
        await Assert.That(names).DoesNotContain("Tenant2Dog");
    }

    [Test]
    public async Task ModifyingTenantId_ThrowsInvalidOperation()
    {
        const string t1 = "tenant-1";
        using var db = BuildDbContext(t1);

        db.Animals.Add(new Animal { Name = "Rex" });
        db.SaveChanges();

        var animal = db.Animals.IgnoreQueryFilters().Single();
        animal.TenantId = "other-tenant";

        await Assert.That(db.SaveChanges).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task DogTag_ForeignKey_WorksWithCompositePrincipalKey()
    {
        const string t1 = "tenant-1";
        using var db = BuildDbContext(t1);

        var dog = new Dog { Name = "Rex", Breed = "Labrador" };
        db.Animals.Add(dog);
        db.SaveChanges();

        db.DogTags.Add(new DogTag { DogId = dog.AnimalId });
        db.SaveChanges(); // must not throw

        await Assert.That(db.DogTags.Count()).IsEqualTo(1);
    }

    [Test]
    public async Task IntegerPk_AutoIncrement_WorksAcrossTenants()
    {
        const string t1 = "tenant-1";
        const string t2 = "tenant-2";
        using var db = BuildDbContext(t1);

        db.TenantedWithIntPks.Add(new TenantedWithIntPk { Value = "A" });
        db.SaveChanges();

        db.CurrentTenantId = t2;
        db.TenantedWithIntPks.Add(new TenantedWithIntPk { Value = "B" });
        db.SaveChanges();

        var all = db.TenantedWithIntPks.IgnoreQueryFilters().ToList();
        await Assert.That(all.Count).IsEqualTo(2);
        // Both should have auto-generated IDs
        await Assert.That(all.All(x => x.Id > 0)).IsTrue();
    }

    // ── Cross-tenant FK violation ─────────────────────────────────────────────

    [Test]
    public async Task CrossTenantFk_ThrowsOnSave()
    {
        // Insert a dog in tenant-1, then try to reference it from a DogTag in tenant-2.
        // The composite FK (TenantId, DogId) won't match any row in the principal → DB error.
        const string t1 = "tenant-1";
        const string t2 = "tenant-2";
        using var db = BuildDbContext(t1);

        var dog = new Dog { Name = "Rex", Breed = "Labrador" };
        db.Animals.Add(dog);
        db.SaveChanges();

        db.CurrentTenantId = t2;
        db.DogTags.Add(new DogTag { DogId = dog.AnimalId });

        // SQLite enforces FK constraints — the composite FK (t2, dogId) won't match (t1, dogId)
        var ex = Assert.Throws<DbUpdateException>(() => db.SaveChanges());
        await Assert.That(ex.InnerException!.Message).Contains("FOREIGN KEY constraint failed");
    }

    // ── Self-referencing FK ───────────────────────────────────────────────────

    [Test]
    public async Task SelfReferencingFk_SameTenant_CanBeSaved()
    {
        const string t1 = "tenant-1";
        using var db = BuildDbContext(t1);

        var parent = new Category { Name = "Electronics" };
        db.Categories.Add(parent);
        db.SaveChanges();

        var child = new Category { Name = "Phones", ParentCategoryId = parent.CategoryId };
        db.Categories.Add(child);
        db.SaveChanges(); // must not throw

        await Assert.That(db.Categories.Count()).IsEqualTo(2);
    }

    [Test]
    public async Task SelfReferencingFk_CrossTenant_ThrowsOnSave()
    {
        const string t1 = "tenant-1";
        const string t2 = "tenant-2";
        using var db = BuildDbContext(t1);

        var parent = new Category { Name = "Electronics" };
        db.Categories.Add(parent);
        db.SaveChanges();

        db.CurrentTenantId = t2;
        var child = new Category { Name = "Phones", ParentCategoryId = parent.CategoryId };
        db.Categories.Add(child);

        // FK (t2, parentId) won't match (t1, parentId) in the principal
        var ex = Assert.Throws<DbUpdateException>(() => db.SaveChanges());
        await Assert.That(ex.InnerException!.Message).Contains("FOREIGN KEY constraint failed");
    }
}
