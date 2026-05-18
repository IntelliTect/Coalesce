using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.Extensions.DependencyInjection;

namespace IntelliTect.Coalesce.Tests.Api.DataSources;

public class DataSourceFactoryTests
{
    private (DataSourceFactory Factory, ReflectionRepository Repo) BuildFactory()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>();
        services.AddCoalesce(b => b.AddContext<AppDbContext>());
        var sp = services.BuildServiceProvider();

        var rr = new ReflectionRepository();
        rr.AddAssembly<AppDbContext>();
        rr.AddAssembly<DataSourceFactoryTests>();

        return (new DataSourceFactory(sp, rr), rr);
    }

    #region Test Models

    [Coalesce, StandaloneEntity]
    public class StandaloneBase
    {
        public int Id { get; set; }

        [DefaultDataSource]
        public class DefaultSource : StandardDataSource<StandaloneBase>
        {
            public DefaultSource(CrudContext ctx) : base(ctx) { }
            public override Task<IQueryable<StandaloneBase>> GetQueryAsync(IDataSourceParameters parameters)
                => Task.FromResult(Array.Empty<StandaloneBase>().AsQueryable());
        }

        public class NamedSource : StandardDataSource<StandaloneBase>
        {
            public NamedSource(CrudContext ctx) : base(ctx) { }
            public override Task<IQueryable<StandaloneBase>> GetQueryAsync(IDataSourceParameters parameters)
                => Task.FromResult(Array.Empty<StandaloneBase>().AsQueryable());
        }
    }

    [Coalesce, StandaloneEntity]
    public class StandaloneDerived : StandaloneBase
    {
        // No own data sources - should inherit from StandaloneBase
    }

    [Coalesce, StandaloneEntity]
    public class StandaloneDerivedWithOwnDefault : StandaloneBase
    {
        [DefaultDataSource]
        public class DerivedDefaultSource : StandardDataSource<StandaloneDerivedWithOwnDefault>
        {
            public DerivedDefaultSource(CrudContext ctx) : base(ctx) { }
            public override Task<IQueryable<StandaloneDerivedWithOwnDefault>> GetQueryAsync(IDataSourceParameters parameters)
                => Task.FromResult(Array.Empty<StandaloneDerivedWithOwnDefault>().AsQueryable());
        }
    }

    [Coalesce, StandaloneEntity]
    public class StandaloneDerivedWithOwnNamed : StandaloneBase
    {
        public class OwnNamedSource : StandardDataSource<StandaloneDerivedWithOwnNamed>
        {
            public OwnNamedSource(CrudContext ctx) : base(ctx) { }
            public override Task<IQueryable<StandaloneDerivedWithOwnNamed>> GetQueryAsync(IDataSourceParameters parameters)
                => Task.FromResult(Array.Empty<StandaloneDerivedWithOwnNamed>().AsQueryable());
        }
    }

    #endregion

    [Test]
    public async Task GetDefaultDataSourceType_DerivedTypeWithNoSources_UsesBaseDefaultSource()
    {
        var (factory, rr) = BuildFactory();

        var servedType = rr.GetClassViewModel<StandaloneDerived>()!;
        var declaredFor = rr.GetClassViewModel<StandaloneDerived>()!;

        var type = factory.GetDataSourceType(servedType, declaredFor, null);

        await Assert.That(type).IsEqualTo(typeof(StandaloneBase.DefaultSource));
    }

    [Test]
    public async Task GetDataSourceType_NamedSourceOnBase_FoundForDerivedType()
    {
        var (factory, rr) = BuildFactory();

        var servedType = rr.GetClassViewModel<StandaloneDerived>()!;
        var declaredFor = rr.GetClassViewModel<StandaloneDerived>()!;

        var type = factory.GetDataSourceType(servedType, declaredFor, nameof(StandaloneBase.NamedSource));

        await Assert.That(type).IsEqualTo(typeof(StandaloneBase.NamedSource));
    }

    [Test]
    public async Task GetDataSourceType_NamedSourceNotInHierarchy_ThrowsDataSourceNotFoundException()
    {
        var (factory, rr) = BuildFactory();

        var servedType = rr.GetClassViewModel<StandaloneDerived>()!;
        var declaredFor = rr.GetClassViewModel<StandaloneDerived>()!;

        await Assert.That(() => factory.GetDataSourceType(servedType, declaredFor, "NonExistentSource"))
            .Throws<DataSourceNotFoundException>();
    }

    [Test]
    public async Task GetDefaultDataSourceType_DerivedWithOwnDefault_OwnDefaultTakesPriority()
    {
        var (factory, rr) = BuildFactory();

        var servedType = rr.GetClassViewModel<StandaloneDerivedWithOwnDefault>()!;
        var declaredFor = rr.GetClassViewModel<StandaloneDerivedWithOwnDefault>()!;

        var type = factory.GetDataSourceType(servedType, declaredFor, null);

        await Assert.That(type).IsEqualTo(typeof(StandaloneDerivedWithOwnDefault.DerivedDefaultSource));
    }

    [Test]
    public async Task GetDataSourceType_DerivedWithOwnNamed_BaseNamedSourceAlsoResolvable()
    {
        var (factory, rr) = BuildFactory();

        var servedType = rr.GetClassViewModel<StandaloneDerivedWithOwnNamed>()!;
        var declaredFor = rr.GetClassViewModel<StandaloneDerivedWithOwnNamed>()!;

        // NamedSource is on the base type; should still be found for derived type
        var type = factory.GetDataSourceType(servedType, declaredFor, nameof(StandaloneBase.NamedSource));

        await Assert.That(type).IsEqualTo(typeof(StandaloneBase.NamedSource));
    }
}
