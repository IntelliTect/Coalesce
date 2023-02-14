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
        [ClassViewModelData(typeof(ExternalParentAsInputOnly), false, true, false)]
        [ClassViewModelData(typeof(ExternalChildAsInputOnly), false, true, false)]
        [ClassViewModelData(typeof(ExternalParentAsOutputOnly), true, false, false)]
        [ClassViewModelData(typeof(ExternalChildAsOutputOnly), true, false, false)]
        [ClassViewModelData(typeof(ReadOnlyEntityUsedAsMethodInput), true, true, false)]
        public void PropertySecurityIsUnused_ReflectsActualUsage(
            ClassViewModelData data, bool read, bool init, bool edit)
        {
            Assert.All(data.ClassViewModel.ClientProperties, p =>
            {
                Assert.Equal(read, !p.SecurityInfo.Read.IsUnused);
                Assert.Equal(init, !p.SecurityInfo.Init.IsUnused);
                Assert.Equal(edit, !p.SecurityInfo.Edit.IsUnused);
            });
        }

        [Theory]
        [ClassViewModelData(typeof(AppDbContext))]
        public void PropertyParent_IsCorrect(ClassViewModelData data)
        {
            var directlyDeclaredProp = data.ClassViewModel.PropertyByName(nameof(AppDbContext.People));
            Assert.Equal(data.ClassViewModel, directlyDeclaredProp.Parent);
            Assert.Equal(data.ClassViewModel, directlyDeclaredProp.EffectiveParent);

            var inheritedProp = data.ClassViewModel.PropertyByName(nameof(AppDbContext.Database));
            Assert.Equal(nameof(DbContext), inheritedProp.Parent.Name);
            Assert.NotEqual(data.ClassViewModel, inheritedProp.Parent);
            Assert.Equal(data.ClassViewModel, inheritedProp.EffectiveParent);
        }

        [Theory]
        [ClassViewModelData(typeof(Case))]
        public void NonHomogenousManyToMany_IsCorrect(ClassViewModelData data)
        {
            var prop = data.ClassViewModel.PropertyByName(nameof(Case.CaseProducts));

            Assert.Equal("Case", prop.ManyToManyNearNavigationProperty.Name);
            Assert.Equal("CaseId", prop.ManyToManyNearNavigationProperty.ForeignKeyProperty.Name);
            Assert.Equal("Product", prop.ManyToManyFarNavigationProperty.Name);
            Assert.Equal("ProductId", prop.ManyToManyFarNavigationProperty.ForeignKeyProperty.Name);
        }

        [Theory]
        [ClassViewModelData(typeof(Person))]
        public void HomogeneousManyToMany_IsCorrect(ClassViewModelData data)
        {
            var prop = data.ClassViewModel.PropertyByName(nameof(Person.SiblingRelationships));

            Assert.Equal("Person", prop.ManyToManyNearNavigationProperty.Name);
            Assert.Equal("PersonId", prop.ManyToManyNearNavigationProperty.ForeignKeyProperty.Name);
            Assert.Equal("PersonTwo", prop.ManyToManyFarNavigationProperty.Name);
            Assert.Equal("PersonTwoId", prop.ManyToManyFarNavigationProperty.ForeignKeyProperty.Name);
        }
    }
}
