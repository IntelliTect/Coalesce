using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.AuditLogging.Tests;

public class AuditConfigurationTests
{
    public SqliteConnection SqliteConn { get; }

    public AuditConfigurationTests()
    {
        SqliteConn = new SqliteConnection("Data Source=:memory:");
        SqliteConn.Open();
    }

    [Fact]
    public void FormatByExpressionSingle()
    {
        using var db = BuildDbContext(b => b
            .Format<AppUser>(x => x.Name, v => (v as string)?.Length.ToString())
        );

        db.Add(new AppUser { Name = "Bob", Title = "Builder" });
        db.SaveChanges();

        Assert.Equal("3", db.AuditLogProperties.Single(p => p.PropertyName == "Name").NewValue);
        Assert.Equal("Builder", db.AuditLogProperties.Single(p => p.PropertyName == "Title").NewValue);
    }

    [Fact]
    public void FormatByExpressionMultiple()
    {
        using var db = BuildDbContext(b => b
            .Format<AppUser>(x => new { x.Name, x.Title }, v => (v as string)?.Length.ToString())
        );

        db.Add(new AppUser { Name = "Bob", Title = "Builder" });
        db.SaveChanges();

        Assert.Equal("3", db.AuditLogProperties.Single(p => p.PropertyName == "Name").NewValue);
        Assert.Equal("7", db.AuditLogProperties.Single(p => p.PropertyName == "Title").NewValue);
    }

    [Fact]
    public void FormatByName()
    {
        using var db = BuildDbContext(b => b
            .Format<AppUser>(["Name", "Title"], v => (v as string)?.Length.ToString())
        );

        db.Add(new AppUser { Name = "Bob", Title = "Builder" });
        db.SaveChanges();

        Assert.Equal("3", db.AuditLogProperties.Single(p => p.PropertyName == "Name").NewValue);
        Assert.Equal("7", db.AuditLogProperties.Single(p => p.PropertyName == "Title").NewValue);
    }

    [Fact]
    public void FormatType()
    {
        using var db = BuildDbContext(b => b
            .FormatType<string>(v => v.Length.ToString())
            .FormatType<DateTimeOffset>(v => v.ToString("O"))
        );

        db.Add(new AppUser { Name = "Bob", NullableValueType = DateTimeOffset.UnixEpoch });
        db.SaveChanges();

        Assert.Equal("3", db.AuditLogProperties.Single(p => p.PropertyName == "Name").NewValue);
        Assert.Equal("1970-01-01T00:00:00.0000000+00:00", db.AuditLogProperties.Single(p => p.PropertyName == "NullableValueType").NewValue);
    }

    [Fact]
    public void ExcludeGeneric()
    {
        using var db = BuildDbContext(b => b
            .Exclude<AppUser>()
        );

        db.Add(new AppUser { Name = "Bob" });
        db.SaveChanges();

        Assert.Empty(db.AuditLogs);
    }

    [Fact]
    public void ExcludeEntry()
    {
        using var db = BuildDbContext(b => b
            .Exclude(entry => entry.Entity is AppUser)
        );

        db.Add(new AppUser { Name = "Bob" });
        db.SaveChanges();

        Assert.Empty(db.AuditLogs);
    }

    [Fact]
    public void ExcludePropertyByName()
    {
        using var db = BuildDbContext(b => b
            .ExcludeProperty<AppUser>("Name")
        );

        db.Add(new AppUser { Name = "Bob", Title = "Builder" });
        db.SaveChanges();

        Assert.Empty(db.AuditLogProperties.Where(p => p.PropertyName == "Name"));
        Assert.Equal("Builder", db.AuditLogProperties.Single(p => p.PropertyName == "Title").NewValue);
    }

    [Fact]
    public void ExcludePropertyByExpressionSingle()
    {
        using var db = BuildDbContext(b => b
            .ExcludeProperty<AppUser>(u => u.Name)
        );

        db.Add(new AppUser { Name = "Bob", Title = "Builder" });
        db.SaveChanges();

        Assert.Empty(db.AuditLogProperties.Where(p => p.PropertyName == "Name"));
        Assert.NotEmpty(db.AuditLogProperties.Where(p => p.PropertyName == "Title"));
    }

    [Fact]
    public void ExcludePropertyByExpressionMultiple()
    {
        using var db = BuildDbContext(b => b
            .ExcludeProperty<AppUser>(u => new { u.Name, u.Title })
        );

        db.Add(new AppUser { Name = "Bob", Title = "Builder" });
        db.SaveChanges();

        Assert.Empty(db.AuditLogProperties.Where(p => p.PropertyName == "Name"));
        Assert.Empty(db.AuditLogProperties.Where(p => p.PropertyName == "Title"));
    }

    [Fact]
    public void AllowList()
    {
        using var db = BuildDbContext(b => b
            .Exclude(x => true)
            .Include<AppUser>()
        );

        db.Add(new AppUser { Name = "Bob", Title = "Builder" });
        db.Add(new ParentWithMappedListText { CustomListTextField = "Foo" });
        db.SaveChanges();

        var log = Assert.Single(db.AuditLogs);
        Assert.Equal(nameof(AppUser), log.Type);
    }

    [Theory]
    [InlineData(true, 1)]
    [InlineData(false, 0)]
    public void IncludeAddedDefaultValues(bool include, int expectedCount)
    {
        using var db = BuildDbContext(
            (b, include) => b.IncludeAddedDefaultValues(include),
            include
        );

        db.Add(new AppUser { Name = "Bob" });
        db.SaveChanges();

        Assert.Equal(expectedCount, db.AuditLogProperties.Count(p => p.PropertyName == nameof(AppUser.BoolProp)));
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
