using IntelliTect.Coalesce.Tests.TargetClasses;
using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Tests.Util;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.TypeDefinition.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using static IntelliTect.Coalesce.DataAnnotations.DateTypeAttribute;

namespace IntelliTect.Coalesce.Tests.TypeDefinition;

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

    [Theory, ClassViewModelData<ComplexModel>]
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
    [PropertyViewModelData<ComplexModel>(nameof(ComplexModel.SingleTestId))]
    [PropertyViewModelData<ComplexModelDependent>(nameof(ComplexModelDependent.ParentId))]
    public void IsForeignKey_IsCorrect(PropertyViewModelData data)
    {
        PropertyViewModel vm = data;

        Assert.True(vm.IsForeignKey);
        Assert.NotNull(vm.ForeignKeyPrincipalType);
        Assert.Equal(PropertyRole.ForeignKey, vm.Role);
    }

    [Theory]
    [PropertyViewModelData<ComplexModel>(nameof(ComplexModel.Tests), nameof(Test.ComplexModelId))]
    [PropertyViewModelData<ComplexModel>(nameof(ComplexModel.ChildrenWithoutRefNavProp), nameof(ComplexModelDependent.ParentId))]
    public void Role_IsCollectionNavigation_IsCorrect(PropertyViewModelData data, string fkName)
    {
        PropertyViewModel vm = data;

        Assert.Equal(PropertyRole.CollectionNavigation, vm.Role);
        Assert.Equal(fkName, vm.ForeignKeyProperty.Name);
    }

    [Theory]
    [PropertyViewModelData<EnumPk>(nameof(EnumPk.EnumPkId))]
    public void EnumPk_HasCorrectRulesAndProps(PropertyViewModelData data)
    {
        PropertyViewModel prop = data;

        Assert.True(prop.IsPrimaryKey);
        Assert.False(prop.IsAutoGeneratedPrimaryKey);
        Assert.True(prop.IsCreateOnly);
        Assert.True(prop.IsRequired);
    }

    [Theory]
    [PropertyViewModelData<DateTimePk>(nameof(DateTimePk.DateTimePkId))]
    public void DateTimePk_HasCorrectRulesAndProps(PropertyViewModelData data)
    {
        PropertyViewModel prop = data;

        Assert.True(prop.IsPrimaryKey);
        Assert.False(prop.IsAutoGeneratedPrimaryKey);
        Assert.True(prop.IsCreateOnly);
        Assert.True(prop.IsRequired);
    }

    [Theory]
    [PropertyViewModelData<DateTimeOffsetPk>(nameof(DateTimeOffsetPk.DateTimeOffsetPkId))]
    public void DateTimeOffsetPk_HasCorrectRulesAndProps(PropertyViewModelData data)
    {
        PropertyViewModel prop = data;

        Assert.True(prop.IsPrimaryKey);
        Assert.False(prop.IsAutoGeneratedPrimaryKey);
        Assert.True(prop.IsCreateOnly);
        Assert.True(prop.IsRequired);
    }

    [Theory]
    [PropertyViewModelData<DateOnlyPk>(nameof(DateOnlyPk.DateOnlyPkId))]
    public void DateOnlyPk_HasCorrectRulesAndProps(PropertyViewModelData data)
    {
        PropertyViewModel prop = data;

        Assert.True(prop.IsPrimaryKey);
        Assert.False(prop.IsAutoGeneratedPrimaryKey);
        Assert.True(prop.IsCreateOnly);
        Assert.True(prop.IsRequired);
    }

    [Theory]
    [PropertyViewModelData<TimeOnlyPk>(nameof(TimeOnlyPk.TimeOnlyPkId))]
    public void TimeOnlyPk_HasCorrectRulesAndProps(PropertyViewModelData data)
    {
        PropertyViewModel prop = data;

        Assert.True(prop.IsPrimaryKey);
        Assert.False(prop.IsAutoGeneratedPrimaryKey);
        Assert.True(prop.IsCreateOnly);
        Assert.True(prop.IsRequired);
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

    [Theory]
    [PropertyViewModelData<ComplexModel>(nameof(ComplexModel.String), true)]
    [PropertyViewModelData<ComplexModel>(nameof(ComplexModel.SingleTestId), true)]
    [PropertyViewModelData<ComplexModel>(nameof(ComplexModel.IntCollection), true)]
    [PropertyViewModelData<ComplexModel>(nameof(ComplexModel.MutablePrimitiveCollection), true)]
    [PropertyViewModelData<ComplexModel>(nameof(ComplexModel.Tests), false)] // collection nav
    [PropertyViewModelData<ComplexModel>(nameof(ComplexModel.SingleTest), false)] // ref nav
    [PropertyViewModelData<PropSec>(nameof(PropSec.ReadOnlyViaRead), false)]
    [PropertyViewModelData<PropSec>(nameof(PropSec.ReadOnlyViaReadOnly), false)]
    [PropertyViewModelData<PropSec>(nameof(PropSec.ReadOnlyViaReadOnlyApi), false)]
    [PropertyViewModelData<PropSec>(nameof(PropSec.ReadOnlyViaNonPublicSetter), false)]
    public void IsClientSerializable_IsCorrect(PropertyViewModelData data, bool expected)
    {
        PropertyViewModel vm = data;
        Assert.Equal(expected, vm.IsClientSerializable);
    }

    [Theory]
    [PropertyViewModelData<ComplexModel>(nameof(ComplexModel.ReferenceNavigation), false)]
    [PropertyViewModelData<ComplexModel>(nameof(ComplexModel.Tests), false)]
    [PropertyViewModelData<ComplexModel>(nameof(ComplexModel.ComplexModelId), false)]
#if NET7_0_OR_GREATER
    [PropertyViewModelData<RequiredAndInitModel>(nameof(RequiredAndInitModel.RequiredRef), true)]
    [PropertyViewModelData<RequiredAndInitModel>(nameof(RequiredAndInitModel.RequiredValue), true)]
    [PropertyViewModelData<RequiredAndInitModel>(nameof(RequiredAndInitModel.RequiredInitRef), true)]
    [PropertyViewModelData<RequiredAndInitModel>(nameof(RequiredAndInitModel.RequiredInitValue), true)]
#endif
    public void IsRequired_IsCorrect(PropertyViewModelData data, bool expected)
    {
        PropertyViewModel vm = data;
        Assert.Equal(expected, vm.IsRequired);
    }

    [Theory]
    [SymbolPropertyViewModelData<Person>(
        nameof(Person.Name),
        "Calculated name of the person. eg., Mr. Michael Stokesbary.\nA concatenation of Title, FirstName, and LastName.")]
    public void Comment_IsCorrect(PropertyViewModelData data, string expected)
    {
        PropertyViewModel vm = data;
        Assert.Equal(expected, vm.Comment, ignoreLineEndingDifferences: true);
    }


    [Theory]
    [PropertyViewModelData<OneToOneParent>(nameof(OneToOneParent.SharedKeyChild1))]
    [PropertyViewModelData<OneToOneParent>(nameof(OneToOneParent.SharedKeyChild2))]
    public void OneToOne_ParentNavigations_HasCorrectMetadata(PropertyViewModelData data)
    {
        PropertyViewModel vm = data;
        Assert.Equal(PropertyRole.ReferenceNavigation, vm.Role);
        Assert.Equal(vm.Parent.PropertyByName(nameof(OneToOneParent.Id)), vm.ForeignKeyProperty);
        Assert.Equal(nameof(OneToOneSharedKeyChild1.Parent), vm.InverseProperty.Name);
    }

    [Theory]
    [PropertyViewModelData<OneToOneSharedKeyChild1>(nameof(OneToOneSharedKeyChild1.Parent), "SharedKeyChild1")]
    [PropertyViewModelData<OneToOneSharedKeyChild2>(nameof(OneToOneSharedKeyChild2.Parent), "SharedKeyChild2")]
    public void OneToOne_ChildNavigations_HasCorrectMetadata(PropertyViewModelData data, string inverse)
    {
        PropertyViewModel vm = data;
        Assert.Equal(PropertyRole.ReferenceNavigation, vm.Role);
        Assert.Equal(vm.Parent.PropertyByName("ParentId"), vm.ForeignKeyProperty);
        Assert.Equal(inverse, vm.InverseProperty.Name);
    }

    [Theory]
    [PropertyViewModelData<OneToOneParent>(nameof(OneToOneParent.SeparateKeyChild))]
    [PropertyViewModelData<OneToOneParent>(nameof(OneToOneParent.SeparateKeyChildNoIp))]
    public void OneToOne_SeparateKey_ParentNavigations_HasCorrectMetadata(PropertyViewModelData data)
    {
        PropertyViewModel vm = data;
        Assert.Equal(PropertyRole.Value, vm.Role);
        Assert.Null(vm.ForeignKeyProperty);
    }

    [Theory]
    [PropertyViewModelData<OneToOneSeparateKeyChild>(nameof(OneToOneSeparateKeyChild.Parent))]
    public void OneToOne_SeparateKey_ChildNavigations_HasCorrectMetadata(PropertyViewModelData data)
    {
        // See comments in OneToOne.cs about what this is all about

        PropertyViewModel vm = data;
        Assert.Equal(PropertyRole.ReferenceNavigation, vm.Role);
        Assert.Equal(vm.Parent.PropertyByName("ParentId"), vm.ForeignKeyProperty);

        // We can't model the other side as a reference navigation,
        // so the inverse property shouldn't be defined.
        Assert.Null(vm.InverseProperty);
    }

    [Theory]
    [PropertyViewModelData<OneToOneManyChildren>(nameof(OneToOneManyChildren.OneToOneParent))]
    [Description("https://github.com/IntelliTect/Coalesce/commit/513db257dda32b99099355f1a6de0f5fbf367f5a")]
    public void ReferenceNavigation_HasCorrectFkWhenPrincipalAlsoParticipatesInOneToOne(PropertyViewModelData data)
    {
        PropertyViewModel prop = data;

        // Precodition: Principal participates in a one-to-one
        Assert.True(prop.Object!.PrimaryKey.IsForeignKey);

        // Correct foreign key here is ParentId, not the child's PK.
        Assert.Equal(PropertyRole.ReferenceNavigation, prop.Role);
        Assert.Equal(nameof(OneToOneManyChildren.OneToOneParentId), prop.ForeignKeyProperty.Name);
    }

    [Theory]
    [PropertyViewModelData<OneToOneParent>(nameof(OneToOneParent.ManyChildren))]
    [Description("https://github.com/IntelliTect/Coalesce/commit/513db257dda32b99099355f1a6de0f5fbf367f5a")]
    public void CollectionNavigation_HasCorrectFkWhenPrincipalAlsoParticipatesInOneToOne(PropertyViewModelData data)
    {
        PropertyViewModel prop = data;

        // Precodition: Principal participates in a one-to-one
        Assert.True(prop.EffectiveParent.PrimaryKey.IsForeignKey);

        // Correct foreign key here is ParentId, not the child's PK.
        Assert.Equal(PropertyRole.CollectionNavigation, prop.Role);
        Assert.Equal(nameof(OneToOneManyChildren.OneToOneParentId), prop.ForeignKeyProperty.Name);
    }

}
