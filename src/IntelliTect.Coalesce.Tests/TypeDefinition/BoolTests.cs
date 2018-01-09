using IntelliTect.Coalesce.Tests.TargetClasses;
using IntelliTect.Coalesce.Tests.Util;
using IntelliTect.Coalesce.TypeDefinition;
using System.Text;
using Xunit;

namespace IntelliTect.Coalesce.Tests.TypeDefinition
{
    public class BoolTests
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
                Assert.False(prop.Type.IsNullable);
            }

            foreach (var prop in new[]
            {
                vm.PropertyByName(nameof(Bools.NullableGenericClassName)),
                vm.PropertyByName(nameof(Bools.NullableGenericKeywordName)),
                vm.PropertyByName(nameof(Bools.NullableQuestionMarkClassName)),
                vm.PropertyByName(nameof(Bools.NullableQuestionMarkKeywordName)),
            })
            {
                Assert.True(prop.Type.IsNullable);
                Assert.False(prop.Type.NullableUnderlyingType.IsNullable);
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

            // Nullable Collections/Arrays
            foreach (var prop in new[]
            {
                vm.PropertyByName(nameof(Bools.CollectionNullableItems)),
                vm.PropertyByName(nameof(Bools.ArrayNullable)),
            })
            {
                Assert.True(prop.Type.PureType.IsNullable);
                Assert.False(prop.Type.PureType.PureType.IsNullable);
            }

            // Non-Nullable Collections/Arrays
            foreach (var prop in new[]
            {
                vm.PropertyByName(nameof(Bools.Collection)),
                vm.PropertyByName(nameof(Bools.Array)),
            })
            {
                Assert.False(prop.Type.PureType.IsNullable);
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
                Assert.True(prop.Type.IsNullable);
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
    }
}
