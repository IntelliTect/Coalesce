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
        await Assert.That(enumType.IsEnumStringSerializable).IsTrue();
    }

    [Test]
    public async Task IsEnumStringSerializable_WithoutJsonStringEnumConverter_ReturnsFalse()
    {
        var enumType = ReflectionRepository.Global.GetOrAddType(typeof(RegularEnum));

        await Assert.That(enumType.IsEnum).IsTrue();
        await Assert.That(enumType.IsEnumStringSerializable).IsFalse();
    }

    [Test]
    public async Task StringEnumModel_Properties_DetectCorrectSerializationType()
    {
        var classViewModel = ReflectionRepository.Global.GetClassViewModel<StringEnumModel>()!;

        var stringEnumProperty = classViewModel.PropertyByName(nameof(StringEnumModel.StringEnum))!;
        var regularEnumProperty = classViewModel.PropertyByName(nameof(StringEnumModel.RegularEnum))!;
        var nullableStringEnumProperty = classViewModel.PropertyByName(nameof(StringEnumModel.NullableStringEnum))!;

        await Assert.That(stringEnumProperty.Type.NullableValueUnderlyingType.IsEnumStringSerializable).IsTrue();
        await Assert.That(regularEnumProperty.Type.NullableValueUnderlyingType.IsEnumStringSerializable).IsFalse();
        await Assert.That(nullableStringEnumProperty.Type.NullableValueUnderlyingType.IsEnumStringSerializable).IsTrue();
    }
}
