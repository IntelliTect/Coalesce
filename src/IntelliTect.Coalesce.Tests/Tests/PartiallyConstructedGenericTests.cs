using IntelliTect.Coalesce.Api.Behaviors;
using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Testing.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace IntelliTect.Coalesce.Tests.Tests;

public class PartiallyConstructedDataSource<T> : StandardDataSource<T, AppDbContext>
    where T : class
{
    public PartiallyConstructedDataSource(CrudContext<AppDbContext> context) : base(context)
    {
    }
}

public class PartiallyConstructedBehaviors<T> : StandardBehaviors<T, AppDbContext>
    where T : class
{
    public PartiallyConstructedBehaviors(CrudContext<AppDbContext> context) : base(context)
    {
    }
}

public class PartiallyConstructedGenericTests
{
    [Test]
    public async Task UseDefaultDataSource_PartiallyConstructedGeneric_RegistersSuccessfully()
    {
        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>();
        services.AddCoalesce(b =>
        {
            b.AddContext<AppDbContext>();
            b.UseDefaultDataSource(typeof(PartiallyConstructedDataSource<>));
        });

        var sp = services.BuildServiceProvider();

        var overrides = sp.GetRequiredService<DefaultCrudStrategyOverrides>();
        await Assert.That(overrides.TryGet(typeof(IEntityFrameworkDataSource<,>), out var implType)).IsTrue();
        await Assert.That(implType).IsEqualTo(typeof(PartiallyConstructedDataSource<>));
    }

    [Test]
    public async Task UseDefaultBehaviors_PartiallyConstructedGeneric_RegistersSuccessfully()
    {
        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>();
        services.AddCoalesce(b =>
        {
            b.AddContext<AppDbContext>();
            b.UseDefaultBehaviors(typeof(PartiallyConstructedBehaviors<>));
        });

        var sp = services.BuildServiceProvider();

        var overrides = sp.GetRequiredService<DefaultCrudStrategyOverrides>();
        await Assert.That(overrides.TryGet(typeof(IEntityFrameworkBehaviors<,>), out var implType)).IsTrue();
        await Assert.That(implType).IsEqualTo(typeof(PartiallyConstructedBehaviors<>));
    }

    [Test]
    public async Task UseDefaultDataSource_MatchingArity_StillWorksViaDI()
    {
        // Verify that matching arity types still register with DI as before.
        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>();
        services.AddCoalesce(b =>
        {
            b.AddContext<AppDbContext>();
            b.UseDefaultDataSource(typeof(StandardDataSource<,>));
        });

        // If it doesn't throw, registration succeeded
        var sp = services.BuildServiceProvider();
        await Assert.That(sp.GetRequiredService<DefaultCrudStrategyOverrides>()).IsNotNull();
    }

    [Test]
    public async Task CloseImplementationType_MatchingArity_ClosesDirectly()
    {
        var closed = DefaultCrudStrategyOverrides.CloseImplementationType(
            typeof(StandardDataSource<,>),
            typeof(IEntityFrameworkDataSource<,>),
            [typeof(Person), typeof(AppDbContext)]);

        await Assert.That(closed).IsEqualTo(typeof(StandardDataSource<Person, AppDbContext>));
    }

    [Test]
    public async Task CloseImplementationType_PartiallyConstructedGeneric_ClosesCorrectly()
    {
        var closed = DefaultCrudStrategyOverrides.CloseImplementationType(
            typeof(PartiallyConstructedDataSource<>),
            typeof(IEntityFrameworkDataSource<,>),
            [typeof(Person), typeof(AppDbContext)]);

        await Assert.That(closed).IsEqualTo(typeof(PartiallyConstructedDataSource<Person>));
    }

    [Test]
    public async Task CloseImplementationType_PartiallyConstructedBehaviors_ClosesCorrectly()
    {
        var closed = DefaultCrudStrategyOverrides.CloseImplementationType(
            typeof(PartiallyConstructedBehaviors<>),
            typeof(IEntityFrameworkBehaviors<,>),
            [typeof(Person), typeof(AppDbContext)]);

        await Assert.That(closed).IsEqualTo(typeof(PartiallyConstructedBehaviors<Person>));
    }

    [Test]
    public async Task DataSourceFactory_PartiallyConstructedGeneric_ResolvesCorrectType()
    {
        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>();
        services.AddCoalesce(b =>
        {
            b.AddContext<AppDbContext>();
            b.UseDefaultDataSource(typeof(PartiallyConstructedDataSource<>));
        });

        var sp = services.BuildServiceProvider();
        var factory = sp.GetRequiredService<IDataSourceFactory>();

        // Use Company because it has no custom data source overrides
        var companyCvm = ReflectionRepositoryFactory.Reflection.GetClassViewModel<Company>()!;

        var dataSource = factory.GetDefaultDataSource(companyCvm, companyCvm);
        await Assert.That(dataSource).IsTypeOf<PartiallyConstructedDataSource<Company>>();
    }

    [Test]
    public async Task BehaviorsFactory_PartiallyConstructedGeneric_ResolvesCorrectType()
    {
        var services = new ServiceCollection();

        services.AddDbContext<AppDbContext>();
        services.AddCoalesce(b =>
        {
            b.AddContext<AppDbContext>();
            b.UseDefaultBehaviors(typeof(PartiallyConstructedBehaviors<>));
        });

        var sp = services.BuildServiceProvider();
        var factory = sp.GetRequiredService<IBehaviorsFactory>();

        var companyCvm = ReflectionRepositoryFactory.Reflection.GetClassViewModel<Company>()!;

        var behaviors = factory.GetDefaultBehaviors(companyCvm);
        await Assert.That(behaviors).IsTypeOf<PartiallyConstructedBehaviors<Company>>();
    }
}
