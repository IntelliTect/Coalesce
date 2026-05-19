using IntelliTect.Coalesce.Api.DataSources;
using IntelliTect.Coalesce.Testing.TargetClasses;
using IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Testing.Util;
using IntelliTect.Coalesce.TypeDefinition;
using Microsoft.Extensions.DependencyInjection;

namespace IntelliTect.Coalesce.Tests.Api.DataSources;

public class DataSourceFactoryTests
{
    private DataSourceFactory CreateFactory()
    {
        var services = new ServiceCollection();
        services.AddDbContext<AppDbContext>();
        services.AddCoalesce(b => b.AddContext<AppDbContext>());
        var sp = services.BuildServiceProvider();
        return new DataSourceFactory(sp, ReflectionRepositoryFactory.Reflection);
    }

    [Test]
    public async Task GetDataSource_OpenGenericDefault_ReturnsClosedGenericForDerivedType()
    {
        var factory = CreateFactory();
        var repo = ReflectionRepositoryFactory.Reflection;

        // AbstractImpl2 has no overrides, so it inherits the open generic default.
        var impl2Vm = repo.GetClassViewModel<AbstractImpl2>()!;
        var dataSource = factory.GetDefaultDataSource(impl2Vm, impl2Vm);

        await Assert.That(dataSource).IsAssignableTo<IDataSource<AbstractImpl2>>();
        await Assert.That(dataSource.GetType().GetGenericArguments().Single()).IsEqualTo(typeof(AbstractImpl2));
        await Assert.That(dataSource.GetType().GetGenericTypeDefinition()).IsEqualTo(typeof(AbstractModel.DefaultAbstractModelDataSource<AbstractModel>).GetGenericTypeDefinition());
    }

    [Test]
    public async Task GetDataSource_OpenGenericNamed_ReturnsClosedGenericForDerivedType()
    {
        var factory = CreateFactory();
        var repo = ReflectionRepositoryFactory.Reflection;

        var impl2Vm = repo.GetClassViewModel<AbstractImpl2>()!;
        var dataSource = factory.GetDataSource(impl2Vm, impl2Vm, "AbstractModelDataSource");

        await Assert.That(dataSource).IsAssignableTo<IDataSource<AbstractImpl2>>();
        await Assert.That(dataSource.GetType().GetGenericArguments().Single()).IsEqualTo(typeof(AbstractImpl2));
        await Assert.That(dataSource.GetType().GetGenericTypeDefinition()).IsEqualTo(typeof(AbstractModel.AbstractModelDataSource<AbstractModel>).GetGenericTypeDefinition());
    }

    [Test]
    public async Task GetDataSource_OpenGenericForBaseType_ReturnsClosedGenericForBaseType()
    {
        var factory = CreateFactory();
        var repo = ReflectionRepositoryFactory.Reflection;

        var abstractVm = repo.GetClassViewModel<AbstractModel>()!;
        var dataSource = factory.GetDataSource(abstractVm, abstractVm, "AbstractModelDataSource");

        await Assert.That(dataSource).IsAssignableTo<IDataSource<AbstractModel>>();
        await Assert.That(dataSource.GetType().GetGenericArguments().Single()).IsEqualTo(typeof(AbstractModel));
    }

    [Test]
    public async Task GetDataSource_DerivedTypeDefaultOverride_TakesPrecedenceOverInheritedOpenGeneric()
    {
        var factory = CreateFactory();
        var repo = ReflectionRepositoryFactory.Reflection;

        var impl1Vm = repo.GetClassViewModel<AbstractImpl1>()!;
        var dataSource = factory.GetDefaultDataSource(impl1Vm, impl1Vm);

        // AbstractImpl1 declares its own Impl1DefaultDataSource, which should take precedence
        // over the inherited open generic DefaultAbstractModelDataSource<T>.
        await Assert.That(dataSource).IsAssignableTo<IDataSource<AbstractImpl1>>();
        await Assert.That(dataSource.GetType()).IsEqualTo(typeof(AbstractImpl1.Impl1DefaultDataSource));
    }

    [Test]
    public async Task GetDataSource_DerivedTypeNamedOverride_TakesPrecedenceOverInheritedOpenGeneric()
    {
        var factory = CreateFactory();
        var repo = ReflectionRepositoryFactory.Reflection;

        var impl1Vm = repo.GetClassViewModel<AbstractImpl1>()!;
        var dataSource = factory.GetDataSource(impl1Vm, impl1Vm, "AbstractModelDataSource");

        // AbstractImpl1 declares its own non-generic AbstractModelDataSource with the same name,
        // which should take precedence over the base class's open generic AbstractModelDataSource<T>.
        await Assert.That(dataSource).IsAssignableTo<IDataSource<AbstractImpl1>>();
        await Assert.That(dataSource.GetType()).IsEqualTo(typeof(AbstractImpl1.AbstractModelDataSource));
    }

    [Test]
    public async Task GetDataSource_DerivedTypeWithOverride_StillInheritsOtherNamedDataSources()
    {
        var factory = CreateFactory();
        var repo = ReflectionRepositoryFactory.Reflection;

        // AbstractImpl1 overrides "AbstractModelDataSource" and the default,
        // but should still inherit "TopLevelAbstractModelDataSource" from the base.
        var impl1Vm = repo.GetClassViewModel<AbstractImpl1>()!;
        var dataSource = factory.GetDataSource(impl1Vm, impl1Vm, "TopLevelAbstractModelDataSource");

        await Assert.That(dataSource).IsAssignableTo<IDataSource<AbstractImpl1>>();
        await Assert.That(dataSource.GetType().GetGenericArguments().Single()).IsEqualTo(typeof(AbstractImpl1));
        await Assert.That(dataSource.GetType().GetGenericTypeDefinition()).IsEqualTo(typeof(TopLevelAbstractModelDataSource<AbstractModel>).GetGenericTypeDefinition());
    }
}
