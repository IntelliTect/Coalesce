using IntelliTect.Coalesce;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.AutoMock;
using System.Security.Claims;

namespace Coalesce.Starter.Vue.Data.Test;

public class TestBase : IDisposable
{
    protected SqliteDatabaseFixture DbFixture { get; }

    private MockerScope? _CurrentMocker;

    protected AutoMocker Mocker => _CurrentMocker ?? throw new InvalidOperationException("The current mocker has been disposed.");

    protected AppDbContext Db => Mocker.Get<AppDbContext>();

    protected ClaimsPrincipal CurrentUser { get; set; } = new();

    public TestBase()
    {
        ReflectionRepository.Global.AddAssembly<AppDbContext>();

        DbFixture = new SqliteDatabaseFixture();

        _CurrentMocker = BeginMockScope(standalone: false);
    }

    public class MockerScope : AutoMocker, IDisposable
    {
        private readonly TestBase? _Parent;

        public MockerScope(TestBase? parent) : base(MockBehavior.Loose)
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

    private MockerScope BeginMockScope(bool standalone = false)
    {
        var mocker = new MockerScope(standalone ? null : this);
        var db = new AppDbContextForSqlite(DbFixture.Options);
        mocker.Use(DbFixture.Options);
        mocker.Use<AppDbContext>(db);

        mocker.Use<CrudContext<AppDbContext>>(new CrudContext<AppDbContext>(
            db,
            () => CurrentUser
        ));

        mocker.GetMock<IDbContextFactory<AppDbContext>>()
            .Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new AppDbContextForSqlite(DbFixture.Options));

        mocker.GetMock<IDbContextFactory<AppDbContext>>()
            .Setup(x => x.CreateDbContext())
            .Returns(() => new AppDbContextForSqlite(DbFixture.Options));

        mocker.Use<IMemoryCache>(new MemoryCache(new MemoryCacheOptions()));

        // Register additional services required by tests,
        // preferring real implementations where possible
        // in order to improve test fidelity.

        return mocker;
    }

    public virtual void Dispose()
    {
        _CurrentMocker?.Dispose();
        DbFixture.Dispose();
    }
}
