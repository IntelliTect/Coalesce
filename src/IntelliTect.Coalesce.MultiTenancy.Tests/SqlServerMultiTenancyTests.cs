using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IntelliTect.Coalesce.MultiTenancy.Tests;

/// <summary>
/// Tests using SQL Server (LocalDB) — validates real relational behavior with composite PKs.
/// Skips gracefully if LocalDB is not available.
/// </summary>
[NotInParallel]
public class SqlServerMultiTenancyTests
{
    private static readonly string SqlServerConnString =
        $"Server=(localdb)\\MSSQLLocalDB;Database=CoalesceMultiTenancyTestsNet{Environment.Version.Major};Trusted_Connection=True;Timeout=5";

    private TestDbContext BuildDbContext(string tenantId)
    {
        var opts = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlServer(SqlServerConnString)
            .Options;

        var db = new TestDbContext(opts) { CurrentTenantId = tenantId };
        try
        {
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
        }
        catch (SqlException ex) when (
            ex.Number == 53
            || ex.Message.Contains("Could not open a connection to SQL Server")
            || ex.Message.Contains("The server was not found or was not accessible")
        )
        {
            TUnit.Core.Skip.Test(ex.Message);
        }
        catch (PlatformNotSupportedException ex) when (
            ex.Message.Contains("LocalDB is not supported on this platform")
        )
        {
            TUnit.Core.Skip.Test(ex.Message);
        }
        return db;
    }

    [Test]
    public async Task AddedEntity_TenantId_IsSetByInterceptor()
    {
        const string tenantId = "tenant-abc";
        using var db = BuildDbContext(tenantId);

        db.Animals.Add(new Animal { Name = "Rex" });
        await db.SaveChangesAsync();

        var animal = await db.Animals.IgnoreQueryFilters().SingleAsync();
        await Assert.That(animal.TenantId).IsEqualTo(tenantId);
    }

    [Test]
    public async Task QueryFilter_RestrictsResultsToCurrentTenant()
    {
        const string t1 = "tenant-1";
        using var db = BuildDbContext(t1);

        db.Animals.Add(new Animal { Name = "Tenant1Dog" });
        await db.SaveChangesAsync();

        // Insert directly with raw SQL to bypass interceptor for second tenant
        await db.Database.ExecuteSqlRawAsync(
            "INSERT INTO Animals (AnimalId, TenantId, Name, Discriminator) VALUES ({0}, {1}, {2}, {3})",
            Guid.NewGuid().ToString("N"), "tenant-2", "Tenant2Dog", "Animal");

        var names = await db.Animals.Select(a => a.Name).ToListAsync();
        await Assert.That(names).Contains("Tenant1Dog");
        await Assert.That(names).DoesNotContain("Tenant2Dog");
    }

    [Test]
    public async Task ModifyingTenantId_ThrowsInvalidOperation()
    {
        const string t1 = "tenant-1";
        using var db = BuildDbContext(t1);

        db.Animals.Add(new Animal { Name = "Rex" });
        await db.SaveChangesAsync();

        var animal = await db.Animals.SingleAsync();
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
        await db.SaveChangesAsync();

        db.DogTags.Add(new DogTag { DogId = dog.AnimalId });
        await db.SaveChangesAsync(); // must not throw

        await Assert.That(await db.DogTags.CountAsync()).IsEqualTo(1);
    }

    [Test]
    public async Task IntegerPk_Identity_WorksAcrossTenants()
    {
        const string t1 = "tenant-1";
        using var db = BuildDbContext(t1);

        db.TenantedWithIntPks.Add(new TenantedWithIntPk { Value = "A" });
        await db.SaveChangesAsync();

        // Insert for second tenant via raw SQL
        await db.Database.ExecuteSqlRawAsync(
            "INSERT INTO TenantedWithIntPks (TenantId, Value) VALUES ({0}, {1})",
            "tenant-2", "B");

        var all = await db.TenantedWithIntPks.IgnoreQueryFilters().ToListAsync();
        await Assert.That(all.Count).IsEqualTo(2);
        await Assert.That(all.All(x => x.Id > 0)).IsTrue();
    }

    [Test]
    public async Task IntegerPk_Identity_IsAutoAssignedByDatabase()
    {
        const string t1 = "tenant-1";
        using var db = BuildDbContext(t1);

        // Insert via EF without setting Id — relies on ValueGenerated.OnAdd being preserved
        var entity = new TenantedWithIntPk { Value = "AutoId" };
        db.TenantedWithIntPks.Add(entity);
        await db.SaveChangesAsync();

        await Assert.That(entity.Id).IsGreaterThan(0);
    }

    // ── Cross-tenant FK violation ─────────────────────────────────────────────

    [Test]
    public async Task CrossTenantFk_ThrowsOnSave()
    {
        const string t1 = "tenant-1";
        using var db = BuildDbContext(t1);

        var dog = new Dog { Name = "Rex", Breed = "Labrador" };
        db.Animals.Add(dog);
        await db.SaveChangesAsync();

        // Insert a DogTag via raw SQL with a different TenantId but same DogId.
        // The composite FK (TenantId, DogId) won't match → SQL Server FK violation.
        var ex = await Assert.ThrowsAsync<SqlException>(
            async () => await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO DogTags (DogTagId, TenantId, DogId) VALUES ({0}, {1}, {2})",
                Guid.NewGuid().ToString("N"), "tenant-2", dog.AnimalId));
        await Assert.That(ex!.Message).Contains("FOREIGN KEY constraint");
    }

    // ── Self-referencing FK ───────────────────────────────────────────────────

    [Test]
    public async Task SelfReferencingFk_SameTenant_CanBeSaved()
    {
        const string t1 = "tenant-1";
        using var db = BuildDbContext(t1);

        var parent = new Category { Name = "Electronics" };
        db.Categories.Add(parent);
        await db.SaveChangesAsync();

        var child = new Category { Name = "Phones", ParentCategoryId = parent.CategoryId };
        db.Categories.Add(child);
        await db.SaveChangesAsync(); // must not throw

        await Assert.That(await db.Categories.CountAsync()).IsEqualTo(2);
    }

    [Test]
    public async Task SelfReferencingFk_CrossTenant_ThrowsOnSave()
    {
        const string t1 = "tenant-1";
        using var db = BuildDbContext(t1);

        var parent = new Category { Name = "Electronics" };
        db.Categories.Add(parent);
        await db.SaveChangesAsync();

        // Insert a child in a different tenant referencing the parent → FK violation
        var ex = await Assert.ThrowsAsync<SqlException>(
            async () => await db.Database.ExecuteSqlRawAsync(
                "INSERT INTO Categories (CategoryId, TenantId, Name, ParentCategoryId) VALUES ({0}, {1}, {2}, {3})",
                Guid.NewGuid().ToString("N"), "tenant-2", "Phones", parent.CategoryId));
        await Assert.That(ex!.Message).Contains("FOREIGN KEY");
    }
}
