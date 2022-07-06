using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.Tests.TargetClasses;
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
        [ClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.ValueArray), "int[]")]
        [ClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.ValueNullableArray), "int?[]")]
        [ClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.ValueArrayNullable), "int[]")]
        [ClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.ValueICollection), "System.Collections.Generic.ICollection<int>")]
        [ClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.ValueNullableICollection), "System.Collections.Generic.ICollection<int?>")]
        [ClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.ValueICollectionNullable), "System.Collections.Generic.ICollection<int>")]
        [ClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.RefArray), "MyProject.ExternalChildDtoGen[]")]
        [ClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.RefNullableArray), "MyProject.ExternalChildDtoGen[]")]
        [ClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.RefArrayNullable), "MyProject.ExternalChildDtoGen[]")]
        [ClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.RefICollection), "System.Collections.Generic.ICollection<MyProject.ExternalChildDtoGen>")]
        [ClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.RefNullableICollection), "System.Collections.Generic.ICollection<MyProject.ExternalChildDtoGen>")]
        [ClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.RefICollectionNullable), "System.Collections.Generic.ICollection<MyProject.ExternalChildDtoGen>")]
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
        [ClassViewModelData(typeof(void), "void")]
        [ClassViewModelData(typeof(bool), "bool")]
        [ClassViewModelData(typeof(bool?), "bool?")]
        [ClassViewModelData(typeof(bool[]), "bool[]")]
        [ClassViewModelData(typeof(bool[,]), "bool[,]")]
        [ClassViewModelData(typeof(bool?[]), "bool?[]")]
        [ClassViewModelData(typeof(System.Collections.Generic.ICollection<bool>), "System.Collections.Generic.ICollection<bool>")]
        [ClassViewModelData(typeof(System.Collections.Generic.ICollection<bool?>), "System.Collections.Generic.ICollection<bool?>")]
        [ClassViewModelData(typeof(Bools), "IntelliTect.Coalesce.Tests.TargetClasses.Bools")]
        [ClassViewModelData(typeof(System.Collections.Generic.ICollection<ExternalParent>),
            "System.Collections.Generic.ICollection<IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.ExternalParent>")]
        [ClassViewModelData(typeof(IEntityFrameworkBehaviors<Exception, TestDbContext>),
            "IntelliTect.Coalesce.IEntityFrameworkBehaviors<System.Exception, IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.TestDbContext>")]
        [ClassViewModelData(typeof(Case.Statuses), "IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.Case.Statuses")]
        public void FullyQualifiedName_HasCorrectValue(
            ClassViewModelData data,
            string expectedTypeName
        )
        {
            TypeViewModel vm = data;
            var value = vm.FullyQualifiedName;

            Assert.Equal(expectedTypeName, value);
        }

        [Theory]
        [ClassViewModelData(typeof(void), "System.Void")]
        [ClassViewModelData(typeof(bool), "System.Boolean")]
        [ClassViewModelData(typeof(bool?), "System.Nullable<System.Boolean>")]
        [ClassViewModelData(typeof(bool[]), "System.Boolean[]")]
        [ClassViewModelData(typeof(bool[,]), "System.Boolean[,]")]
        [ClassViewModelData(typeof(bool?[]), "System.Nullable<System.Boolean>[]")]
        [ClassViewModelData(typeof(System.Collections.Generic.ICollection<bool>), "System.Collections.Generic.ICollection<System.Boolean>")]
        [ClassViewModelData(typeof(System.Collections.Generic.ICollection<bool?>), "System.Collections.Generic.ICollection<System.Nullable<System.Boolean>>")]
        [ClassViewModelData(typeof(Bools), "IntelliTect.Coalesce.Tests.TargetClasses.Bools")]
        [ClassViewModelData(typeof(System.Collections.Generic.ICollection<ExternalParent>),
            "System.Collections.Generic.ICollection<IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.ExternalParent>")]
        [ClassViewModelData(typeof(IEntityFrameworkBehaviors<Exception, TestDbContext>),
            "IntelliTect.Coalesce.IEntityFrameworkBehaviors<System.Exception, IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.TestDbContext>")]
        public void VerboseFullyQualifiedName_HasCorrectValue(
            ClassViewModelData data,
            string expectedTypeName
        )
        {
            TypeViewModel vm = data;
            var value = vm.VerboseFullyQualifiedName;

            Assert.Equal(expectedTypeName, value);
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

        [Theory]
        // System structs
        [ClassViewModelData(typeof(byte), false)]
        [ClassViewModelData(typeof(byte[]), false)]
        // System classes
        [ClassViewModelData(typeof(string), false)]
        [ClassViewModelData(typeof(string[]), false)]
        [ClassViewModelData(typeof(List<string>), false)]
        // Custom classes
        [ClassViewModelData(typeof(InternalClass), true)]
        [ClassViewModelData(typeof(InternalClass[]), true)]
        [ClassViewModelData(typeof(List<InternalClass>), true)]
        [ClassViewModelData(typeof(InternalUseClass), true)]
        [ClassViewModelData(typeof(InternalUseClass[]), true)]
        [ClassViewModelData(typeof(List<InternalClass>), true)]
        public void IsInternalUse_IsCorrectForAllTypeCombinations(ClassViewModelData data, bool isInternalUse)
        {
            TypeViewModel vm = data;
            Assert.Equal(isInternalUse, vm.IsInternalUse);
        }

        [Theory]
        [ClassViewModelData(typeof(int), false)]
        [ClassViewModelData(typeof(int?), false)]
        [ClassViewModelData(typeof(IWeatherService), true)]
        [ClassViewModelData(typeof(WeatherService), false)]
        public void IsInterface_IsCorrect(ClassViewModelData data, bool expected)
            => Assert.Equal(expected, ((TypeViewModel)data).IsInterface);

        [Theory]
        [ClassViewModelData(typeof(int), false)]
        [ClassViewModelData(typeof(int?), false)]
        [ClassViewModelData(typeof(IWeatherService), false)]
        [ClassViewModelData(typeof(WeatherService), true)]
        public void IsClass_IsCorrect(ClassViewModelData data, bool expected)
            => Assert.Equal(expected, ((TypeViewModel)data).IsClass);

        [Theory]
        [ClassViewModelData(typeof(int), false)]
        [ClassViewModelData(typeof(int?), false)]
        [ClassViewModelData(typeof(DateTime), false)]
        [ClassViewModelData(typeof(string), false)]
        [ClassViewModelData(typeof(IFile), false)]
        [ClassViewModelData(typeof(string[]), false)]
        [ClassViewModelData(typeof(List<string>), false)]
        [ClassViewModelData(typeof(IWeatherService), true)]
        [ClassViewModelData(typeof(WeatherService), true)]
        public void IsPoco_IsCorrect(ClassViewModelData data, bool expected)
            => Assert.Equal(expected, ((TypeViewModel)data).IsPOCO);

        [Theory]
        [ClassViewModelData(typeof(int), false)]
        [ClassViewModelData(typeof(int?), false)]
        [ClassViewModelData(typeof(DateTime), false)]
        [ClassViewModelData(typeof(string), false)]
        [ClassViewModelData(typeof(WeatherService[]), false)]
        [ClassViewModelData(typeof(IWeatherService), true)]
        [ClassViewModelData(typeof(WeatherService), true)]
        public void HasClassViewModel_IsCorrect(ClassViewModelData data, bool expected)
            => Assert.Equal(expected, ((TypeViewModel)data).HasClassViewModel);
    }
}
