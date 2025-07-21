using IntelliTect.Coalesce.Tests.Util;
using IntelliTect.Coalesce.Tests.TargetClasses;
using IntelliTect.Coalesce.TypeDefinition;

namespace IntelliTect.Coalesce.Tests.Tests.Security;

public class ClassSecurityTests
{
    [Theory]
    [ClassViewModelData(typeof(EditWithSpaces))]
    public void WhenRolesHaveSpaces_TrimsRolesCorrectly(ClassViewModelData data)
    {
        ClassViewModel vm = data;

        Assert.Contains("RoleA", vm.SecurityInfo.Edit.RoleList);
        Assert.Contains("RoleB", vm.SecurityInfo.Edit.RoleList);
        Assert.Contains("RoleC", vm.SecurityInfo.Edit.RoleList);
    }

    [Theory]
    [ClassViewModelData(typeof(AbstractModel))]
    public void AbstractEntityWithDbSet_HasReadOnlyApi(ClassViewModelData data)
    {
        // Exposing an API for abstract entities helps support TPT/TPH configurations (#8).
        ClassViewModel vm = data;

        Assert.True(vm.SecurityInfo.Read.IsAllowed());

        Assert.False(vm.SecurityInfo.Edit.IsAllowed());
        Assert.False(vm.SecurityInfo.Create.IsAllowed());

        // In theory delete could be exposed, but doing so would not work with
        // Behaviors-implemented security (it would bypass the security of the concrete type).
        // So, delete can't be done on a base abstract entity.
        Assert.False(vm.SecurityInfo.Delete.IsAllowed());
    }
}
