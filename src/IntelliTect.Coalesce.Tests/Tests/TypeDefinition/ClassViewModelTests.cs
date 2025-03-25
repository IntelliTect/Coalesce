using IntelliTect.Coalesce.Tests.TargetClasses;
using IntelliTect.Coalesce.Tests.Util;
using Xunit;

namespace IntelliTect.Coalesce.Tests.TypeDefinition
{
    public class ClassViewModelTests
    {
        [Theory]
        [ClassViewModelData(typeof(TargetClasses.OrderingChild))]
        public void DefaultOrderBy_UsesNestedPropertiesWhenOrderingByRefNavigation(ClassViewModelData data)
        {
            var orderings = data.ClassViewModel.DefaultOrderBy;
            Assert.Collection(orderings,
                ordering =>
                {
                    Assert.Equal(1, ordering.FieldOrder);
                    Assert.Equal("OrderingParent1", ordering.Properties[0].Name);
                    Assert.Equal("OrderingGrandparent", ordering.Properties[1].Name);
                    Assert.Equal("OrderedField", ordering.Properties[2].Name);
                },
                ordering =>
                {
                    Assert.Equal(2, ordering.FieldOrder);
                    Assert.Equal("OrderingParent2", ordering.Properties[0].Name);
                    Assert.Equal("OrderingGrandparent", ordering.Properties[1].Name);
                    Assert.Equal("OrderedField", ordering.Properties[2].Name);
                }
            );
        }

        [Theory]
        [ClassViewModelData(typeof(TargetClasses.OrdersByUnorderableParent))]
        public void DefaultOrderBy_UsesNavigationDirectlyWhenOrderingByUnorderableRefNavigation(ClassViewModelData data)
        {
            var orderings = data.ClassViewModel.DefaultOrderBy;
            Assert.Collection(orderings,
                ordering =>
                {
                    var prop = Assert.Single(ordering.Properties);
                    Assert.Equal("Parent", prop.Name);
                }
            );
        }

        [Theory]
        [ClassViewModelData(typeof(AbstractImpl1))]
        public void GetAttribute_RespectsInheritance(ClassViewModelData data)
        {
            var vm = data.ClassViewModel;

            // The read and create attributes are declared on AbstractImpl's base class,
            // but should still be surfaced on AbstractImpl because they are inherited attributes.
            Assert.Equal("ReadRole", vm.SecurityInfo.Read.Roles);
            Assert.True(vm.SecurityInfo.Create.NoAccess);

            // Edit role is defined on both, so the one on AbstractImpl should be the effective one:
            Assert.True(vm.SecurityInfo.Edit.NoAccess);
        }
    }
}
