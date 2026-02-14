using IntelliTect.Coalesce.Tests.TargetClasses;
using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Tests.Util;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.TypeDefinition.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using static IntelliTect.Coalesce.DataAnnotations.DateTypeAttribute;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Tests.TypeDefinition;

public class PropertyViewModelTests
{
    [Test, ClassViewModelData(typeof(Bools))]
    // Unfortunately, we must specify our parameter type as ClassViewModelData instead of ClassViewModel.
    // This is because xunit's implicit conversion support only checks for conversions
    // defined on the parameter's class and not the argument's class.
    // See https://github.com/xunit/xunit/issues/1607
    public async Task IsNullable_CorrectForValueTypes(ClassViewModelData data)
    {
        ClassViewModel vm = data;

        foreach (var prop in new[]
        {
            vm.PropertyByName(nameof(Bools.NonNullableClassName)),
            vm.PropertyByName(nameof(Bools.NonNullableKeywordName)),
        })
        {
            await Assert.That(prop.Type.IsReferenceOrNullableValue).IsFalse();
        }

        foreach (var prop in new[]
        {
            vm.PropertyByName(nameof(Bools.NullableGenericClassName)),
            vm.PropertyByName(nameof(Bools.NullableGenericKeywordName)),
            vm.PropertyByName(nameof(Bools.NullableQuestionMarkClassName)),
            vm.PropertyByName(nameof(Bools.NullableQuestionMarkKeywordName)),
        })
        {
            await Assert.That(prop.Type.IsReferenceOrNullableValue).IsTrue();
            await Assert.That(prop.Type.NullableValueUnderlyingType.IsReferenceOrNullableValue).IsFalse();
        }

        // Collections
        foreach (var prop in new[]
        {
            vm.PropertyByName(nameof(Bools.Collection)),
            vm.PropertyByName(nameof(Bools.CollectionNullableItems)),
        })
        {
            await Assert.That(prop.Type.IsCollection).IsTrue();

            await Assert.That(prop.Type.IsBool).IsFalse();
            await Assert.That(prop.PureType.IsBool).IsTrue();
        }

        // Arrays
        foreach (var prop in new[]
        {
            vm.PropertyByName(nameof(Bools.Array)),
            vm.PropertyByName(nameof(Bools.ArrayNullable)),
        })
        {
            await Assert.That(prop.Type.IsArray).IsTrue();
            await Assert.That(prop.Type.IsCollection).IsTrue();
            await Assert.That(prop.Type.IsBool).IsFalse();
            await Assert.That(prop.PureType.IsBool).IsTrue();
            await Assert.That(prop.Type.ArrayType.IsBool).IsTrue();
        }

        // Nullable Collections/Arrays
        foreach (var prop in new[]
        {
            vm.PropertyByName(nameof(Bools.CollectionNullableItems)),
            vm.PropertyByName(nameof(Bools.ArrayNullable)),
        })
        {
            await Assert.That(prop.Type.IsCollection).IsTrue();
            await Assert.That(prop.Type.PureType.IsReferenceOrNullableValue).IsTrue();
            await Assert.That(prop.Type.PureType.PureType.IsReferenceOrNullableValue).IsFalse();
        }

        // Non-Nullable Collections/Arrays
        foreach (var prop in new[]
        {
            vm.PropertyByName(nameof(Bools.Collection)),
            vm.PropertyByName(nameof(Bools.Array)),
        })
        {
            await Assert.That(prop.Type.IsCollection).IsTrue();
            await Assert.That(prop.Type.PureType.IsReferenceOrNullableValue).IsFalse();
        }
    }

    [Test, ClassViewModelData(typeof(Bools))]
    public async Task IsBool_CorrectForBoolProperties(ClassViewModelData data)
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
            await Assert.That(prop.Type.IsBool).IsTrue();
            await Assert.That(prop.PureType.IsBool).IsTrue();
            await Assert.That(prop.Type.IsReferenceOrNullableValue).IsTrue();
        }

        // Collections
        foreach (var prop in new[]
        {
            vm.PropertyByName(nameof(Bools.Collection)),
            vm.PropertyByName(nameof(Bools.CollectionNullableItems)),
        })
        {
            await Assert.That(prop.Type.IsCollection).IsTrue();
            await Assert.That(prop.Type.IsBool).IsFalse();
            await Assert.That(prop.PureType.IsBool).IsTrue();
        }
        // Arrays
        foreach (var prop in new[]
        {
            vm.PropertyByName(nameof(Bools.Array)),
            vm.PropertyByName(nameof(Bools.ArrayNullable)),
        })
        {
            await Assert.That(prop.Type.IsArray).IsTrue();
            await Assert.That(prop.Type.IsBool).IsFalse();
            await Assert.That(prop.PureType.IsBool).IsTrue();
        }
    }

    [Test, ClassViewModelData<ComplexModel>]
    public async Task DateType_IsCorrect(ClassViewModelData data)
    {
        ClassViewModel vm = data;

        await Assert.That(vm.PropertyByName(nameof(ComplexModel.SystemDateOnly)).DateType).IsEqualTo(DateTypes.DateOnly);
        await Assert.That(vm.PropertyByName(nameof(ComplexModel.DateOnlyViaAttribute)).DateType).IsEqualTo(DateTypes.DateOnly);

        await Assert.That(vm.PropertyByName(nameof(ComplexModel.SystemTimeOnly)).DateType).IsEqualTo(DateTypes.TimeOnly);

        await Assert.That(vm.PropertyByName(nameof(ComplexModel.DateTime)).DateType).IsEqualTo(DateTypes.DateTime);
        await Assert.That(vm.PropertyByName(nameof(ComplexModel.DateTimeNullable)).DateType).IsEqualTo(DateTypes.DateTime);
        await Assert.That(vm.PropertyByName(nameof(ComplexModel.DateTimeOffset)).DateType).IsEqualTo(DateTypes.DateTime);
        await Assert.That(vm.PropertyByName(nameof(ComplexModel.DateTimeOffsetNullable)).DateType).IsEqualTo(DateTypes.DateTime);
    }

    [Test]
    [PropertyViewModelData<ComplexModel>(nameof(ComplexModel.SingleTestId))]
    [PropertyViewModelData<ComplexModelDependent>(nameof(ComplexModelDependent.ParentId))]
    public async Task IsForeignKey_IsCorrect(PropertyViewModelData data)
    {
        PropertyViewModel vm = data;

        await Assert.That(vm.IsForeignKey).IsTrue();
        await Assert.That(vm.ForeignKeyPrincipalType).IsNotNull();
        await Assert.That(vm.Role).IsEqualTo(PropertyRole.ForeignKey);
    }

    [Test]
    [PropertyViewModelData<ComplexModel>(nameof(ComplexModel.Tests), nameof(Test.ComplexModelId))]
    [PropertyViewModelData<ComplexModel>(nameof(ComplexModel.ChildrenWithoutRefNavProp), nameof(ComplexModelDependent.ParentId))]
    public async Task Role_IsCollectionNavigation_IsCorrect(PropertyViewModelData data, string fkName)
    {
        PropertyViewModel vm = data;

        await Assert.That(vm.Role).IsEqualTo(PropertyRole.CollectionNavigation);
        await Assert.That(vm.ForeignKeyProperty.Name).IsEqualTo(fkName);
    }

    [Test]
    [PropertyViewModelData<EnumPk>(nameof(EnumPk.EnumPkId))]
    public async Task EnumPk_HasCorrectRulesAndProps(PropertyViewModelData data)
    {
        PropertyViewModel prop = data;

        await Assert.That(prop.IsPrimaryKey).IsTrue();
        await Assert.That(prop.IsAutoGeneratedPrimaryKey).IsFalse();
        await Assert.That(prop.IsCreateOnly).IsTrue();
        await Assert.That(prop.IsRequired).IsTrue();
    }

    [Test]
    [PropertyViewModelData<DateTimePk>(nameof(DateTimePk.DateTimePkId))]
    public async Task DateTimePk_HasCorrectRulesAndProps(PropertyViewModelData data)
    {
        PropertyViewModel prop = data;

        await Assert.That(prop.IsPrimaryKey).IsTrue();
        await Assert.That(prop.IsAutoGeneratedPrimaryKey).IsFalse();
        await Assert.That(prop.IsCreateOnly).IsTrue();
        await Assert.That(prop.IsRequired).IsTrue();
    }

    [Test]
    [PropertyViewModelData<DateTimeOffsetPk>(nameof(DateTimeOffsetPk.DateTimeOffsetPkId))]
    public async Task DateTimeOffsetPk_HasCorrectRulesAndProps(PropertyViewModelData data)
    {
        PropertyViewModel prop = data;

        await Assert.That(prop.IsPrimaryKey).IsTrue();
        await Assert.That(prop.IsAutoGeneratedPrimaryKey).IsFalse();
        await Assert.That(prop.IsCreateOnly).IsTrue();
        await Assert.That(prop.IsRequired).IsTrue();
    }

    [Test]
    [PropertyViewModelData<DateOnlyPk>(nameof(DateOnlyPk.DateOnlyPkId))]
    public async Task DateOnlyPk_HasCorrectRulesAndProps(PropertyViewModelData data)
    {
        PropertyViewModel prop = data;

        await Assert.That(prop.IsPrimaryKey).IsTrue();
        await Assert.That(prop.IsAutoGeneratedPrimaryKey).IsFalse();
        await Assert.That(prop.IsCreateOnly).IsTrue();
        await Assert.That(prop.IsRequired).IsTrue();
    }

    [Test]
    [PropertyViewModelData<TimeOnlyPk>(nameof(TimeOnlyPk.TimeOnlyPkId))]
    public async Task TimeOnlyPk_HasCorrectRulesAndProps(PropertyViewModelData data)
    {
        PropertyViewModel prop = data;

        await Assert.That(prop.IsPrimaryKey).IsTrue();
        await Assert.That(prop.IsAutoGeneratedPrimaryKey).IsFalse();
        await Assert.That(prop.IsCreateOnly).IsTrue();
        await Assert.That(prop.IsRequired).IsTrue();
    }

    [Test]
    [ClassViewModelData(typeof(ExternalParentAsInputOnly), false, true, false)]
    [ClassViewModelData(typeof(ExternalChildAsInputOnly), false, true, false)]
    [ClassViewModelData(typeof(ExternalParentAsOutputOnly), true, false, false)]
    [ClassViewModelData(typeof(ExternalChildAsOutputOnly), true, false, false)]
    [ClassViewModelData(typeof(ReadOnlyEntityUsedAsMethodInput), true, true, false)]
    public async Task PropertySecurityIsUnused_ReflectsActualUsage(
        ClassViewModelData data, bool read, bool init, bool edit)
    {
        await Assert.That(data.ClassViewModel.ClientProperties).All(p =>
        {
            Assert.Equal(read, !p.SecurityInfo.Read.IsUnused);
            Assert.Equal(init, !p.SecurityInfo.Init.IsUnused);
            Assert.Equal(edit, !p.SecurityInfo.Edit.IsUnused);
        });
    }

    [Test]
    [ClassViewModelData(typeof(AppDbContext))]
    public async Task PropertyParent_IsCorrect(ClassViewModelData data)
    {
        var directlyDeclaredProp = data.ClassViewModel.PropertyByName(nameof(AppDbContext.People));
        await Assert.That(directlyDeclaredProp.Parent).IsEqualTo(data.ClassViewModel);
        await Assert.That(directlyDeclaredProp.EffectiveParent).IsEqualTo(data.ClassViewModel);

        var inheritedProp = data.ClassViewModel.PropertyByName(nameof(AppDbContext.Database));
        await Assert.That(inheritedProp.Parent.Name).IsEqualTo(nameof(DbContext));
        await Assert.That(inheritedProp.Parent).IsNotEqualTo(data.ClassViewModel);
        await Assert.That(inheritedProp.EffectiveParent).IsEqualTo(data.ClassViewModel);
    }

    [Test]
    [ClassViewModelData(typeof(Case))]
    public async Task NonHomogenousManyToMany_IsCorrect(ClassViewModelData data)
    {
        var prop = data.ClassViewModel.PropertyByName(nameof(Case.CaseProducts));

        await Assert.That(prop.ManyToManyNearNavigationProperty.Name).IsEqualTo("Case");
        await Assert.That(prop.ManyToManyNearNavigationProperty.ForeignKeyProperty.Name).IsEqualTo("CaseId");
        await Assert.That(prop.ManyToManyFarNavigationProperty.Name).IsEqualTo("Product");
        await Assert.That(prop.ManyToManyFarNavigationProperty.ForeignKeyProperty.Name).IsEqualTo("ProductId");
    }

    [Test]
    [ClassViewModelData(typeof(Person))]
    public async Task HomogeneousManyToMany_IsCorrect(ClassViewModelData data)
    {
        var prop = data.ClassViewModel.PropertyByName(nameof(Person.SiblingRelationships));

        await Assert.That(prop.ManyToManyNearNavigationProperty.Name).IsEqualTo("Person");
        await Assert.That(prop.ManyToManyNearNavigationProperty.ForeignKeyProperty.Name).IsEqualTo("PersonId");
        await Assert.That(prop.ManyToManyFarNavigationProperty.Name).IsEqualTo("PersonTwo");
        await Assert.That(prop.ManyToManyFarNavigationProperty.ForeignKeyProperty.Name).IsEqualTo("PersonTwoId");
    }

    [Test]
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
    public async Task IsClientSerializable_IsCorrect(PropertyViewModelData data, bool expected)
    {
        PropertyViewModel vm = data;
        await Assert.That(vm.IsClientSerializable).IsEqualTo(expected);
    }

    [Test]
    [PropertyViewModelData<ComplexModel>(nameof(ComplexModel.ReferenceNavigation), false)]
    [PropertyViewModelData<ComplexModel>(nameof(ComplexModel.Tests), false)]
    [PropertyViewModelData<ComplexModel>(nameof(ComplexModel.ComplexModelId), false)]
#if NET7_0_OR_GREATER
    [PropertyViewModelData<RequiredAndInitModel>(nameof(RequiredAndInitModel.RequiredRef), true)]
    [PropertyViewModelData<RequiredAndInitModel>(nameof(RequiredAndInitModel.RequiredValue), true)]
    [PropertyViewModelData<RequiredAndInitModel>(nameof(RequiredAndInitModel.RequiredInitRef), true)]
    [PropertyViewModelData<RequiredAndInitModel>(nameof(RequiredAndInitModel.RequiredInitValue), true)]
#endif
    public async Task IsRequired_IsCorrect(PropertyViewModelData data, bool expected)
    {
        PropertyViewModel vm = data;
        await Assert.That(vm.IsRequired).IsEqualTo(expected);
    }

    [Test]
    [SymbolPropertyViewModelData<Person>(
        nameof(Person.Name),
        "Calculated name of the person. eg., Mr. Michael Stokesbary.\nA concatenation of Title, FirstName, and LastName.")]
    public async Task Comment_IsCorrect(PropertyViewModelData data, string expected)
    {
        PropertyViewModel vm = data;
        await Assert.That(vm.Comment).IsEqualTo(expected);
    }


    [Test]
    [PropertyViewModelData<OneToOneParent>(nameof(OneToOneParent.SharedKeyChild1))]
    [PropertyViewModelData<OneToOneParent>(nameof(OneToOneParent.SharedKeyChild2))]
    public async Task OneToOne_ParentNavigations_HasCorrectMetadata(PropertyViewModelData data)
    {
        PropertyViewModel vm = data;
        await Assert.That(vm.Role).IsEqualTo(PropertyRole.ReferenceNavigation);
        await Assert.That(vm.ForeignKeyProperty).IsEqualTo(vm.Parent.PropertyByName(nameof(OneToOneParent.Id)));
        await Assert.That(vm.InverseProperty.Name).IsEqualTo(nameof(OneToOneSharedKeyChild1.Parent));
        await Assert.That(vm.EffectiveParent.PrimaryKey.IsCreateOnly).IsFalse();
    }

    [Test]
    [PropertyViewModelData<OneToOneSharedKeyChild1>(nameof(OneToOneSharedKeyChild1.Parent), "SharedKeyChild1")]
    [PropertyViewModelData<OneToOneSharedKeyChild2>(nameof(OneToOneSharedKeyChild2.Parent), "SharedKeyChild2")]
    public async Task OneToOne_ChildNavigations_HasCorrectMetadata(PropertyViewModelData data, string inverse)
    {
        PropertyViewModel vm = data;
        await Assert.That(vm.Role).IsEqualTo(PropertyRole.ReferenceNavigation);
        await Assert.That(vm.ForeignKeyProperty).IsEqualTo(vm.Parent.PropertyByName("ParentId"));
        await Assert.That(vm.InverseProperty.Name).IsEqualTo(inverse);
        await Assert.That(vm.EffectiveParent.PrimaryKey.IsCreateOnly).IsTrue();
    }

    [Test]
    [PropertyViewModelData<OneToOneParent>(nameof(OneToOneParent.SeparateKeyChild))]
    [PropertyViewModelData<OneToOneParent>(nameof(OneToOneParent.SeparateKeyChildNoIp))]
    public async Task OneToOne_SeparateKey_ParentNavigations_HasCorrectMetadata(PropertyViewModelData data)
    {
        PropertyViewModel vm = data;
        await Assert.That(vm.Role).IsEqualTo(PropertyRole.Value);
        await Assert.That(vm.ForeignKeyProperty).IsNull();
    }

    [Test]
    [PropertyViewModelData<OneToOneSeparateKeyChild>(nameof(OneToOneSeparateKeyChild.Parent))]
    public async Task OneToOne_SeparateKey_ChildNavigations_HasCorrectMetadata(PropertyViewModelData data)
    {
        // See comments in OneToOne.cs about what this is all about

        PropertyViewModel vm = data;
        await Assert.That(vm.Role).IsEqualTo(PropertyRole.ReferenceNavigation);
        await Assert.That(vm.ForeignKeyProperty).IsEqualTo(vm.Parent.PropertyByName("ParentId"));

        // We can't model the other side as a reference navigation,
        // so the inverse property shouldn't be defined.
        await Assert.That(vm.InverseProperty).IsNull();
    }

    [Test]
    [PropertyViewModelData<OneToOneManyChildren>(nameof(OneToOneManyChildren.OneToOneParent))]
    [Description("https://github.com/IntelliTect/Coalesce/commit/513db257dda32b99099355f1a6de0f5fbf367f5a")]
    public async Task ReferenceNavigation_HasCorrectFkWhenPrincipalAlsoParticipatesInOneToOne(PropertyViewModelData data)
    {
        PropertyViewModel prop = data;

        // Precodition: Principal participates in a one-to-one
        await Assert.That(prop.Object!.PrimaryKey.IsForeignKey).IsTrue();

        // Correct foreign key here is ParentId, not the child's PK.
        await Assert.That(prop.Role).IsEqualTo(PropertyRole.ReferenceNavigation);
        await Assert.That(prop.ForeignKeyProperty.Name).IsEqualTo(nameof(OneToOneManyChildren.OneToOneParentId));
    }

    [Test]
    [PropertyViewModelData<OneToOneParent>(nameof(OneToOneParent.ManyChildren))]
    [Description("https://github.com/IntelliTect/Coalesce/commit/513db257dda32b99099355f1a6de0f5fbf367f5a")]
    public async Task CollectionNavigation_HasCorrectFkWhenPrincipalAlsoParticipatesInOneToOne(PropertyViewModelData data)
    {
        PropertyViewModel prop = data;

        // Precodition: Principal participates in a one-to-one
        await Assert.That(prop.EffectiveParent.PrimaryKey.IsForeignKey).IsTrue();

        // Correct foreign key here is ParentId, not the child's PK.
        await Assert.That(prop.Role).IsEqualTo(PropertyRole.CollectionNavigation);
        await Assert.That(prop.ForeignKeyProperty.Name).IsEqualTo(nameof(OneToOneManyChildren.OneToOneParentId));
    }

}