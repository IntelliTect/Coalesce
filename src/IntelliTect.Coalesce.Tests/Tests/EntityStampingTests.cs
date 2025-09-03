using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
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
    public async Task WithApplicationServiceDefinedHttpContextAccessor_SetsStampValueFromAppServiceHttpContext(bool async)
    {
        // Arrange
        var mock = new Mock<IHttpContextAccessor>();
        mock.SetupGet(c => c.HttpContext).Returns(new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity("TestAuth")) });

        var appSp = new ServiceCollection()
            .AddSingleton(mock.Object)
            .BuildServiceProvider();

        using var db = BuildDbContext(b =>
        {
            var b2 = b.UseStamping<Case>((obj, user) => obj.Title = user!.Identity!.AuthenticationType)
                .UseApplicationServiceProvider(appSp);

            // Add an internally registered IHttpContextAccessor as well so we can test that application services are preferred
            ((IDbContextOptionsBuilderInfrastructure)b2).AddOrUpdateExtension(new InternalServiceModifyExtension());

            return b2;
        });

        // Act
        db.Cases.Add(new Case());
        if (async) await db.SaveChangesAsync();
        else db.SaveChanges();

        // Assert
        var entity = db.Cases.Single();
        Assert.Equal("TestAuth", entity.Title);
    }

    /// <summary>
    /// This effectively emulates what happens to the services if Coalesce.AuditLogging is added.
    /// </summary>
    private class InternalServiceModifyExtension : IDbContextOptionsExtension
    {
        public DbContextOptionsExtensionInfo Info => new InfoClass(this);

        private class InfoClass(IDbContextOptionsExtension extension) : DbContextOptionsExtensionInfo(extension)
        {
            public override bool IsDatabaseProvider => false;
            public override string LogFragment => "";
            public override int GetServiceProviderHashCode() => 0;
            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo) { }
            public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => true;
        }

        public void ApplyServices(IServiceCollection services) => services.AddHttpContextAccessor();

        public void Validate(IDbContextOptions options) { }
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
