using IntelliTect.Coalesce.Tests.TargetClasses;
using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Tests.Util;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.TypeDefinition.Enums;
using System.Text;
using Xunit;
using static IntelliTect.Coalesce.DataAnnotations.DateTypeAttribute;

namespace IntelliTect.Coalesce.Tests.TypeDefinition
{
    public class PropertyViewModelTests
    {
        [Theory, ClassViewModelData(typeof(Bools))]
        // Unfortunately, we must specify our parameter type as ClassViewModelData instead of ClassViewModel.
        // This is because xunit's implicit conversion support only checks for conversions
        // defined on the parameter's class and not the argument's class.
        // See https://github.com/xunit/xunit/issues/1607
        public void IsNullable_CorrectForValueTypes(ClassViewModelData data)
        {
            ClassViewModel vm = data;

            foreach (var prop in new[]
            {
                vm.PropertyByName(nameof(Bools.NonNullableClassName)),
                vm.PropertyByName(nameof(Bools.NonNullableKeywordName)),
            })
            {
                Assert.False(prop.Type.IsReferenceOrNullableValue);
            }

            foreach (var prop in new[]
            {
                vm.PropertyByName(nameof(Bools.NullableGenericClassName)),
                vm.PropertyByName(nameof(Bools.NullableGenericKeywordName)),
                vm.PropertyByName(nameof(Bools.NullableQuestionMarkClassName)),
                vm.PropertyByName(nameof(Bools.NullableQuestionMarkKeywordName)),
            })
            {
                Assert.True(prop.Type.IsReferenceOrNullableValue);
                Assert.False(prop.Type.NullableValueUnderlyingType.IsReferenceOrNullableValue);
            }

            // Collections
            foreach (var prop in new[]
            {
                vm.PropertyByName(nameof(Bools.Collection)),
                vm.PropertyByName(nameof(Bools.CollectionNullableItems)),
            })
            {
                Assert.True(prop.Type.IsCollection);

                Assert.False(prop.Type.IsBool);
                Assert.True(prop.PureType.IsBool);
            }

            // Arrays
            foreach (var prop in new[]
            {
                vm.PropertyByName(nameof(Bools.Array)),
                vm.PropertyByName(nameof(Bools.ArrayNullable)),
            })
            {
                Assert.True(prop.Type.IsArray);
                Assert.True(prop.Type.IsCollection);
                Assert.False(prop.Type.IsBool);
                Assert.True(prop.PureType.IsBool);
                Assert.True(prop.Type.ArrayType.IsBool);
            }

            // Nullable Collections/Arrays
            foreach (var prop in new[]
            {
                vm.PropertyByName(nameof(Bools.CollectionNullableItems)),
                vm.PropertyByName(nameof(Bools.ArrayNullable)),
            })
            {
                Assert.True(prop.Type.IsCollection);
                Assert.True(prop.Type.PureType.IsReferenceOrNullableValue);
                Assert.False(prop.Type.PureType.PureType.IsReferenceOrNullableValue);
            }

            // Non-Nullable Collections/Arrays
            foreach (var prop in new[]
            {
                vm.PropertyByName(nameof(Bools.Collection)),
                vm.PropertyByName(nameof(Bools.Array)),
            })
            {
                Assert.True(prop.Type.IsCollection);
                Assert.False(prop.Type.PureType.IsReferenceOrNullableValue);
            }
        }

        [Theory, ClassViewModelData(typeof(Bools))]
        public void IsBool_CorrectForBoolProperties(ClassViewModelData data)
        {
            ClassViewModel vm = data;

            foreach (var prop in new[]
            {
                vm.PropertyByName(nameof(Bools.NullableGenericClassName)),
                vm.PropertyByName(nameof(Bools.NullableGenericKeywordName)),
                vm.PropertyByName(nameof(Bools.NullableQuestionMarkClassName)),
                vm.PropertyByName(nameof(Bools.NullableQuestionMarkKeywordName)),
            })
            {
                Assert.True(prop.Type.IsBool);
                Assert.True(prop.PureType.IsBool);
                Assert.True(prop.Type.IsReferenceOrNullableValue);
            }

            // Collections
            foreach (var prop in new[]
            {
                vm.PropertyByName(nameof(Bools.Collection)),
                vm.PropertyByName(nameof(Bools.CollectionNullableItems)),
            })
            {
                Assert.True(prop.Type.IsCollection);
                Assert.False(prop.Type.IsBool);
                Assert.True(prop.PureType.IsBool);
            }
            // Arrays
            foreach (var prop in new[]
            {
                vm.PropertyByName(nameof(Bools.Array)),
                vm.PropertyByName(nameof(Bools.ArrayNullable)),
            })
            {
                Assert.True(prop.Type.IsArray);
                Assert.False(prop.Type.IsBool);
                Assert.True(prop.PureType.IsBool);
            }
        }

        [Theory, ClassViewModelData(typeof(ComplexModel))]
        public void DateType_IsCorrect(ClassViewModelData data)
        {
            ClassViewModel vm = data;

            Assert.Equal(DateTypes.DateOnly, vm.PropertyByName(nameof(ComplexModel.SystemDateOnly)).DateType);
            Assert.Equal(DateTypes.DateOnly, vm.PropertyByName(nameof(ComplexModel.DateOnlyViaAttribute)).DateType);

            Assert.Equal(DateTypes.TimeOnly, vm.PropertyByName(nameof(ComplexModel.SystemTimeOnly)).DateType);

            Assert.Equal(DateTypes.DateTime, vm.PropertyByName(nameof(ComplexModel.DateTime)).DateType);
            Assert.Equal(DateTypes.DateTime, vm.PropertyByName(nameof(ComplexModel.DateTimeNullable)).DateType);
            Assert.Equal(DateTypes.DateTime, vm.PropertyByName(nameof(ComplexModel.DateTimeOffset)).DateType);
            Assert.Equal(DateTypes.DateTime, vm.PropertyByName(nameof(ComplexModel.DateTimeOffsetNullable)).DateType);
        }

        [Theory]
        [PropertyViewModelData(typeof(ComplexModel), nameof(ComplexModel.SingleTestId))]
        [PropertyViewModelData(typeof(ComplexModelDependent), nameof(ComplexModelDependent.ParentId))]
        public void IsForeignKey_IsCorrect(PropertyViewModelData data)
        {
            PropertyViewModel vm = data;

            Assert.True(vm.IsForeignKey);
            Assert.NotNull(vm.ForeignKeyPrincipalType);
            Assert.Equal(PropertyRole.ForeignKey, vm.Role);
        }
    }
}
