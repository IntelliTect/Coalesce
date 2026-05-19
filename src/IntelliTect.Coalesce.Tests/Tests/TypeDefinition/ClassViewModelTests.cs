using IntelliTect.Coalesce.Testing.TargetClasses;
using IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Testing.Util;
using IntelliTect.Coalesce.TypeDefinition;

namespace IntelliTect.Coalesce.Tests.TypeDefinition;

public class ClassViewModelTests
{
    [Test]
    [ClassViewModelData(typeof(Testing.TargetClasses.OrderingChild))]
    public async Task DefaultOrderBy_UsesNestedPropertiesWhenOrderingByRefNavigation(ClassViewModelData data)
    {
        await data.ClassViewModel.DefaultOrderBy.AssertCollection(
            async ordering =>
            {
                await Assert.That(ordering.FieldOrder).IsEqualTo(1);
                await Assert.That(ordering.Properties[0].Name).IsEqualTo("OrderingParent1");
                await Assert.That(ordering.Properties[1].Name).IsEqualTo("OrderingGrandparent");
                await Assert.That(ordering.Properties[2].Name).IsEqualTo("OrderedField");
            },
            async ordering =>
            {
                await Assert.That(ordering.FieldOrder).IsEqualTo(2);
                await Assert.That(ordering.Properties[0].Name).IsEqualTo("OrderingParent2");
                await Assert.That(ordering.Properties[1].Name).IsEqualTo("OrderingGrandparent");
                await Assert.That(ordering.Properties[2].Name).IsEqualTo("OrderedField");
            }
        );
    }

    [Test]
    [ClassViewModelData(typeof(Testing.TargetClasses.OrdersByUnorderableParent))]
    public async Task DefaultOrderBy_UsesNavigationDirectlyWhenOrderingByUnorderableRefNavigation(ClassViewModelData data)
    {
        await data.ClassViewModel.DefaultOrderBy.AssertCollection(
            async ordering =>
            {
                await Assert.That(ordering.Properties).Count().IsEqualTo(1);
                await Assert.That(ordering.Properties[0].Name).IsEqualTo("Parent");
            }
        );
    }

    [Test]
    [ClassViewModelData(typeof(AbstractImpl1))]
    public async Task GetAttribute_RespectsInheritance(ClassViewModelData data)
    {
        var vm = data.ClassViewModel;

        // The read and create attributes are declared on AbstractImpl's base class,
        // but should still be surfaced on AbstractImpl because they are inherited attributes.
        await Assert.That(vm.SecurityInfo.Read.Roles).IsEqualTo("ReadRole");
        await Assert.That(vm.SecurityInfo.Create.NoAccess).IsTrue();

        // Edit role is defined on both, so the one on AbstractImpl should be the effective one:
        await Assert.That(vm.SecurityInfo.Edit.NoAccess).IsTrue();
    }

    [Test]
    [ClassViewModelData(typeof(ComplexModel))]
    public async Task ClientConsts_IncludesConsts(ClassViewModelData data)
    {
        var vm = data.ClassViewModel;

        var consts = vm.ClientConsts;
        await Assert.That(consts).Contains(c => c.Name == nameof(ComplexModel.MagicNumber) && c.Value.Equals(ComplexModel.MagicNumber));
        await Assert.That(consts).Contains(c => c.Name == nameof(ComplexModel.MagicString) && c.Value.Equals(ComplexModel.MagicString));
        await Assert.That(consts).Contains(c => c.Name == nameof(ComplexModel.MagicEnum) && ((int)c.Value).Equals((int)ComplexModel.MagicEnum));
        await Assert.That(consts).DoesNotContain(c => c.Name == nameof(ComplexModel.UnexpostedConst));
    }

    [Test]
    [ClassViewModelData(typeof(Testing.TargetClasses.SuppressedDefaultOrdering))]
    public async Task DefaultOrderBy_SuppressesFallbackWhenSpecified(ClassViewModelData data)
    {
        var orderings = data.ClassViewModel.DefaultOrderBy;

        // Should be empty - no fallback ordering should be applied
        await Assert.That(orderings).IsEmpty();
    }

    [Test]
    public async Task ClientDataSources_ExcludesAbstractNestedDataSources()
    {
        var repo = ReflectionRepositoryFactory.Reflection;
        var entityVm = repo.GetClassViewModel<Case>()!;

        var dataSources = entityVm.ClientDataSources(repo).ToList();

        // The concrete AllOpenCases should be discovered
        await Assert.That(dataSources).Contains(ds => ds.Name == nameof(Case.AllOpenCases));

        // The abstract AbstractCaseDataSource should NOT be discovered
        await Assert.That(dataSources).DoesNotContain(ds => ds.Name == nameof(Case.AbstractCaseDataSource));
    }

    [Test]
    public async Task ClientDataSources_OpenGenericConstrainedToType_DiscoveredForBaseType()
    {
        var repo = ReflectionRepositoryFactory.Reflection;

        // GenericCaseDataSource<T> where T : Case is nested in Case and should appear for Case itself.
        var caseVm = repo.GetClassViewModel<Case>()!;
        var dataSources = caseVm.ClientDataSources(repo).ToList();
        await Assert.That(dataSources).Contains(ds => ds.Name == "GenericCaseDataSource");

        // AbstractModelDataSource<T> where T : AbstractModel should appear for AbstractModel.
        var abstractVm = repo.GetClassViewModel<AbstractModel>()!;
        var abstractDs = abstractVm.ClientDataSources(repo).ToList();
        await Assert.That(abstractDs).Contains(ds => ds.Name == "AbstractModelDataSource");
        await Assert.That(abstractDs).Contains(ds => ds.Name == "DefaultAbstractModelDataSource");
    }

    [Test]
    public async Task ClientDataSources_OpenGenericConstrainedToBase_DiscoveredForDerivedTypes()
    {
        var repo = ReflectionRepositoryFactory.Reflection;

        // AbstractModelDataSource<T> and DefaultAbstractModelDataSource<T> are nested in AbstractModel.
        // They should be available for derived types that don't override them.
        // AbstractImpl1 overrides both, so use AbstractImpl2 which has no overrides.
        var impl2Vm = repo.GetClassViewModel<AbstractImpl2>()!;
        var impl2Sources = impl2Vm.ClientDataSources(repo).ToList();
        await Assert.That(impl2Sources).Contains(ds => ds.Name == "AbstractModelDataSource");
        await Assert.That(impl2Sources).Contains(ds => ds.Name == "DefaultAbstractModelDataSource");
    }

    [Test]
    public async Task ClientDataSources_OpenGenericDefault_IsDefaultForDerivedType()
    {
        var repo = ReflectionRepositoryFactory.Reflection;

        // AbstractImpl2 has no overrides, so it inherits the open generic default.
        var impl2Vm = repo.GetClassViewModel<AbstractImpl2>()!;
        var defaultSource = impl2Vm.ClientDataSources(repo).SingleOrDefault(ds => ds.IsDefaultDataSource);

        await Assert.That(defaultSource).IsNotNull();
        await Assert.That(defaultSource!.Name).IsEqualTo("DefaultAbstractModelDataSource");
    }

    [Test]
    public async Task ClientDataSources_OpenGenericDerivedType_ClosedWithDerivedType()
    {
        var repo = ReflectionRepositoryFactory.Reflection;

        // The closed generic type should be AbstractModel.AbstractModelDataSource<AbstractImpl2>,
        // not AbstractModel.AbstractModelDataSource<AbstractModel>.
        var impl2Vm = repo.GetClassViewModel<AbstractImpl2>()!;
        var impl2Sources = impl2Vm.ClientDataSources(repo).ToList();

        var ds = impl2Sources.First(ds => ds.Name == "AbstractModelDataSource");
        var typeArg = ds.Type.GenericArgumentsFor(typeof(IDataSource<>))?.Single();
        await Assert.That(typeArg?.ClassViewModel?.Name).IsEqualTo(nameof(AbstractImpl2));
    }

    [Test]
    public async Task ClientDataSources_TopLevelOpenGeneric_DiscoveredForConstraintAndDerivedTypes()
    {
        var repo = ReflectionRepositoryFactory.Reflection;

        // TopLevelAbstractModelDataSource<T> where T : AbstractModel is decorated with [Coalesce]
        // and should be discovered for AbstractModel and all derived types.
        var abstractVm = repo.GetClassViewModel<AbstractModel>()!;
        var abstractSources = abstractVm.ClientDataSources(repo).ToList();
        await Assert.That(abstractSources).Contains(ds => ds.Name == "TopLevelAbstractModelDataSource");

        var impl1Vm = repo.GetClassViewModel<AbstractImpl1>()!;
        var impl1Sources = impl1Vm.ClientDataSources(repo).ToList();
        await Assert.That(impl1Sources).Contains(ds => ds.Name == "TopLevelAbstractModelDataSource");

        var impl2Vm = repo.GetClassViewModel<AbstractImpl2>()!;
        var impl2Sources = impl2Vm.ClientDataSources(repo).ToList();
        await Assert.That(impl2Sources).Contains(ds => ds.Name == "TopLevelAbstractModelDataSource");
    }
}
