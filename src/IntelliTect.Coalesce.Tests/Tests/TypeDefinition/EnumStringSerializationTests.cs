using IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Testing.Util;
using IntelliTect.Coalesce.TypeDefinition;

namespace IntelliTect.Coalesce.Tests.TypeDefinition;

public class EnumStringSerializationTests
{
    [Test]
    public void IsEnumStringSerializable_WithJsonStringEnumConverter_ReturnsTrue()
    {
        var enumType = ReflectionRepository.Global.GetOrAddType(typeof(StringSerializedEnum));

        Assert.That(enumType.IsEnum, Is.True);
        Assert.That(enumType.IsEnumStringSerializable, Is.True);
    }

    [Test]
    public void IsEnumStringSerializable_WithoutJsonStringEnumConverter_ReturnsFalse()
    {
        var enumType = ReflectionRepository.Global.GetOrAddType(typeof(RegularEnum));

        Assert.That(enumType.IsEnum, Is.True);
        Assert.That(enumType.IsEnumStringSerializable, Is.False);
    }

    [Test]
    public void StringEnumModel_Properties_DetectCorrectSerializationType()
    {
        var classViewModel = ReflectionRepository.Global.GetClassViewModel<StringEnumModel>();

        var stringEnumProperty = classViewModel!.PropertyByName(nameof(StringEnumModel.StringEnum))!;
        var regularEnumProperty = classViewModel.PropertyByName(nameof(StringEnumModel.RegularEnum))!;
        var nullableStringEnumProperty = classViewModel.PropertyByName(nameof(StringEnumModel.NullableStringEnum))!;

        Assert.That(stringEnumProperty.Type.NullableValueUnderlyingType.IsEnumStringSerializable, Is.True);
        Assert.That(regularEnumProperty.Type.NullableValueUnderlyingType.IsEnumStringSerializable, Is.False);
        Assert.That(nullableStringEnumProperty.Type.NullableValueUnderlyingType.IsEnumStringSerializable, Is.True);
    }
}
