using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace IntelliTect.Coalesce.AuditLogging.Tests;

public class EntityStampingTests
{
    public AppDbContext Db { get; }

    class TestService
    {
        public string TestValue => "42";
    }

    public EntityStampingTests()
    {
        var appSp = new ServiceCollection()
            .AddSingleton<TestService>()
            .BuildServiceProvider();

        Db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .UseApplicationServiceProvider(appSp)
            .Options
        );
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WithCustomApplicationService_SetsStampValue(bool async)
    {
        var appSp = new ServiceCollection()
            .AddSingleton<TestService>()
            .BuildServiceProvider();

        // Arrange
        using var db = BuildDbContext(b => b
            .UseStamping<Case, TestService>((obj, service) => obj.Title = service?.TestValue)
            .UseApplicationServiceProvider(appSp)
        );

        // Act
        db.Cases.Add(new Case());
        if (async) await db.SaveChangesAsync();
        else db.SaveChanges();

        // Assert
        var entity = db.Cases.Single();
        Assert.Equal("42", entity.Title);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WhenUserUnavailable_InjectsNull(bool async)
    {
        // Arrange
        using var db = BuildDbContext(b => b
            .UseStamping<Case>((obj, user) => obj.Title = user?.ToString() ?? "null")
        );

        // Act
        db.Cases.Add(new Case());
        if (async) await db.SaveChangesAsync();
        else db.SaveChanges();

        // Assert
        var entity = db.Cases.Single();
        Assert.Equal("null", entity.Title);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WhenServiceUnavailable_InjectsNull(bool async)
    {
        // Arrange
        using var db = BuildDbContext(b => b
            .UseStamping<Case, TestService>((obj, service) => obj.Title = service?.TestValue ?? "null")
        );

        // Act
        db.Cases.Add(new Case());
        if (async) await db.SaveChangesAsync();
        else db.SaveChanges();

        // Assert
        var entity = db.Cases.Single();
        Assert.Equal("null", entity.Title);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CanInjectContext(bool async)
    {
        // Arrange
        using var db = BuildDbContext(b => b
            .UseStamping<Case, AppDbContext>((obj, db) => obj.Title = db!.Database.ProviderName)
        );

        // Act
        db.Cases.Add(new Case());
        if (async) await db.SaveChangesAsync();
        else db.SaveChanges();

        // Assert
        var entity = db.Cases.Single();
        Assert.Equal("Microsoft.EntityFrameworkCore.InMemory", entity.Title);
    }

    private AppDbContext BuildDbContext(Func<DbContextOptionsBuilder<AppDbContext>, DbContextOptionsBuilder> setup)
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString());

        var db = new AppDbContext(setup(builder).Options);

        db.Database.EnsureCreated();

        return db;
    }
}
