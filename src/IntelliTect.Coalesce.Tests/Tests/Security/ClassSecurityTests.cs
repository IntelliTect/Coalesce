using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Tests.Util;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using IntelliTect.Coalesce.Tests.TargetClasses;
using IntelliTect.Coalesce.TypeDefinition;

namespace IntelliTect.Coalesce.Tests.Tests.Security
{
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
    }
}
