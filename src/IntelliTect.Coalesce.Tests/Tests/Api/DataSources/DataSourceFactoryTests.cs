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

        var impl1Vm = repo.GetClassViewModel<AbstractImpl1>()!;
        var dataSource = factory.GetDefaultDataSource(impl1Vm, impl1Vm);

        // The resolved data source should be the closed generic DefaultAbstractModelDataSource<AbstractImpl1>,
        // not some base-type version.
        await Assert.That(dataSource).IsAssignableTo<IDataSource<AbstractImpl1>>();
        await Assert.That(dataSource.GetType().GetGenericArguments().Single()).IsEqualTo(typeof(AbstractImpl1));
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
}
