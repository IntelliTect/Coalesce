using IntelliTect.Coalesce.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Runtime.CompilerServices;
using Z.EntityFramework.Plus;

namespace IntelliTect.Coalesce.AuditLogging.Tests;

public class AuditTests
{
    public SqliteConnection SqliteConn { get; }

    public AuditTests()
    {
        SqliteConn = new SqliteConnection("Data Source=:memory:");
        SqliteConn.Open();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
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

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
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
        Assert.Equal(0, db.AuditLogs.Count());
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

        Assert.Equal(0, db.AuditLogs.Count());
        // Our config functions should have each been called:
        Assert.Equal(1, calls1);
        Assert.Equal(1, calls2.Value);

        // Act/Assert 2
        db = MakeDb();
        db.Users.Single().Name = "bob2";
        db.SaveChanges();

        Assert.Equal(0, db.AuditLogs.Count());
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
        Assert.Equal(1, db.AuditLogs.Count());
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

    [Fact]
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

    [Fact]
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


    [Theory]
    [InlineData(PropertyDescriptionMode.None, null)]
    [InlineData(PropertyDescriptionMode.FkListText, "ListTextA")]
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
        var typeChangeProp = Assert.Single(log.Properties!.Where(p => p.PropertyName == nameof(AppUser.Parent1Id)));

        Assert.Equal(expected, typeChangeProp.NewValueDescription);
    }

    [Fact]
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

        var typeChangeProp = GetAuditLogProp();
        Assert.Null(typeChangeProp.OldValueDescription);
        Assert.Equal("ListTextA", typeChangeProp.NewValueDescription);


        // Act/Assert: Update
        Cleanup();
        user = db.Users.Single();
        user.Parent1Id = "B";
        await db.SaveChangesAsync();

        typeChangeProp = GetAuditLogProp();
        Assert.Equal("ListTextA", typeChangeProp.OldValueDescription);
        Assert.Equal("ListTextB", typeChangeProp.NewValueDescription);


        // Act/Assert: Delete
        Cleanup();
        db.Remove(user);
        await db.SaveChangesAsync();

        typeChangeProp = GetAuditLogProp();
        Assert.Equal("ListTextB", typeChangeProp.OldValueDescription);
        Assert.Null(typeChangeProp.NewValueDescription);

        void Cleanup()
        {
            db.AuditLogs.Delete();
            db.ChangeTracker.Clear();
        }
        AuditLogProperty GetAuditLogProp()
        {
            var log = db.AuditLogs.Include(l => l.Properties).Single(e => e.Type == nameof(AppUser));
            return Assert.Single(log.Properties!.Where(p => p.PropertyName == nameof(AppUser.Parent1Id)));
        }
    }

    [Fact]
    public void PropertyDesc_PopulatesValuesCorrectlyWhenPrincipalAlsoChanges()
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

        var typeChangeProp = GetAuditLogProp();
        Assert.Equal("ListTextA", typeChangeProp.OldValueDescription);
        Assert.Equal("NewListTextB", typeChangeProp.NewValueDescription);

        AuditLogProperty GetAuditLogProp()
        {
            var log = db.AuditLogs.Include(l => l.Properties).Single(e => e.Type == nameof(AppUser));
            return Assert.Single(log.Properties!.Where(p => p.PropertyName == nameof(AppUser.Parent1Id)));
        }
    }

    [Fact]
    public void PropertyDesc_PopulatesValuesForUnMappedListText()
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

        var typeChangeProp = GetAuditLogProp();
        Assert.Null(typeChangeProp.OldValueDescription);
        Assert.Equal("Name:ThingA", typeChangeProp.NewValueDescription);


        // Act/Assert: Update
        Cleanup();
        user = db.Users.Single();
        user.Parent2Id = "B";
        db.SaveChanges();

        typeChangeProp = GetAuditLogProp();
        Assert.Equal("Name:ThingA", typeChangeProp.OldValueDescription);
        Assert.Equal("Name:ThingB", typeChangeProp.NewValueDescription);


        // Act/Assert: Delete
        Cleanup();
        db.Remove(user);
        db.SaveChanges();

        typeChangeProp = GetAuditLogProp();
        Assert.Equal("Name:ThingB", typeChangeProp.OldValueDescription);
        Assert.Null(typeChangeProp.NewValueDescription);

        void Cleanup()
        {
            db.AuditLogs.Delete();
            db.ChangeTracker.Clear();
        }
        AuditLogProperty GetAuditLogProp()
        {
            var log = db.AuditLogs.Include(l => l.Properties).Single(e => e.Type == nameof(AppUser));
            return Assert.Single(log.Properties!.Where(p => p.PropertyName == nameof(AppUser.Parent2Id)));
        }
    }

    [Fact]
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
        Assert.Empty(db.ParentWithMappedListTexts.Local);
        Assert.Null(user.Parent1);
    }

    private WebApplicationBuilder CreateAppBuilder()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Host.UseDefaultServiceProvider(sp => sp.ValidateScopes = true);
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
        var entry = Assert.Single(db.AuditLogs.Include(c => c.Properties));
        Assert.Equal(expectedCustom1, entry.CustomField1);
        Assert.Equal(expectedCustom2, entry.CustomField2);
        Assert.Equal(nameof(AppUser), entry.Type);
        Assert.Equal(user.Id, entry.KeyValue);
        Assert.Equal(user.Name, entry.Description);
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

class TestOperationContext(IHttpContextAccessor? httpContext = null) : IAuditOperationContext<TestAuditLog>
{
    public void Populate(TestAuditLog auditEntry, EntityEntry entry)
    {
        auditEntry.UserId = httpContext?.HttpContext?.User?.GetUserId();
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
