using IntelliTect.Coalesce.DataAnnotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace IntelliTect.Coalesce.AuditLogging.Tests;

public class AuditTests : IDisposable
{
    public SqliteConnection SqliteConn { get; }

    public AuditTests()
    {
        SqliteConn = new SqliteConnection("Data Source=:memory:");
        SqliteConn.Open();
    }

    public void Dispose()
    {
        SqliteConn?.Dispose();
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task WithSqlite_PopulatesContextAndSavesEntries(bool async)
    {
        // Arrange
        using var db = BuildDbContext(b => b
            .UseCoalesceAuditLogging<TestAuditLog>(x => x
                .WithAugmentation<TestOperationContext>()
            ));

        await RunBasicTest(db, async,
            expectedCustom1: "from TestOperationContext",
            expectedCustom2: null);
    }

    [Test]
    [Arguments(false)]
    [Arguments(true)]
    public async Task SuppressAudit_SuppressesAudit(bool async)
    {
        // Arrange
        using var db = BuildDbContext(b => b
            .UseCoalesceAuditLogging((Action<AuditLoggingBuilder<TestAuditLog>>?)(x => x
                .WithAugmentation<TestOperationContext>())
            ));

        // Act
        db.SuppressAudit = true;
        db.Add(new AppUser { Name = "bob" });
        int rows = async ? await db.SaveChangesAsync() : db.SaveChanges();

        // Assert
        await Assert.That(db.AuditLogs.Count()).IsEqualTo(0);
        await Assert.That(rows).IsEqualTo(1);
    }

    [Test]
    // Volatility of underlying ConcurrentDictionary of underlying MemoryCache causes sporadic failures of this test
    // when run in parallel with the other tests
    [NotInParallel]
    public async Task ConfigureAudit_IsCached()
    {
        // Arrange
        int calls1 = 0;
        StrongBox<int> calls2 = new(0);
        TestDbContext MakeDb(bool excludeUser = true)
        {
            var db = BuildDbContext(b => b
                .UseCoalesceAuditLogging<TestAuditLog>(x =>
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
                        x.Exclude<TestAuditLog>();
                    }, calls2);
                }));
            db.Database.EnsureCreated();
            return db;
        }

        // Act/Assert 1
        var db = MakeDb();
        db.Add(new AppUser { Name = "bob" });
        db.SaveChanges();

        await Assert.That(db.AuditLogs.Count()).IsEqualTo(0);
        // Our config functions should have each been called:
        await Assert.That(calls1).IsEqualTo(1);
        await Assert.That(calls2.Value).IsEqualTo(1);

        // Act/Assert 2
        db = MakeDb();
        db.Users.Single().Name = "bob2";
        db.SaveChanges();

        await Assert.That(db.AuditLogs.Count()).IsEqualTo(0);
        // Our config functions should have still only been called once.
        await Assert.That(calls1).IsEqualTo(1);
        await Assert.That(calls2.Value).IsEqualTo(1);

        // Act/Assert 3
        db = MakeDb(excludeUser: false);
        db.Users.Single().Name = "bob3";
        db.SaveChanges();

        // The first config function will still be called only once
        // because it is turned off for `excludeUser: false`,
        // and the second config function will now be called twice
        // since it is being chained off a different base state than before.
        await Assert.That(db.AuditLogs.Count()).IsEqualTo(1);
        await Assert.That(calls1).IsEqualTo(1);
        await Assert.That(calls2.Value).IsEqualTo(2);
    }

    [Test]
    public async Task WithFullApp_CanAccessApplicationServicesFromOperationContext()
    {
        // Arrange
        var builder = CreateAppBuilder();

        builder.Services.AddScoped<ApplicationRegisteredService>();

        builder.Services.AddDbContext<TestDbContext>(options => options
            .UseSqlite(SqliteConn)
            .UseCoalesceAuditLogging<TestAuditLog>(x => x
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

    [Test]
    public async Task WithFullApp_WhenDependencyIsOptionalAndMissing_DoesNotFailToConstructOperationContext()
    {
        // Arrange
        var builder = CreateAppBuilder();

        builder.Services.AddDbContext<TestDbContext>(options => options
            .UseSqlite(SqliteConn)
            .UseCoalesceAuditLogging<TestAuditLog>(x => x
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

    [Test]
    public async Task WithFullApp_CanInjectInterfaceRegisteredOperationContext()
    {
        // Arrange
        var builder = CreateAppBuilder();

        builder.Services.AddSingleton<IAuditOperationContext<TestAuditLog>, TestOperationContext>();
        builder.Services.AddDbContextFactory<TestDbContext>(options => options
            .UseSqlite(SqliteConn)
            .UseCoalesceAuditLogging<TestAuditLog>(x => x
                .WithAugmentation<IAuditOperationContext<TestAuditLog>>()
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


    [Test]
    [Arguments(PropertyDescriptionMode.None, null)]
    [Arguments(PropertyDescriptionMode.FkListText, "ListTextA")]
    public async Task PropertyDesc_RespectsConfig(PropertyDescriptionMode mode, string? expected)
    {
        // Arrange
        using var db = BuildDbContext(b => b
            .UseCoalesceAuditLogging<TestAuditLog>(x => x
                .WithAugmentation<TestOperationContext>()
                .WithPropertyDescriptions(mode)
            ));

        var user = new AppUser { Name = "bob", Parent1 = new() { CustomListTextField = "ListTextA" } };
        db.Add(user);
        await db.SaveChangesAsync();

        var log = db.AuditLogs.Include(l => l.Properties).Single(e => e.Type == nameof(AppUser));
        var typeChangeProp = await Assert.That(log.Properties!.Where(p => p.PropertyName == nameof(AppUser.Parent1Id))).HasSingleItem();

        await Assert.That(typeChangeProp.NewValueDescription).IsEqualTo(expected);
    }

    [Test]
    public async Task PropertyDesc_PopulatesValuesForMappedListText()
    {
        // Arrange
        using var db = BuildDbContext(b => b
            .UseCoalesceAuditLogging<TestAuditLog>(x => x
                .WithAugmentation<TestOperationContext>()
            ));

        db.SuppressAudit = true;
        db.Add(new ParentWithMappedListText { Id = "A", CustomListTextField = "ListTextA" });
        db.Add(new ParentWithMappedListText { Id = "B", CustomListTextField = "ListTextB" });
        db.SaveChanges();
        db.SuppressAudit = false;

        // Act/Assert: Insert
        Cleanup();
        var user = new AppUser { Name = "bob", Parent1Id = "A" };
        db.Add(user);
        await db.SaveChangesAsync();

        var typeChangeProp = await GetAuditLogProp();
        await Assert.That(typeChangeProp.OldValueDescription).IsNull();
        await Assert.That(typeChangeProp.NewValueDescription).IsEqualTo("ListTextA");


        // Act/Assert: Update
        Cleanup();
        user = db.Users.Single();
        user.Parent1Id = "B";
        await db.SaveChangesAsync();

        typeChangeProp = await GetAuditLogProp();
        await Assert.That(typeChangeProp.OldValueDescription).IsEqualTo("ListTextA");
        await Assert.That(typeChangeProp.NewValueDescription).IsEqualTo("ListTextB");


        // Act/Assert: Delete
        Cleanup();
        db.Remove(user);
        await db.SaveChangesAsync();

        typeChangeProp = await GetAuditLogProp();
        await Assert.That(typeChangeProp.OldValueDescription).IsEqualTo("ListTextB");
        await Assert.That(typeChangeProp.NewValueDescription).IsNull();

        void Cleanup()
        {
            db.AuditLogs.ExecuteDelete();
            db.ChangeTracker.Clear();
        }
        async Task<AuditLogProperty> GetAuditLogProp()
        {
            var log = db.AuditLogs.Include(l => l.Properties).Single(e => e.Type == nameof(AppUser));
            return await Assert.That(log.Properties!.Where(p => p.PropertyName == nameof(AppUser.Parent1Id))).HasSingleItem();
        }
    }

    [Test]
    public async Task PropertyDesc_PopulatesValuesCorrectlyWhenPrincipalAlsoChanges()
    {
        // Arrange
        using var db = BuildDbContext(b => b
            .UseCoalesceAuditLogging<TestAuditLog>(x => x
                .WithAugmentation<TestOperationContext>()
            ));

        db.SuppressAudit = true;
        var parentA = new ParentWithMappedListText { Id = "A", CustomListTextField = "ListTextA" };
        db.Add(parentA);
        var parentB = new ParentWithMappedListText { Id = "B", CustomListTextField = "ListTextB" };
        db.Add(parentB);
        var user = new AppUser { Name = "bob", Parent1Id = "A" };
        db.Add(user);
        db.SaveChanges();
        db.SuppressAudit = false;

        // Act/Assert: Insert
        user.Parent1Id = "B";
        parentA.CustomListTextField = "NewListTextA";
        parentB.CustomListTextField = "NewListTextB";
        db.SaveChanges();

        var typeChangeProp = await GetAuditLogProp();
        await Assert.That(typeChangeProp.OldValueDescription).IsEqualTo("ListTextA");
        await Assert.That(typeChangeProp.NewValueDescription).IsEqualTo("NewListTextB");

        async Task<AuditLogProperty> GetAuditLogProp()
        {
            var log = db.AuditLogs.Include(l => l.Properties).Single(e => e.Type == nameof(AppUser));
            return await Assert.That(log.Properties!.Where(p => p.PropertyName == nameof(AppUser.Parent1Id))).HasSingleItem();
        }
    }

    [Test]
    public async Task PropertyDesc_PopulatesValuesForUnMappedListText()
    {
        // Arrange
        using var db = BuildDbContext(b => b
            .UseCoalesceAuditLogging<TestAuditLog>(x => x
                .WithAugmentation<TestOperationContext>()
            ));

        db.SuppressAudit = true;
        db.Add(new ParentWithUnMappedListText { Id = "A", Name = "ThingA" });
        db.Add(new ParentWithUnMappedListText { Id = "B", Name = "ThingB" });
        db.SaveChanges();
        db.SuppressAudit = false;

        // Act/Assert: Insert
        Cleanup();
        var user = new AppUser { Name = "bob", Parent2Id = "A" };
        db.Add(user);
        db.SaveChanges();

        var typeChangeProp = await GetAuditLogProp();
        await Assert.That(typeChangeProp.OldValueDescription).IsNull();
        await Assert.That(typeChangeProp.NewValueDescription).IsEqualTo("Name:ThingA");


        // Act/Assert: Update
        Cleanup();
        user = db.Users.Single();
        user.Parent2Id = "B";
        db.SaveChanges();

        typeChangeProp = await GetAuditLogProp();
        await Assert.That(typeChangeProp.OldValueDescription).IsEqualTo("Name:ThingA");
        await Assert.That(typeChangeProp.NewValueDescription).IsEqualTo("Name:ThingB");


        // Act/Assert: Delete
        Cleanup();
        db.Remove(user);
        db.SaveChanges();

        typeChangeProp = await GetAuditLogProp();
        await Assert.That(typeChangeProp.OldValueDescription).IsEqualTo("Name:ThingB");
        await Assert.That(typeChangeProp.NewValueDescription).IsNull();

        void Cleanup()
        {
            db.AuditLogs.ExecuteDelete();
            db.ChangeTracker.Clear();
        }
        async Task<AuditLogProperty> GetAuditLogProp()
        {
            var log = db.AuditLogs.Include(l => l.Properties).Single(e => e.Type == nameof(AppUser));
            return await Assert.That(log.Properties!.Where(p => p.PropertyName == nameof(AppUser.Parent2Id))).HasSingleItem();
        }
    }

    [Test]
    public async Task PropertyDesc_OnlyLoadsPrincipalWhenChanged()
    {
        // Arrange
        using var db = BuildDbContext(b => b
            .UseCoalesceAuditLogging<TestAuditLog>(x => x
                .WithAugmentation<TestOperationContext>()
            ));

        db.SuppressAudit = true;
        var user = new AppUser { Name = "bob", Parent1 = new() { CustomListTextField = "ListTextA" } };
        db.Add(user);
        await db.SaveChangesAsync();
        db.SuppressAudit = false;

        // Act
        db.ChangeTracker.Clear();
        user = db.Users.Single();
        user.Name = "bob2";
        await db.SaveChangesAsync();

        // Assert
        // Navigation prop should not be loaded because it wasn't changed.
        // This ensures we don't waste database calls loading principal entities for no reason.
        await Assert.That(db.ParentWithMappedListTexts.Local).IsEmpty();
        await Assert.That(user.Parent1).IsNull();
    }

    [Test]
    public async Task PropertyDesc_DoesntBreakForOneToOneWhenPkIsFk()
    {
        // Arrange
        using var db = BuildDbContext(b => b
            .UseCoalesceAuditLogging<TestAuditLog>(x => x
                .WithAugmentation<TestOperationContext>()
            ));

        // Act
        var entity = new OneToOneParent { Name = "bob" };
        db.Add(entity);
        await db.SaveChangesAsync();


        // Assert
        var log = await Assert.That(db.AuditLogs.Include(l => l.Properties)).HasSingleItem();
    }

    [Test]
    public async Task FormatsPrimitiveCollections()
    {
        using var db = BuildDbContext(b => b
            .UseCoalesceAuditLogging<TestAuditLog>(x => x
                .WithAugmentation<TestOperationContext>()
            ));

        db.Add(new AppUser { Name = "Bob", EnumArray = [SecurityPermissionLevels.DenyAll, SecurityPermissionLevels.AllowAuthenticated] });
        db.SaveChanges();

        var log = await Assert.That(db.AuditLogs).HasSingleItem();
        var prop = await Assert.That(log.Properties!.Where(p => p.PropertyName == nameof(AppUser.EnumArray))).HasSingleItem();
        await Assert.That(prop.NewValue).IsEqualTo("[DenyAll, AllowAuthenticated]");
    }

    private WebApplicationBuilder CreateAppBuilder()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Host.UseDefaultServiceProvider(sp => sp.ValidateScopes = true);
        builder.Logging.ClearProviders();
        return builder;
    }

    private TestDbContext BuildDbContext(Func<DbContextOptionsBuilder<TestDbContext>, DbContextOptionsBuilder> setup)
    {
        var builder = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(SqliteConn);

        var db = new TestDbContext(setup(builder).Options);

        db.Database.EnsureCreated();

        return db;
    }

    private static async Task RunBasicTest(
        TestDbContext db,
        bool async,
        string? expectedCustom1,
        string? expectedCustom2
    )
    {
        db.Database.EnsureCreated();

        var user = new AppUser { Name = "bob" };
        db.Add(user);

        // Act
        if (async) await db.SaveChangesAsync();
        else db.SaveChanges();

        // Assert
        var entry = await Assert.That(db.AuditLogs.Include(c => c.Properties)).HasSingleItem();
        await Assert.That(entry.CustomField1).IsEqualTo(expectedCustom1);
        await Assert.That(entry.CustomField2).IsEqualTo(expectedCustom2);
        await Assert.That(entry.Type).IsEqualTo(nameof(AppUser));
        await Assert.That(entry.KeyValue).IsEqualTo(user.Id);
        await Assert.That(entry.Description).IsEqualTo(user.Name);
        await Assert.That(entry.State).IsEqualTo(AuditEntryState.EntityAdded);
        await Assert.That(entry.Date.UtcDateTime).IsEqualTo(DateTimeOffset.Now.UtcDateTime).Within(TimeSpan.FromSeconds(10));

        var idProp = entry.Properties!.ElementAt(0);
        await Assert.That(idProp.PropertyName).IsEqualTo(nameof(AppUser.Id));
        await Assert.That(idProp.NewValue).IsEqualTo(user.Id);

        var nameProp = entry.Properties!.ElementAt(1);
        await Assert.That(nameProp.PropertyName).IsEqualTo(nameof(AppUser.Name));
        await Assert.That(nameProp.NewValue).IsEqualTo(user.Name);
    }
}

class TestOperationContext(IHttpContextAccessor? httpContext = null) : IAuditOperationContext<TestAuditLog>
{
    public void Populate(TestAuditLog auditEntry, EntityEntry entry)
    {
        auditEntry.UserId = httpContext?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        auditEntry.CustomField1 = "from TestOperationContext";
    }
}

class TestOperationContextWithAppService(ApplicationRegisteredService? service = null) : IAuditOperationContext<TestAuditLog>
{
    public void Populate(TestAuditLog auditEntry, EntityEntry entry)
    {
        auditEntry.CustomField1 = "from TestOperationContextWithAppService";
        auditEntry.CustomField2 = service?.ValueFromAppService;
    }
}

class ApplicationRegisteredService
{
    public string ValueFromAppService = "from ApplicationRegisteredService";
}
