using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Tests.Util;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
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
        [ClassViewModelData(typeof(ExternalParentAsInputOnly), false, true)]
        [ClassViewModelData(typeof(ExternalChildAsInputOnly), false, true)]
        [ClassViewModelData(typeof(ExternalParentAsOutputOnly), true, false)]
        [ClassViewModelData(typeof(ExternalChildAsOutputOnly), true, false)]
        public void ExternalType_PropertySecurityReflectsActualUsage(ClassViewModelData data, bool read, bool write)
        {
            Assert.All(data.ClassViewModel.ClientProperties, p =>
            {
                Assert.Equal(read, !p.SecurityInfo.Read.IsUnused);
                Assert.Equal(write, !p.SecurityInfo.Edit.IsUnused);
            });
        }

        [Theory]
        [ClassViewModelData(typeof(TestDbContext))]
        public void PropertyParent_IsCorrect(ClassViewModelData data)
        {
            var directlyDeclaredProp = data.ClassViewModel.PropertyByName(nameof(TestDbContext.People));
            Assert.Equal(data.ClassViewModel, directlyDeclaredProp.Parent);
            Assert.Equal(data.ClassViewModel, directlyDeclaredProp.EffectiveParent);

            var inheritedProp = data.ClassViewModel.PropertyByName(nameof(TestDbContext.Database));
            Assert.Equal(nameof(DbContext), inheritedProp.Parent.Name);
            Assert.NotEqual(data.ClassViewModel, inheritedProp.Parent);
            Assert.Equal(data.ClassViewModel, inheritedProp.EffectiveParent);
        }
    }
}
