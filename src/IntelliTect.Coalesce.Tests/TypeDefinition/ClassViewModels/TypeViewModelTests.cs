using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace IntelliTect.Coalesce.Tests.TypeDefinition.ClassViewModels
{
    public class TypeViewModelTests
    {
        [Theory]
        // With class name as a substring in a namespace.
        [ClassViewModelData(
            typeof(TargetClasses.ComplexModel),
            "MyTestProject.Web.Models", nameof(TargetClasses.ComplexModel.Tests),
            "System.Collections.Generic.ICollection<MyTestProject.Web.Models.TestDtoGen>")]
        [ClassViewModelData(
            typeof(TargetClasses.ComplexModel),
            "MyTestProject.Web.Models", nameof(TargetClasses.ComplexModel.SingleTest),
            "MyTestProject.Web.Models.TestDtoGen")]

        // With class name as a distinct namespace.
        [ClassViewModelData(
            typeof(TargetClasses.ComplexModel),
            "MyProject.Test.Models", nameof(TargetClasses.ComplexModel.Tests),
            "System.Collections.Generic.ICollection<MyProject.Test.Models.TestDtoGen>")]
        [ClassViewModelData(
            typeof(TargetClasses.ComplexModel),
            "MyProject.Test.Models", nameof(TargetClasses.ComplexModel.SingleTest),
            "MyProject.Test.Models.TestDtoGen")]
        public void NullableTypeForDto_DoesNotMangleNamespace(
            ClassViewModelData data,
            string dtoOputputNamespace,
            string propertyName,
            string expectedPropertyType
        )
        {
            ClassViewModel vm = data;

            var prop = vm.PropertyByName(propertyName);

            // Precondition: Model's type name is contained in output namespace:
            Assert.Contains(prop.PureType.Name, dtoOputputNamespace);

            // Precondition: Model's type name is contained in its own namespace:
            Assert.Contains(prop.PureType.Name, prop.PureType.FullNamespace);

            var dtoPropType = prop.Type.NullableTypeForDto(dtoOputputNamespace);

            Assert.Equal(expectedPropertyType, dtoPropType);
        }

    }
}
