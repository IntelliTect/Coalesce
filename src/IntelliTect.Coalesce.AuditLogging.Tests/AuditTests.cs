using IntelliTect.Coalesce.AuditLogging;
using IntelliTect.Coalesce.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Z.EntityFramework.Plus;

namespace IntelliTect.Coalesce.AuditLogging.Tests;
public class AuditTests
{
    private const string SqlServerConnString = "Server=(localdb)\\MSSQLLocalDB;Database=CoalesceAuditLoggingTests;Trusted_Connection=True;";

    public SqliteConnection SqliteConn { get; }

    public AuditTests()
    {
        SqliteConn = new SqliteConnection("Data Source=:memory:");
        SqliteConn.Open();
    }

    [Fact]
    public async Task WithSqlServer_UpdatesExistingRecordForLikeChanges()
    {
        // Arrange
        using var db = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlServer(SqlServerConnString)
            .UseCoalesceAuditLogging<TestObjectChange>()
            .Options);

        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var user = new AppUser { Name = "bob" };
        db.Add(user);
        await db.SaveChangesAsync();

        // Act/Assert
        user.Name = "bob2";
        await db.SaveChangesAsync();

        Assert.Equal(2, db.ObjectChanges.Count()); // Two records: EntityAdded, and EntityUpdated

        // Act/Assert
        user.Name = "bob3";
        await db.SaveChangesAsync();

        Assert.Equal(2, db.ObjectChanges.Count()); // Two records: EntityAdded, and EntityUpdated

        // Assert
        var entry = Assert.Single(db.ObjectChanges.Include(c => c.Properties).Where(c => c.State == AuditEntryState.EntityModified));

        var propChange = Assert.Single(entry.Properties);
        Assert.Equal(nameof(AppUser.Name), propChange.PropertyName);
        Assert.Equal("bob", propChange.OldValue);
        Assert.Equal("bob3", propChange.NewValue);
    }

    [Fact]
    public async Task WithSqlServer_CreatesNewRecordForLikeChangesOutsideMergeWindow()
    {
        // Arrange
        using var db = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlServer(SqlServerConnString)
            .UseCoalesceAuditLogging<TestObjectChange>()
            .Options);

        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var user = new AppUser { Name = "bob" };
        db.Add(user);
        await db.SaveChangesAsync();

        // Act/Assert
        user.Name = "bob2";
        await db.SaveChangesAsync();

        Assert.Equal(2, db.ObjectChanges.Count()); // Two records: EntityAdded, and EntityUpdated

        // Make the original Updated entry old such that it is outside the merge window.
        db.ObjectChanges.First(c => c.State == AuditEntryState.EntityModified).Date -= TimeSpan.FromMinutes(1);
        db.SaveChanges();

        // Act/Assert
        user.Name = "bob3";
        await db.SaveChangesAsync();
        Assert.Equal(3, db.ObjectChanges.Count()); // Now two records for EntityUpdated
    }

    [Fact]
    public async Task WithSqlServer_CreatesNewRecordForUnlikeChanges()
    {
        // Arrange
        using var db = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlServer(SqlServerConnString)
            .UseCoalesceAuditLogging<TestObjectChange>()
            .Options);

        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        var user = new AppUser { Name = "bob" };
        db.Add(user);
        await db.SaveChangesAsync();

        // Act/Assert
        user.Name = "bob2";
        await db.SaveChangesAsync();

        Assert.Equal(2, db.ObjectChanges.Count()); // Two records: EntityAdded, and EntityUpdated

        // Act/Assert
        user.Name = "bob3";
        user.Title = "Associate";
        await db.SaveChangesAsync();

        Assert.Equal(3, db.ObjectChanges.Count()); // Now two records for EntityUpdated
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WithSqlServer_PopulatesContextAndSavesEntries(bool async)
    {
        // Arrange
        using var db = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlServer(SqlServerConnString)
            .UseCoalesceAuditLogging<TestObjectChange>(x => x
                .WithAugmentation<TestOperationContext>()
            )
            .Options);

        await RunBasicTest(db, async,
            expectedCustom1: "from TestOperationContext",
            expectedCustom2: null);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task WithSqlite_PopulatesContextAndSavesEntries(bool async)
    {
        // Arrange
        using var db = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(SqliteConn)
            .UseCoalesceAuditLogging<TestObjectChange>(x => x
                .WithAugmentation<TestOperationContext>()
            )
            .Options);

        await RunBasicTest(db, async,
            expectedCustom1: "from TestOperationContext",
            expectedCustom2: null);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SuppressAudit_SuppressesAudit(bool async)
    {
        // Arrange
        using var db = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(SqliteConn)
            .UseCoalesceAuditLogging<TestObjectChange>(x => x
                .WithAugmentation<TestOperationContext>()
            )
            .Options);

        // Act
        db.SuppressAudit = true;
        db.Add(new AppUser { Name = "bob" });
        int rows = async ? await db.SaveChangesAsync() : db.SaveChanges();

        // Assert
        Assert.Equal(0, db.ObjectChanges.Count());
        Assert.Equal(1, rows);
    }

    [Fact]
    public void ConfigureAudit_IsCached()
    {
        // Arrange
        int calls1 = 0;
        StrongBox<int> calls2 = new(0);
        TestDbContext MakeDb(bool excludeUser = true)
        {
            var db = new TestDbContext(new DbContextOptionsBuilder<TestDbContext>()
                .UseSqlite(SqliteConn)
                .UseCoalesceAuditLogging<TestObjectChange>(x =>
                {
                    x.WithAugmentation<TestOperationContext>();
                    if (excludeUser)
                    {
                        // Dynamic config can be done via conditional calls to ConfigureAudit
                        x.ConfigureAudit(c =>
                        {
                            c.Exclude<AppUser>();
                            calls1++;
                        });
                    }

                    // Dynamic config can also be done by passing things to arg2.
                    // Note that StrongBox doesn't have equality semantics,
                    // so the incrementing of it won't actually produce a new cached config instance.
                    x.ConfigureAudit(static (x, arg) =>
                    {
                        arg.Value++;
                        x.Exclude<TestObjectChange>();
                    }, calls2);
                })
                .Options);
            db.Database.EnsureCreated();
            return db;
        }

        // Act/Assert 1
        var db = MakeDb();
        db.Add(new AppUser { Name = "bob" });
        db.SaveChanges();

        Assert.Equal(0, db.ObjectChanges.Count());
        // Our config functions should have each been called:
        Assert.Equal(1, calls1);
        Assert.Equal(1, calls2.Value);

        // Act/Assert 2
        db = MakeDb();
        db.Users.Single().Name = "bob2";
        db.SaveChanges();

        Assert.Equal(0, db.ObjectChanges.Count());
        // Our config functions should have still only been called once.
        Assert.Equal(1, calls1);
        Assert.Equal(1, calls2.Value);

        // Act/Assert 3
        db = MakeDb(excludeUser: false);
        db.Users.Single().Name = "bob3";
        db.SaveChanges();

        // The first config function will still be called only once
        // because it is turned off for `excludeUser: false`,
        // and the second config function will now be called twice
        // since it is being chained off a different base state than before.
        Assert.Equal(1, db.ObjectChanges.Count());
        Assert.Equal(1, calls1);
        Assert.Equal(2, calls2.Value);
    }

    [Fact]
    public async Task WithFullApp_CanAccessApplicationServicesFromOperationContext()
    {
        // Arrange
        var builder = CreateAppBuilder();

        builder.Services.AddScoped<ApplicationRegisteredService>();

        builder.Services.AddDbContext<TestDbContext>(options => options
            .UseSqlite(SqliteConn)
            .UseCoalesceAuditLogging<TestObjectChange>(x => x
                .WithAugmentation<TestOperationContextWithAppService>()
            )
        );

        var app = builder.Build();

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        // Act/Assert
        await RunBasicTest(db, async: true,
            expectedCustom1: "from TestOperationContextWithAppService",
            expectedCustom2: "from ApplicationRegisteredService"
        );
    }

    [Fact]
    public async Task WithFullApp_WhenDependencyIsOptionalAndMissing_DoesNotFailToConstructOperationContext()
    {
        // Arrange
        var builder = CreateAppBuilder();

        builder.Services.AddDbContext<TestDbContext>(options => options
            .UseSqlite(SqliteConn)
            .UseCoalesceAuditLogging<TestObjectChange>(x => x
                .WithAugmentation<TestOperationContextWithAppService>()
            )
        );

        var app = builder.Build();

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        // Act/Assert
        await RunBasicTest(db, async: true,
            expectedCustom1: "from TestOperationContextWithAppService",
            expectedCustom2: null
        );
    }

    [Fact]
    public async Task WithFullApp_CanInjectInterfaceRegisteredOperationContext()
    {
        // Arrange
        var builder = CreateAppBuilder();

        builder.Services.AddSingleton<IAuditOperationContext<TestObjectChange>, TestOperationContext>();
        builder.Services.AddDbContextFactory<TestDbContext>(options => options
            .UseSqlite(SqliteConn)
            .UseCoalesceAuditLogging<TestObjectChange>(x => x
                .WithAugmentation<IAuditOperationContext<TestObjectChange>>()
            )
        );

        var app = builder.Build();

        // Act/Assert
        using var db = app.Services.GetRequiredService<IDbContextFactory<TestDbContext>>().CreateDbContext();
        await RunBasicTest(db, async: true,
            expectedCustom1: "from TestOperationContext",
            expectedCustom2: null
        );
    }

    private WebApplicationBuilder CreateAppBuilder()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Host.UseDefaultServiceProvider(sp => sp.ValidateScopes = true);
        return builder;
    }

    private static async Task RunBasicTest(
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
        Assert.Equal(nameof(AppUser), entry.Type);
        Assert.Equal(user.Id, entry.KeyValue);
        Assert.Equal(AuditEntryState.EntityAdded, entry.State);
        Assert.Equal(DateTimeOffset.Now.UtcDateTime, entry.Date.UtcDateTime, TimeSpan.FromSeconds(10));

        var idProp = entry.Properties!.ElementAt(0);
        Assert.Equal(nameof(AppUser.Id), idProp.PropertyName);
        Assert.Equal(user.Id, idProp.NewValue);

        var nameProp = entry.Properties!.ElementAt(1);
        Assert.Equal(nameof(AppUser.Name), nameProp.PropertyName);
        Assert.Equal(user.Name, nameProp.NewValue);
    }
}

class TestOperationContext(IHttpContextAccessor? httpContext = null) : IAuditOperationContext<TestObjectChange>
{
    public void Populate(TestObjectChange auditEntry, EntityEntry entry)
    {
        auditEntry.UserId = httpContext?.HttpContext?.User?.GetUserId();
        auditEntry.CustomField1 = "from TestOperationContext";
    }
}

class TestOperationContextWithAppService(ApplicationRegisteredService? service = null) : IAuditOperationContext<TestObjectChange>
{
    public void Populate(TestObjectChange auditEntry, EntityEntry entry)
    {
        auditEntry.CustomField1 = "from TestOperationContextWithAppService";
        auditEntry.CustomField2 = service?.ValueFromAppService;
    }
}

class ApplicationRegisteredService
{
    public string ValueFromAppService = "from ApplicationRegisteredService";
}
