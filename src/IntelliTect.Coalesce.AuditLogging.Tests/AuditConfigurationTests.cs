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
        // Arrange
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
        // Arrange
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
        // Arrange
        using var db = BuildDbContext(b => b
            .Format<AppUser>(["Name", "Title"], v => (v as string)?.Length.ToString())
        );

        db.Add(new AppUser { Name = "Bob", Title = "Builder" });
        db.SaveChanges();

        Assert.Equal("3", db.AuditLogProperties.Single(p => p.PropertyName == "Name").NewValue);
        Assert.Equal("7", db.AuditLogProperties.Single(p => p.PropertyName == "Title").NewValue);
    }

    [Fact]
    public void ExcludeGeneric()
    {
        // Arrange
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
        // Arrange
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
        // Arrange
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
        // Arrange
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
        // Arrange
        using var db = BuildDbContext(b => b
            .ExcludeProperty<AppUser>(u => new { u.Name, u.Title })
        );

        db.Add(new AppUser { Name = "Bob", Title = "Builder" });
        db.SaveChanges();

        Assert.Empty(db.AuditLogProperties.Where(p => p.PropertyName == "Name"));
        Assert.Empty(db.AuditLogProperties.Where(p => p.PropertyName == "Title"));
    }

    private TestDbContext BuildDbContext(Action<AuditConfiguration> setup)
    {
        var builder = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(SqliteConn);

        var db = new TestDbContext(builder
            .UseCoalesceAuditLogging<TestAuditLog>(x => x
                .ConfigureAudit(setup)
            ).Options);

        db.Database.EnsureCreated();

        return db;
    }
}
