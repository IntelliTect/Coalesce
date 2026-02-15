using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.AuditLogging.Tests;

public class AuditConfigurationTests : IDisposable
{
    public SqliteConnection SqliteConn { get; }

    public AuditConfigurationTests()
    {
        SqliteConn = new SqliteConnection("Data Source=:memory:");
        SqliteConn.Open();
    }

    public void Dispose()
    {
        SqliteConn?.Dispose();
    }

    [Test]
    public async Task FormatByExpressionSingle()
    {
        using var db = BuildDbContext(b => b
            .Format<AppUser>(x => x.Name, v => (v as string)?.Length.ToString())
        );

        db.Add(new AppUser { Name = "Bob", Title = "Builder" });
        db.SaveChanges();

        await Assert.That(db.AuditLogProperties.Single(p => p.PropertyName == "Name").NewValue).IsEqualTo("3");
        await Assert.That(db.AuditLogProperties.Single(p => p.PropertyName == "Title").NewValue).IsEqualTo("Builder");
    }

    [Test]
    public async Task FormatByExpressionMultiple()
    {
        using var db = BuildDbContext(b => b
            .Format<AppUser>(x => new { x.Name, x.Title }, v => (v as string)?.Length.ToString())
        );

        db.Add(new AppUser { Name = "Bob", Title = "Builder" });
        db.SaveChanges();

        await Assert.That(db.AuditLogProperties.Single(p => p.PropertyName == "Name").NewValue).IsEqualTo("3");
        await Assert.That(db.AuditLogProperties.Single(p => p.PropertyName == "Title").NewValue).IsEqualTo("7");
    }

    [Test]
    public async Task FormatByName()
    {
        using var db = BuildDbContext(b => b
            .Format<AppUser>(["Name", "Title"], v => (v as string)?.Length.ToString())
        );

        db.Add(new AppUser { Name = "Bob", Title = "Builder" });
        db.SaveChanges();

        await Assert.That(db.AuditLogProperties.Single(p => p.PropertyName == "Name").NewValue).IsEqualTo("3");
        await Assert.That(db.AuditLogProperties.Single(p => p.PropertyName == "Title").NewValue).IsEqualTo("7");
    }

    [Test]
    public async Task FormatType()
    {
        using var db = BuildDbContext(b => b
            .FormatType<string>(v => v.Length.ToString())
            .FormatType<DateTimeOffset>(v => v.ToString("O"))
        );

        db.Add(new AppUser { Name = "Bob", NullableValueType = DateTimeOffset.UnixEpoch });
        db.SaveChanges();

        await Assert.That(db.AuditLogProperties.Single(p => p.PropertyName == "Name").NewValue).IsEqualTo("3");
        await Assert.That(db.AuditLogProperties.Single(p => p.PropertyName == "NullableValueType").NewValue).IsEqualTo("1970-01-01T00:00:00.0000000+00:00");
    }

    [Test]
    public async Task ExcludeGeneric()
    {
        using var db = BuildDbContext(b => b
            .Exclude<AppUser>()
        );

        db.Add(new AppUser { Name = "Bob" });
        db.SaveChanges();

        await Assert.That(db.AuditLogs).IsEmpty();
    }

    [Test]
    public async Task ExcludeEntry()
    {
        using var db = BuildDbContext(b => b
            .Exclude(entry => entry.Entity is AppUser)
        );

        db.Add(new AppUser { Name = "Bob" });
        db.SaveChanges();

        await Assert.That(db.AuditLogs).IsEmpty();
    }

    [Test]
    public async Task ExcludePropertyByName()
    {
        using var db = BuildDbContext(b => b
            .ExcludeProperty<AppUser>("Name")
        );

        db.Add(new AppUser { Name = "Bob", Title = "Builder" });
        db.SaveChanges();

        await Assert.That(db.AuditLogProperties.Where(p => p.PropertyName == "Name")).IsEmpty();
        await Assert.That(db.AuditLogProperties.Single(p => p.PropertyName == "Title").NewValue).IsEqualTo("Builder");
    }

    [Test]
    public async Task ExcludePropertyByExpressionSingle()
    {
        using var db = BuildDbContext(b => b
            .ExcludeProperty<AppUser>(u => u.Name)
        );

        db.Add(new AppUser { Name = "Bob", Title = "Builder" });
        db.SaveChanges();

        await Assert.That(db.AuditLogProperties.Where(p => p.PropertyName == "Name")).IsEmpty();
        await Assert.That(db.AuditLogProperties.Where(p => p.PropertyName == "Title")).IsNotEmpty();
    }

    [Test]
    public async Task ExcludePropertyByExpressionMultiple()
    {
        using var db = BuildDbContext(b => b
            .ExcludeProperty<AppUser>(u => new { u.Name, u.Title })
        );

        db.Add(new AppUser { Name = "Bob", Title = "Builder" });
        db.SaveChanges();

        await Assert.That(db.AuditLogProperties.Where(p => p.PropertyName == "Name")).IsEmpty();
        await Assert.That(db.AuditLogProperties.Where(p => p.PropertyName == "Title")).IsEmpty();
    }

    [Test]
    public async Task AllowList()
    {
        using var db = BuildDbContext(b => b
            .Exclude(x => true)
            .Include<AppUser>()
        );

        db.Add(new AppUser { Name = "Bob", Title = "Builder" });
        db.Add(new ParentWithMappedListText { CustomListTextField = "Foo" });
        db.SaveChanges();

        var log = await Assert.That(db.AuditLogs).HasSingleItem();
        await Assert.That(log.Type).IsEqualTo(nameof(AppUser));
    }

    [Test]
    [Arguments(true, 1)]
    [Arguments(false, 0)]
    public async Task IncludeAddedDefaultValues(bool include, int expectedCount)
    {
        using var db = BuildDbContext(
            (b, include) => b.IncludeAddedDefaultValues(include),
            include
        );

        db.Add(new AppUser { Name = "Bob" });
        db.SaveChanges();

        await Assert.That(db.AuditLogProperties.Count(p => p.PropertyName == nameof(AppUser.BoolProp))).IsEqualTo(expectedCount);
    }

    private TestDbContext BuildDbContext(Action<AuditConfiguration> setup)
        => BuildDbContext((c, setup) => setup(c), setup);

    private TestDbContext BuildDbContext<TArg>(Action<AuditConfiguration, TArg> setup, TArg arg)
    {
        var builder = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(SqliteConn);

        var db = new TestDbContext(builder
            .UseCoalesceAuditLogging<TestAuditLog>(x => x
                .ConfigureAudit(setup, arg)
            ).Options);

        db.Database.EnsureCreated();

        return db;
    }
}
