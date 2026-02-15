using IntelliTect.Coalesce.Testing.Util;
using IntelliTect.Coalesce.Testing.TargetClasses;
using IntelliTect.Coalesce.TypeDefinition;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Tests.Tests.Security;

public class ClassSecurityTests
{
    [Test]
    [ClassViewModelData(typeof(EditWithSpaces))]
    public async Task WhenRolesHaveSpaces_TrimsRolesCorrectly(ClassViewModelData data)
    {
        ClassViewModel vm = data;

        await Assert.That(vm.SecurityInfo.Edit.RoleList).Contains("RoleA");
        await Assert.That(vm.SecurityInfo.Edit.RoleList).Contains("RoleB");
        await Assert.That(vm.SecurityInfo.Edit.RoleList).Contains("RoleC");
    }

    [Test]
    [ClassViewModelData(typeof(AbstractModel))]
    public async Task AbstractEntityWithDbSet_HasReadOnlyApi(ClassViewModelData data)
    {
        // Exposing an API for abstract entities helps support TPT/TPH configurations (#8).
        ClassViewModel vm = data;

        await Assert.That(vm.SecurityInfo.Read.IsAllowed()).IsTrue();

        await Assert.That(vm.SecurityInfo.Edit.IsAllowed()).IsFalse();
        await Assert.That(vm.SecurityInfo.Create.IsAllowed()).IsFalse();

        // In theory delete could be exposed, but doing so would not work with
        // Behaviors-implemented security (it would bypass the security of the concrete type).
        // So, delete can't be done on a base abstract entity.
        await Assert.That(vm.SecurityInfo.Delete.IsAllowed()).IsFalse();
    }
}
