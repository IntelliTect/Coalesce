using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Tests.Util;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xunit;

namespace IntelliTect.Coalesce.Tests.TypeDefinition
{
    public class TypeViewModelTests
    {
        [Theory]
        [Description("https://github.com/IntelliTect/Coalesce/issues/28")]
        // With class name as a substring in a namespace.
        [ClassViewModelData(
            typeof(ComplexModel),
            "MyTestProject.Web.Models", nameof(ComplexModel.Tests),
            "System.Collections.Generic.ICollection<MyTestProject.Web.Models.TestDtoGen>")]
        [ClassViewModelData(
            typeof(ComplexModel),
            "MyTestProject.Web.Models", nameof(ComplexModel.SingleTest),
            "MyTestProject.Web.Models.TestDtoGen")]

        // With class name as a distinct namespace.
        [ClassViewModelData(
            typeof(ComplexModel),
            "MyProject.Test.Models", nameof(ComplexModel.Tests),
            "System.Collections.Generic.ICollection<MyProject.Test.Models.TestDtoGen>")]
        [ClassViewModelData(
            typeof(ComplexModel),
            "MyProject.Test.Models", nameof(ComplexModel.SingleTest),
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

        [Theory]
        [ClassViewModelData(typeof(decimal))]
        [ClassViewModelData(typeof(decimal?))]
        [ClassViewModelData(typeof(int))]
        [ClassViewModelData(typeof(int?))]
        [ClassViewModelData(typeof(short))]
        [ClassViewModelData(typeof(short?))]
        [ClassViewModelData(typeof(double))]
        [ClassViewModelData(typeof(double?))]
        public void IsNumber_TrueForNumbers(ClassViewModelData data)
        {
            TypeViewModel vm = data;

            Assert.True(vm.IsNumber);
        }
    }
}
