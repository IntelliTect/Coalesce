using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.Tests.TargetClasses;
using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Tests.Util;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Tests.TypeDefinition;

public class TypeViewModelTests
{
    [Test]
    [Description("https://github.com/IntelliTect/Coalesce/issues/28")]
    // With class name as a substring in a namespace.
    [ClassViewModelData(
        typeof(ComplexModel),
        "MyTestProject.Web.Models", nameof(ComplexModel.Tests),
        "System.Collections.Generic.ICollection<MyTestProject.Web.Models.TestResponse>")]
    [ClassViewModelData(
        typeof(ComplexModel),
        "MyTestProject.Web.Models", nameof(ComplexModel.SingleTest),
        "MyTestProject.Web.Models.TestResponse")]

    // With class name as a distinct namespace.
    [ClassViewModelData(
        typeof(ComplexModel),
        "MyProject.Test.Models", nameof(ComplexModel.Tests),
        "System.Collections.Generic.ICollection<MyProject.Test.Models.TestResponse>")]
    [ClassViewModelData(
        typeof(ComplexModel),
        "MyProject.Test.Models", nameof(ComplexModel.SingleTest),
        "MyProject.Test.Models.TestResponse")]
    public async Task NullableTypeForDto_DoesNotMangleNamespace(
        ClassViewModelData data,
        string dtoOputputNamespace,
        string propertyName,
        string expectedPropertyType
    )
    {
        ClassViewModel vm = data;

        var prop = vm.PropertyByName(propertyName);

        // Precondition: Model's type name is contained in output namespace:
        await Assert.That(dtoOputputNamespace).Contains(prop.PureType.Name);

        // Precondition: Model's type name is contained in its own namespace:
        await Assert.That(prop.PureType.FullNamespace).Contains(prop.PureType.Name);

        var dtoPropType = prop.Type.NullableTypeForDto(isInput: false, dtoNamespace: dtoOputputNamespace);

        await Assert.That(dtoPropType).IsEqualTo(expectedPropertyType);
    }


    [Test]
    [ClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.ValueArray), "int[]")]
    [ClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.ValueNullableArray), "int?[]")]
    [ClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.ValueArrayNullable), "int[]")]
    [ClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.ValueICollection), "System.Collections.Generic.ICollection<int>")]
    [ClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.ValueNullableICollection), "System.Collections.Generic.ICollection<int?>")]
    [ClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.ValueICollectionNullable), "System.Collections.Generic.ICollection<int>")]
    [ClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.RefArray), "MyProject.ExternalChildResponse[]")]
    [ClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.RefNullableArray), "MyProject.ExternalChildResponse[]")]
    [ClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.RefArrayNullable), "MyProject.ExternalChildResponse[]")]
    [ClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.RefICollection), "System.Collections.Generic.ICollection<MyProject.ExternalChildResponse>")]
    [ClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.RefNullableICollection), "System.Collections.Generic.ICollection<MyProject.ExternalChildResponse>")]
    [ClassViewModelData(typeof(ExternalParent), nameof(ExternalParent.RefICollectionNullable), "System.Collections.Generic.ICollection<MyProject.ExternalChildResponse>")]
    public async Task NullableTypeForDto_HandlesCollectionsProperly(
        ClassViewModelData data,
        string propertyName,
        string expectedPropertyType
    )
    {
        ClassViewModel vm = data;

        var prop = vm.PropertyByName(propertyName);

        var dtoPropType = prop.Type.NullableTypeForDto(isInput: false, dtoNamespace: "MyProject");

        await Assert.That(dtoPropType).IsEqualTo(expectedPropertyType);
    }

    [Test]
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
    [ClassViewModelData(typeof(IEntityFrameworkBehaviors<Exception, AppDbContext>),
        "IntelliTect.Coalesce.IEntityFrameworkBehaviors<System.Exception, IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.AppDbContext>")]
    [ClassViewModelData(typeof(Case.Statuses), "IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.Case.Statuses")]
    public async Task FullyQualifiedName_HasCorrectValue(
        ClassViewModelData data,
        string expectedTypeName
    )
    {
        TypeViewModel vm = data;
        var value = vm.FullyQualifiedName;

        await Assert.That(value).IsEqualTo(expectedTypeName);
    }

    [Test]
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
    [ClassViewModelData(typeof(IEntityFrameworkBehaviors<Exception, AppDbContext>),
        "IntelliTect.Coalesce.IEntityFrameworkBehaviors<System.Exception, IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext.AppDbContext>")]
    public async Task VerboseFullyQualifiedName_HasCorrectValue(
        ClassViewModelData data,
        string expectedTypeName
    )
    {
        TypeViewModel vm = data;
        var value = vm.VerboseFullyQualifiedName;

        await Assert.That(value).IsEqualTo(expectedTypeName);
    }

    [Test]
    [ClassViewModelData(typeof(decimal))]
    [ClassViewModelData(typeof(decimal?))]
    [ClassViewModelData(typeof(int))]
    [ClassViewModelData(typeof(int?))]
    [ClassViewModelData(typeof(short))]
    [ClassViewModelData(typeof(short?))]
    [ClassViewModelData(typeof(double))]
    [ClassViewModelData(typeof(double?))]
    public async Task IsNumber_TrueForNumbers(ClassViewModelData data)
    {
        TypeViewModel vm = data;
        await Assert.That(vm.IsNumber).IsTrue();
    }

    [Test]
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
    [ClassViewModelData(typeof(List<InternalUseClass>), true)]
    public async Task IsInternalUse_IsCorrectForAllTypeCombinations(ClassViewModelData data, bool isInternalUse)
    {
        TypeViewModel vm = data;
        await Assert.That(vm.IsInternalUse).IsEqualTo(isInternalUse);
    }

    [Test]
    [ClassViewModelData(typeof(int), false)]
    [ClassViewModelData(typeof(int?), false)]
    [ClassViewModelData(typeof(IWeatherService), true)]
    [ClassViewModelData(typeof(WeatherService), false)]
    public async Task IsInterface_IsCorrect(ClassViewModelData data, bool expected)
        => await Assert.That(((TypeViewModel)data).IsInterface).IsEqualTo(expected);

    [Test]
    [ClassViewModelData(typeof(int), false)]
    [ClassViewModelData(typeof(int?), false)]
    [ClassViewModelData(typeof(IWeatherService), false)]
    [ClassViewModelData(typeof(WeatherService), true)]
    public async Task IsClass_IsCorrect(ClassViewModelData data, bool expected)
        => await Assert.That(((TypeViewModel)data).IsClass).IsEqualTo(expected);

    [Test]
    [ClassViewModelData(typeof(int), false)]
    [ClassViewModelData(typeof(int?), false)]
    [ClassViewModelData(typeof(DateTime), false)]
    [ClassViewModelData(typeof(string), false)]
    [ClassViewModelData(typeof(IFile), false)]
    [ClassViewModelData(typeof(string[]), false)]
    [ClassViewModelData(typeof(List<string>), false)]
    [ClassViewModelData(typeof(IWeatherService), true)]
    [ClassViewModelData(typeof(WeatherService), true)]
    public async Task IsPoco_IsCorrect(ClassViewModelData data, bool expected)
        => await Assert.That(((TypeViewModel)data).IsPOCO).IsEqualTo(expected);

    [Test]
    [ClassViewModelData(typeof(int), false)]
    [ClassViewModelData(typeof(int?), false)]
    [ClassViewModelData(typeof(DateTime), false)]
    [ClassViewModelData(typeof(string), false)]
    [ClassViewModelData(typeof(WeatherService[]), false)]
    [ClassViewModelData(typeof(IWeatherService), true)]
    [ClassViewModelData(typeof(WeatherService), true)]
    public async Task HasClassViewModel_IsCorrect(ClassViewModelData data, bool expected)
        => await Assert.That(((TypeViewModel)data).HasClassViewModel).IsEqualTo(expected);

    [Test]
    [ClassViewModelData(typeof(Case.Statuses))]
    public async Task EnumValues_IsCorrect(ClassViewModelData data)
    {
        // TODO: TUnit migration - Assert.Collection had element inspectors. Manually add assertions for each element.
        await Assert.That(data.TypeViewModel.EnumValues).HasCount(5);
    }

    [Test]
    [ClassViewModelData(typeof(TestBehaviors<,>))]
    public async Task UnconstructedGeneric(ClassViewModelData data)
    {
        var type = data.TypeViewModel;
        await Assert.That(type.IsGeneric).IsTrue();
        await Assert.That(type.PureType).IsEqualTo(type);
        await Assert.That(type.FirstTypeArgument).IsNull();
    }

    [Test]
    [ClassViewModelData(typeof(ComplexInheritanceDerived))]
    public async Task DerivedClass_GenericArgumentsFor_ReturnsDerivedInterfaceUsage(ClassViewModelData data)
    {
        // It is important for ReflectionRepository._generatedParamDtos that
        // we get the most specific interface implementation when asking about the
        // symbol that satisfies a particular interface. Otherwise, it'll pick the base type
        // when trying to find the `IGeneratedParameterDto<>` symbol for a derived type.

        var type = data.TypeViewModel;

        await Assert.That(type.GenericArgumentsFor(typeof(IAmInheritedMultipleTimes<>))[0].Name).IsEqualTo(nameof(ComplexInheritanceDerived));
    }
}