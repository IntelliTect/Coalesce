using IntelliTect.Coalesce.Tests.Util;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using IntelliTect.Coalesce.Tests.TargetClasses;

namespace IntelliTect.Coalesce.Tests.Tests.Security
{
    public class PropertySecurityTests
    {
        [Theory]
        [ClassViewModelData(typeof(PropSec), nameof(PropSec.ReadOnlyViaRead))]
        [ClassViewModelData(typeof(PropSec), nameof(PropSec.ReadOnlyViaReadOnly))]
        [ClassViewModelData(typeof(PropSec), nameof(PropSec.ReadOnlyViaReadOnlyApi))]
        [ClassViewModelData(typeof(PropSec), nameof(PropSec.ReadOnlyViaNonPublicSetter))]
        public void ReadOnly_CorrectForReadOnlyProps(ClassViewModelData data, string propName)
        {
            ClassViewModel vm = data;
            var prop = vm.PropertyByName(propName);

            Assert.True(prop.IsReadOnly);
            Assert.False(prop.IsClientSerializable);
            Assert.False(prop.IsClientWritable);
        }
    }
}
