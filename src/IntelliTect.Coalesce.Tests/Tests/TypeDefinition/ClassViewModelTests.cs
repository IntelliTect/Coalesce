using IntelliTect.Coalesce.Testing.TargetClasses;
using IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Testing.Util;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Tests.TypeDefinition;

public class ClassViewModelTests
{
    [Test]
    [ClassViewModelData(typeof(Testing.TargetClasses.OrderingChild))]
    public async Task DefaultOrderBy_UsesNestedPropertiesWhenOrderingByRefNavigation(ClassViewModelData data)
    {
        var orderings = data.ClassViewModel.DefaultOrderBy;
        // TODO: TUnit migration - Assert.Collection had element inspectors. Manually add assertions for each element.
        await Assert.That(orderings).Count().IsEqualTo(2);
    }

    [Test]
    [ClassViewModelData(typeof(Testing.TargetClasses.OrdersByUnorderableParent))]
    public async Task DefaultOrderBy_UsesNavigationDirectlyWhenOrderingByUnorderableRefNavigation(ClassViewModelData data)
    {
        var orderings = data.ClassViewModel.DefaultOrderBy;
        // TODO: TUnit migration - Assert.Collection had element inspectors. Manually add assertions for each element.
        await Assert.That(orderings).Count().IsEqualTo(1);
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
}
