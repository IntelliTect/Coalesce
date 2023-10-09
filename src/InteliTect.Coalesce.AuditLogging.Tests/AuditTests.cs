using IntelliTect.Coalesce.AuditLogging;
using IntelliTect.Coalesce.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace IntelliTect.Coalesce.AuditLogging.Tests;
public class AuditTests
{
    private const string SqlServerConnString = "Server=(localdb)\\MSSQLLocalDB;Database=CoalesceAuditLoggingTests;Trusted_Connection=True;";

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WithSqlServer_WithoutFullApp_PopulatesContextAndSavesEntries(bool async)
    {
        // Arrange
        using var db = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlServer(SqlServerConnString)
            .UseCoalesceAuditLogging<TestObjectChange>()
            .Options);

        await RunTest(db, async, 
            expectedCustom1: null,
            expectedCustom2: "from IObjectChange.Populate");
    }

    [Fact]
    public async Task WithFullApp_CanAccessApplicationServicesFromOperationContext()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddScoped<ApplicationRegisteredService>();

        builder.Services.AddCoalesceAuditLoggingOperationContext<TestObjectChange, TestOperationContextWithAppService>();
        builder.Services.AddDbContext<TestDbContext>(options => options
            .UseSqlServer(SqlServerConnString)
            .UseCoalesceAuditLogging<TestObjectChange>()
        );

        var app = builder.Build();

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        // Act/Assert
        await RunTest(db, async: true, 
            expectedCustom1: "from ApplicationRegisteredService",
            // IObjectChange.Populate injects our custom service and appends its value if available.
            expectedCustom2: "from IObjectChange.Populate from ApplicationRegisteredService"
        );
    }

    private static async Task RunTest(
        TestDbContext db, 
        bool async, 
        string? expectedCustom1,
        string? expectedCustom2
    )
    {
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var user = new AppUser { Name = "bob" };
        db.Add(user);

        // Act
        if (async) await db.SaveChangesAsync();
        else db.SaveChanges();

        // Assert
        var entry = Assert.Single(db.ObjectChanges.Include(c => c.Properties));
        Assert.Equal(expectedCustom1, entry.CustomField1);
        Assert.Equal(expectedCustom2, entry.CustomField2);
        Assert.Equal(nameof(AppUser), entry.EntityTypeName);
        Assert.Equal(user.Id, entry.EntityKeyValue);
        Assert.Equal("EntityAdded", entry.State);
        Assert.Equal(DateTimeOffset.Now.UtcDateTime, entry.Date.UtcDateTime, TimeSpan.FromSeconds(10));

        var idProp = entry.Properties!.ElementAt(0);
        Assert.Equal(nameof(AppUser.Id), idProp.PropertyName);
        Assert.Equal(user.Id, idProp.NewValue);

        var nameProp = entry.Properties!.ElementAt(1);
        Assert.Equal(nameof(AppUser.Name), nameProp.PropertyName);
        Assert.Equal(user.Name, nameProp.NewValue);
    }
}

class AppUser
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string? Name { get; set; }
}

internal class TestObjectChange : ObjectChange
{
    public string? UserId { get; set; }
    public AppUser? User { get; set; }

    public string? CustomField1 { get; set; }
    public string? CustomField2 { get; set; }

    protected override void Populate(DbContext db, IServiceProvider serviceProvider, EntityEntry entry)
    {
        var service = serviceProvider.GetService<ApplicationRegisteredService>();
        CustomField2 = ("from IObjectChange.Populate " + (service?.ValueFromAppService ?? "")).Trim();
    }
}

class TestOperationContext(IHttpContextAccessor httpContext) : IAuditOperationContext<TestObjectChange>
{
    public void Populate(TestObjectChange auditEntry, EntityEntry entry)
    {
        auditEntry.UserId = httpContext.HttpContext?.User?.GetUserId();
        auditEntry.CustomField1 = "from TestOperationContext";
    }
}

class ApplicationRegisteredService
{
    public string ValueFromAppService = "from ApplicationRegisteredService";
}

class TestOperationContextWithAppService(ApplicationRegisteredService service) : IAuditOperationContext<TestObjectChange>
{
    public void Populate(TestObjectChange auditEntry, EntityEntry entry)
        => auditEntry.CustomField1 = service.ValueFromAppService;
}

internal class TestDbContext : DbContext, IAuditLogContext<TestObjectChange>
{
    public TestDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<TestObjectChange> ObjectChanges => Set<TestObjectChange>();
    public DbSet<ObjectChangeProperty> ObjectChangeProperties => Set<ObjectChangeProperty>();

    public bool SuppressAudit => false;
}
