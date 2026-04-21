using IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Testing.Util;
using IntelliTect.Coalesce.TypeDefinition;

namespace IntelliTect.Coalesce.Tests.TypeDefinition;

public class EnumStringSerializationTests
{
    [Test]
    public async Task IsEnumStringSerializable_WithJsonStringEnumConverter_ReturnsTrue()
    {
        var enumType = ReflectionRepository.Global.GetOrAddType(typeof(StringSerializedEnum));

        await Assert.That(enumType.IsEnum).IsTrue();
        await Assert.That(enumType.IsStringEnum).IsTrue();
    }

    [Test]
    public async Task IsEnumStringSerializable_WithoutJsonStringEnumConverter_ReturnsFalse()
    {
        var enumType = ReflectionRepository.Global.GetOrAddType(typeof(Case.Statuses));

        await Assert.That(enumType.IsEnum).IsTrue();
        await Assert.That(enumType.IsStringEnum).IsFalse();
    }

    [Test]
    public async Task ComplexModel_Properties_DetectCorrectSerializationType()
    {
        var classViewModel = ReflectionRepository.Global.GetClassViewModel<ComplexModel>()!;

        var stringEnumProperty = classViewModel.PropertyByName(nameof(ComplexModel.StringEnum))!;
        var regularEnumProperty = classViewModel.PropertyByName(nameof(ComplexModel.EnumWithDefault))!;
        var nullableStringEnumProperty = classViewModel.PropertyByName(nameof(ComplexModel.StringEnumNullable))!;

        await Assert.That(stringEnumProperty.Type.NullableValueUnderlyingType.IsStringEnum).IsTrue();
        await Assert.That(regularEnumProperty.Type.NullableValueUnderlyingType.IsStringEnum).IsFalse();
        await Assert.That(nullableStringEnumProperty.Type.NullableValueUnderlyingType.IsStringEnum).IsTrue();
    }
}
