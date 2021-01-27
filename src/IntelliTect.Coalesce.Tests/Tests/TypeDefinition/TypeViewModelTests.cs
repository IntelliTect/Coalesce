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


        // SymbolClassViewModelData here is being used because
        // we don't ever use this method in reflection contexts.
        // It does seem to actually output correct type for Reflection, but it emits
        // "System.Int32" instead of "int", among other things, failing the assertions.
        [Theory]
        [SymbolClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.ValueArray), "int[]")]
        [SymbolClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.ValueNullableArray), "int?[]")]
        [SymbolClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.ValueArrayNullable), "int[]")]
        [SymbolClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.ValueICollection), "System.Collections.Generic.ICollection<int>")]
        [SymbolClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.ValueNullableICollection), "System.Collections.Generic.ICollection<int?>")]
        [SymbolClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.ValueICollectionNullable), "System.Collections.Generic.ICollection<int>")]
        [SymbolClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.RefArray), "MyProject.ExternalChildDtoGen[]")]
        [SymbolClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.RefNullableArray), "MyProject.ExternalChildDtoGen[]")]
        [SymbolClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.RefArrayNullable), "MyProject.ExternalChildDtoGen[]")]
        [SymbolClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.RefICollection), "System.Collections.Generic.ICollection<MyProject.ExternalChildDtoGen>")]
        [SymbolClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.RefNullableICollection), "System.Collections.Generic.ICollection<MyProject.ExternalChildDtoGen>")]
        [SymbolClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.RefICollectionNullable), "System.Collections.Generic.ICollection<MyProject.ExternalChildDtoGen>")]
        public void NullableTypeForDto_HandlesCollectionsProperly(
            ClassViewModelData data,
            string propertyName,
            string expectedPropertyType
        )
        {
            ClassViewModel vm = data;

            var prop = vm.PropertyByName(propertyName);

            var dtoPropType = prop.Type.NullableTypeForDto("MyProject");

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
