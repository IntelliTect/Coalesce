using IntelliTect.Coalesce.Api;
using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.Extensions.DependencyInjection;

#nullable enable

namespace IntelliTect.Coalesce.Tests.Api.DataSources;

public class DataSourceFactoryTests
{
    private (DataSourceFactory Factory, ReflectionRepository Repo) BuildFactory(System.Security.Claims.ClaimsPrincipal? user = null)
    {
        var testUser = user ?? new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity("test"));

        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>();
        services.AddCoalesce(b => b.AddContext<AppDbContext>());
        // Override CrudContext with a test-friendly registration (no IHttpContextAccessor needed)
        services.AddScoped(_ => new CrudContext(() => testUser));
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
        public string? Name { get; set; }

        [DefaultDataSource]
        public class DefaultSource : StandardDataSource<StandaloneBase>
        {
            public DefaultSource(CrudContext ctx) : base(ctx) { }
            public override Task<IQueryable<StandaloneBase>> GetQueryAsync(IDataSourceParameters parameters)
                => Task.FromResult(TestData.AsQueryable());
        }

        public class NamedSource : StandardDataSource<StandaloneBase>
        {
            public NamedSource(CrudContext ctx) : base(ctx) { }
            public override Task<IQueryable<StandaloneBase>> GetQueryAsync(IDataSourceParameters parameters)
                => Task.FromResult(TestData.AsQueryable());
        }

        public static IEnumerable<StandaloneBase> TestData =>
        [
            new StandaloneDerived { Id = 1, Name = "Derived1", DerivedProp = "D1" },
            new StandaloneDerived { Id = 2, Name = "Derived2", DerivedProp = "D2" },
            new StandaloneBase { Id = 3, Name = "Base3" },
        ];
    }

    [Coalesce, StandaloneEntity]
    public class StandaloneDerived : StandaloneBase
    {
        public string? DerivedProp { get; set; }
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

    public class StandaloneBaseDto : IResponseDto<StandaloneBase>
    {
        public int Id { get; set; }
        public string? Name { get; set; }

        public void MapFrom(StandaloneBase obj, IMappingContext context, IncludeTree? tree = null)
        {
            if (obj == null) return;
            switch (this)
            {
                case StandaloneDerivedDto derived:
                    derived.MapFrom((StandaloneDerived)obj, context, tree);
                    return;
            }
            Id = obj.Id;
            Name = obj.Name;
        }
    }

    public class StandaloneDerivedDto : StandaloneBaseDto, IResponseDto<StandaloneDerived>
    {
        public string? DerivedProp { get; set; }

        public void MapFrom(StandaloneDerived obj, IMappingContext context, IncludeTree? tree = null)
        {
            Id = obj.Id;
            Name = obj.Name;
            DerivedProp = obj.DerivedProp;
        }
    }

    [Coalesce, StandaloneEntity]
    [Read("Admin")]
    public class SecuredBase
    {
        public int Id { get; set; }

        public class SecuredBaseSource : StandardDataSource<SecuredBase>
        {
            public SecuredBaseSource(CrudContext ctx) : base(ctx) { }
            public override Task<IQueryable<SecuredBase>> GetQueryAsync(IDataSourceParameters parameters)
                => Task.FromResult(new SecuredBase[] { new SecuredDerived { Id = 1 } }.AsQueryable());
        }
    }

    [Coalesce, StandaloneEntity]
    public class SecuredDerived : SecuredBase
    {
        // No own data sources - inherits from SecuredBase which requires "Admin" role
    }

    #endregion

    #region Type Resolution Tests

    [Test]
    public async Task GetDefaultDataSourceType_DerivedTypeWithNoSources_Throws()
    {
        var (factory, rr) = BuildFactory();

        var servedType = rr.GetClassViewModel<StandaloneDerived>()!;
        var declaredFor = rr.GetClassViewModel<StandaloneDerived>()!;

        await Assert.That(() => factory.GetDataSourceType(servedType, declaredFor, null))
            .Throws<ArgumentException>();
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

        var type = factory.GetDataSourceType(servedType, declaredFor, nameof(StandaloneBase.NamedSource));

        await Assert.That(type).IsEqualTo(typeof(StandaloneBase.NamedSource));
    }

    #endregion

    #region Adapter Wrapping Tests

    [Test]
    public async Task GetDataSource_DerivedType_ReturnsAdaptedDataSource()
    {
        var (factory, rr) = BuildFactory();
        var declaredFor = rr.GetClassViewModel<StandaloneDerived>()!;

        var dataSource = factory.GetDataSource<StandaloneDerived>(declaredFor, nameof(StandaloneBase.NamedSource));

        await Assert.That(dataSource).IsNotNull();
        await Assert.That(dataSource).IsTypeOf<InheritedDataSourceAdapter<StandaloneDerived, StandaloneBase>>();
    }

    [Test]
    public async Task GetDataSource_DerivedTypeWithOwnSource_DoesNotWrap()
    {
        var (factory, rr) = BuildFactory();
        var declaredFor = rr.GetClassViewModel<StandaloneDerivedWithOwnDefault>()!;

        var dataSource = factory.GetDataSource<StandaloneDerivedWithOwnDefault>(declaredFor, null);

        await Assert.That(dataSource).IsTypeOf<StandaloneDerivedWithOwnDefault.DerivedDefaultSource>();
    }

    #endregion

    #region Adapter GetItemAsync Tests

    [Test]
    public async Task GetItemAsync_AdaptedSource_ReturnsDerivedItem()
    {
        var (factory, rr) = BuildFactory();
        var declaredFor = rr.GetClassViewModel<StandaloneDerived>()!;

        var dataSource = factory.GetDataSource<StandaloneDerived>(declaredFor, nameof(StandaloneBase.NamedSource));
        var result = await dataSource.GetItemAsync(1, new DataSourceParameters());

        await Assert.That(result.WasSuccessful).IsTrue();
        await Assert.That(result.Object).IsNotNull();
        await Assert.That(result.Object!.Id).IsEqualTo(1);
        await Assert.That(result.Object).IsTypeOf<StandaloneDerived>();
        await Assert.That(((StandaloneDerived)result.Object!).DerivedProp).IsEqualTo("D1");
    }

    [Test]
    public async Task GetItemAsync_AdaptedSource_BaseOnlyItemReturnsFailure()
    {
        var (factory, rr) = BuildFactory();
        var declaredFor = rr.GetClassViewModel<StandaloneDerived>()!;

        var dataSource = factory.GetDataSource<StandaloneDerived>(declaredFor, nameof(StandaloneBase.NamedSource));
        // Id 3 is a StandaloneBase (not StandaloneDerived)
        var result = await dataSource.GetItemAsync(3, new DataSourceParameters());

        await Assert.That(result.WasSuccessful).IsFalse();
        await Assert.That(result.Message).IsNotNull();
    }

    [Test]
    public async Task GetMappedItemAsync_AdaptedSource_MapsDerivedItem()
    {
        var (factory, rr) = BuildFactory();
        var declaredFor = rr.GetClassViewModel<StandaloneDerived>()!;

        var dataSource = factory.GetDataSource<StandaloneDerived>(declaredFor, nameof(StandaloneBase.NamedSource));
        var result = await dataSource.GetMappedItemAsync<StandaloneDerivedDto>(1, new DataSourceParameters());

        await Assert.That(result.WasSuccessful).IsTrue();
        await Assert.That(result.Object).IsNotNull();
        await Assert.That(result.Object!.Id).IsEqualTo(1);
        await Assert.That(result.Object!.Name).IsEqualTo("Derived1");
        await Assert.That(result.Object!.DerivedProp).IsEqualTo("D1");
    }

    #endregion

    #region Adapter List/Count Operations Throw

    [Test]
    public async Task GetListAsync_AdaptedSource_ThrowsNotSupported()
    {
        var (factory, rr) = BuildFactory();
        var declaredFor = rr.GetClassViewModel<StandaloneDerived>()!;

        var dataSource = factory.GetDataSource<StandaloneDerived>(declaredFor, nameof(StandaloneBase.NamedSource));

        await Assert.ThrowsAsync<NotSupportedException>(() => dataSource.GetListAsync(new ListParameters()));
    }

    [Test]
    public async Task GetMappedListAsync_AdaptedSource_ThrowsNotSupported()
    {
        var (factory, rr) = BuildFactory();
        var declaredFor = rr.GetClassViewModel<StandaloneDerived>()!;

        var dataSource = factory.GetDataSource<StandaloneDerived>(declaredFor, nameof(StandaloneBase.NamedSource));

        await Assert.ThrowsAsync<NotSupportedException>(() => dataSource.GetMappedListAsync<StandaloneDerivedDto>(new ListParameters()));
    }

    [Test]
    public async Task GetCountAsync_AdaptedSource_ThrowsNotSupported()
    {
        var (factory, rr) = BuildFactory();
        var declaredFor = rr.GetClassViewModel<StandaloneDerived>()!;

        var dataSource = factory.GetDataSource<StandaloneDerived>(declaredFor, nameof(StandaloneBase.NamedSource));

        await Assert.ThrowsAsync<NotSupportedException>(() => dataSource.GetCountAsync(new FilterParameters()));
    }

    #endregion

    #region Adapter Security Tests

    [Test]
    public async Task GetItemAsync_AdaptedSource_UserWithBaseTypeRole_Succeeds()
    {
        var user = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(
                [new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Admin")],
                "test"));
        var (factory, rr) = BuildFactory(user);
        var declaredFor = rr.GetClassViewModel<SecuredDerived>()!;

        var dataSource = factory.GetDataSource<SecuredDerived>(declaredFor, nameof(SecuredBase.SecuredBaseSource));
        var result = await dataSource.GetItemAsync(1, new DataSourceParameters());

        await Assert.That(result.WasSuccessful).IsTrue();
    }

    [Test]
    public async Task GetItemAsync_AdaptedSource_UserWithoutBaseTypeRole_Denied()
    {
        // User is authenticated but does NOT have "Admin" role
        var user = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity("test"));
        var (factory, rr) = BuildFactory(user);
        var declaredFor = rr.GetClassViewModel<SecuredDerived>()!;

        var dataSource = factory.GetDataSource<SecuredDerived>(declaredFor, nameof(SecuredBase.SecuredBaseSource));
        var result = await dataSource.GetItemAsync(1, new DataSourceParameters());

        await Assert.That(result.WasSuccessful).IsFalse();
        await Assert.That(result.Message).Contains("not authorized");
    }

    #endregion
}
