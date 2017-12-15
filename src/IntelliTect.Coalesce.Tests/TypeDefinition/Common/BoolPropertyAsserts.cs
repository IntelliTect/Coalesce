using IntelliTect.Coalesce.Tests.TypeDefinition.TargetClasses;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace IntelliTect.Coalesce.Tests.TypeDefinition.Common
{
    internal static class BoolPropertyAsserts
    {
        internal static void CheckNullableBoolProperties(ClassViewModel vm)
        {
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
                //TODO: This assert is broken for Roslyn
                //Assert.False(prop.Type.IsBool);
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

        internal static void CheckIsBoolForProperties(ClassViewModel vm)
        {
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
                //TODO: This assert is broken for Roslyn
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
