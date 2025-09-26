#if Identity
using Coalesce.Starter.Vue.Data.Auth;
using Microsoft.AspNetCore.Identity;
#endif
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq.AutoMock;
using Moq.Protected;

namespace Coalesce.Starter.Vue.Data.Test;

public class TestBase : IDisposable
{
    private SqliteDatabaseFixture DbFixture { get; } = new();

    private MockerScope? _CurrentMocker;
    private readonly List<Action<MockerScope>> _persistentSetups = [];

    protected AutoMocker Mocker => _CurrentMocker ?? throw new InvalidOperationException("The current mocker has been disposed.");

    protected AppDbContext Db => Mocker.Get<AppDbContext>();

    protected ClaimsPrincipal CurrentUser { get; set; } = new();

    static TestBase()
    {
        ReflectionRepository.Global.AddAssembly<AppDbContext>();
    }

    public TestBase()
    {
        _CurrentMocker = BeginMockScope(standalone: false);
    }

#if Identity
    public async Task SetCurrentUserAsync(User? user)
    {
        if (user is null)
        {
            CurrentUser = new ClaimsPrincipal();
            return;
        }

        // Mock the part of ClaimsPrincipalFactory that depends on Identity infrastructure,
        // because setting up all the mocks to make that work for real is heavy.
        using var mocker = BeginMockScope(standalone: true);
        mocker.Use(Options.Create(new IdentityOptions()));
        var principalFactory = mocker.CreateSelfMock<ClaimsPrincipalFactory>(callBase: true);
        Mock.Get(principalFactory)
            .Protected()
            .Setup<Task<ClaimsIdentity>>("GenerateClaimsAsync", user)
            .ReturnsAsync(() => new ClaimsIdentity(
                [
                    new Claim(AppClaimTypes.UserId, user.Id),
                    new Claim(AppClaimTypes.UserName, user.UserName!),
                    new Claim(AppClaimTypes.Email, user.Email!),
                ],
                "Test",
                AppClaimTypes.UserName,
                AppClaimTypes.Role
            ));

        CurrentUser = await principalFactory.CreateAsync(user);
    }

#endif
    /// <summary>
    /// <para>
    /// Create a new <see cref="Mocker"/>, allowing for new instances of all services
    /// to be obtained - especially a new <see cref="AppDbContext"/>.
    /// Persistence mechanisms are maintained, including the same <see cref="SqliteDatabaseFixture"/>.
    /// </para>
    /// <para>
    /// Doing this allows you to verify that test setup steps haven't polluted the state
    /// of services in such a way that will cause the test to behave differently than it would
    /// in a real scenario.
    /// </para>
    /// <para>
    /// For example, you should call this after setting up test data, and also in multi-step tests
    /// where the steps would normally be performed by separate web requests/background jobs/etc.
    /// </para>
    /// </summary>
    protected void RefreshServices()
    {
        _CurrentMocker?.Dispose();
        BeginMockScope(standalone: false);
    }

    /// <summary>
    /// Create an standalone mocker instance that can be used to acquire services.
    /// Usually you should use the current mock scope in <see cref="Mocker"/>, 
    /// resetting it with <see cref="RefreshServices"/> as needed. 
    /// Only create an independent scope for operations like parallel processing
    /// (an unusual thing to be doing in unit tests).
    /// </summary>
    protected MockerScope BeginMockScope() => BeginMockScope(true);

    /// <summary>
    /// Register per-test or per-class mocker setup that is applied immediately,
    /// and then again on each <see cref="RefreshServices"/> call.
    /// </summary>
    protected void PersistentSetup(Action<AutoMocker> setup)
    {
        _persistentSetups.Add(setup);
        if (_CurrentMocker is not null)
        {
            setup(_CurrentMocker);
        }
    }

    private MockerScope BeginMockScope(bool standalone = false)
    {
        var mocker = new MockerScope(standalone ? null : this);

        var dbOptions = new DbContextOptionsBuilder<AppDbContext>(DbFixture.Options)
            .UseApplicationServiceProvider(mocker).Options;

        var db = new AppDbContextForSqlite(dbOptions);
#if Tenancy
        db.TenantId = db.Tenants.OrderBy(t => t.TenantId).First().TenantId;
#endif

        // Expose the current user to EF extensions (if any).
        mocker.GetMock<IHttpContextAccessor>()
            .SetupGet(a => a.HttpContext)
            .Returns(() => new DefaultHttpContext { User = CurrentUser });

        mocker.Use(dbOptions);
        mocker.Use<AppDbContext>(db);

        mocker.Use<CrudContext<AppDbContext>>(new CrudContext<AppDbContext>(
            db,
            () => CurrentUser
        ));

        mocker.GetMock<IDbContextFactory<AppDbContext>>()
            .Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new AppDbContextForSqlite(dbOptions));

        mocker.GetMock<IDbContextFactory<AppDbContext>>()
            .Setup(x => x.CreateDbContext())
            .Returns(() => new AppDbContextForSqlite(dbOptions));

        mocker.Use<IMemoryCache>(new MemoryCache(new MemoryCacheOptions()));

        // Register additional services required by tests,
        // preferring real implementations where possible
        // in order to improve test fidelity.

        foreach (var setup in _persistentSetups) setup(mocker);

        return mocker;
    }

    public virtual void Dispose()
    {
        _CurrentMocker?.Dispose();
        DbFixture.Dispose();
    }

    public class MockerScope : AutoMocker, IDisposable
    {
        private readonly TestBase? _Parent;

        public MockerScope(TestBase? parent) : base(MockBehavior.Loose, DefaultValue.Empty, callBase: true)
        {
            _Parent = parent;
            if (parent != null) parent._CurrentMocker = this;
        }

        public void Dispose()
        {
            if (_Parent == null) return;
            Interlocked.CompareExchange(ref _Parent._CurrentMocker, null, this);
            this.AsDisposable().Dispose();
        }
    }
}
