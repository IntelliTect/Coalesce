using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IntelliTect.Coalesce.MultiTenancy.Tests;

/// <summary>
/// Model metadata tests using the SQL Server provider.
/// These only inspect the EF model and never connect to a database.
/// </summary>
public class SqlServerModelTests
{
    private static TestDbContext BuildModelContext()
    {
        // Connection string is irrelevant — we never open a connection, just build the model.
        var opts = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlServer("Server=.;Database=Unused;Trusted_Connection=True")
            .Options;

        return new TestDbContext(opts) { CurrentTenantId = "unused" };
    }

    [Test]
    public async Task TenantedEntity_PrimaryKey_IsComposite()
    {
        using var db = BuildModelContext();
        var animalType = db.Model.FindEntityType(typeof(Animal))!;
        var pk = animalType.FindPrimaryKey()!;

        await Assert.That(pk.Properties.Count).IsEqualTo(2);
        await Assert.That(pk.Properties[0].Name).IsEqualTo("TenantId");
        await Assert.That(pk.Properties[1].Name).IsEqualTo("AnimalId");
    }

    [Test]
    public async Task IntegerPk_UsesCompositePk()
    {
        // SQL Server supports composite PKs with IDENTITY, so no alternate key workaround needed.
        using var db = BuildModelContext();
        var entityType = db.Model.FindEntityType(typeof(TenantedWithIntPk))!;
        var pk = entityType.FindPrimaryKey()!;

        await Assert.That(pk.Properties.Count).IsEqualTo(2);
        await Assert.That(pk.Properties[0].Name).IsEqualTo("TenantId");
        await Assert.That(pk.Properties[1].Name).IsEqualTo("Id");
    }

    [Test]
    public async Task IntegerPk_PreservesValueGeneratedOnAdd()
    {
        using var db = BuildModelContext();
        var entityType = db.Model.FindEntityType(typeof(TenantedWithIntPk))!;
        var idProp = entityType.FindProperty("Id")!;

        // After PK expansion, the Id column must still be marked as store-generated
        await Assert.That(idProp.ValueGenerated).IsEqualTo(Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAdd);
    }
}
