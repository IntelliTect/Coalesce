using IntelliTect.Coalesce.Tests.TypeDefinition.TargetClasses;
using IntelliTect.Coalesce.TypeDefinition;
using System.Text;
using Xunit;

namespace IntelliTect.Coalesce.Tests.TypeDefinition.Reflection
{
    public partial class PropertyTypeTests
    {
        [Fact]
        public void IsNullable_CorrectForValueTypes()
        {
            var vm = new ReflectionClassViewModel(typeof(Bools));
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
                Assert.False(prop.Type.UnderlyingNullableType.IsNullable);
            }

            // TODO: arrays???
            // TODO: check the right things on collections.
            //foreach (var prop in new[]
            //{
            //    vm.PropertyByName(nameof(Bools.Collection)),
            //    vm.PropertyByName(nameof(Bools.CollectionNullableItems)),
            //})
            //{
            //    Assert.True(prop.Type.IsCollection);
            //    Assert.False(prop.Type.IsBool);
            //    Assert.True(prop.PureType.IsBool);
            //}
        }


        [Fact]
        public void IsBool_CorrectForBoolProperties()
        {
            var vm = new ReflectionClassViewModel(typeof(Bools));
            foreach (var prop in new[]
            {
                vm.PropertyByName(nameof(Bools.NullableGenericClassName)),
                vm.PropertyByName(nameof(Bools.NullableGenericKeywordName)),
                vm.PropertyByName(nameof(Bools.NullableQuestionMarkClassName)),
                vm.PropertyByName(nameof(Bools.NullableQuestionMarkKeywordName)),
                vm.PropertyByName(nameof(Bools.Collection)),
                vm.PropertyByName(nameof(Bools.CollectionNullableItems)),
            })
            {
                Assert.True(prop.Type.IsBool);
                Assert.True(prop.PureType.IsBool);
            }

            // todo: collections
            //foreach (var prop in new[]
            //{
            //    vm.PropertyByName(nameof(Bools.BoolCollection1)),
            //    vm.PropertyByName(nameof(Bools.BoolCollection2)),
            //})
            //{
            //    Assert.True(prop.Type.IsCollection);
            //    Assert.False(prop.Type.IsBool);
            //    Assert.True(prop.PureType.IsBool);
            //}
        }
    }
}
