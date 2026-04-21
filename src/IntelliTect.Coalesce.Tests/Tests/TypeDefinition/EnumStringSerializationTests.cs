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
        var enumType = ReflectionRepository.Global.GetOrAddType(typeof(Case.Statuses));

        await Assert.That(enumType.IsEnum).IsTrue();
        await Assert.That(enumType.IsEnumStringSerializable).IsFalse();
    }

    [Test]
    public async Task ComplexModel_Properties_DetectCorrectSerializationType()
    {
        var classViewModel = ReflectionRepository.Global.GetClassViewModel<ComplexModel>()!;

        var stringEnumProperty = classViewModel.PropertyByName(nameof(ComplexModel.StringEnum))!;
        var regularEnumProperty = classViewModel.PropertyByName(nameof(ComplexModel.EnumWithDefault))!;
        var nullableStringEnumProperty = classViewModel.PropertyByName(nameof(ComplexModel.StringEnumNullable))!;

        await Assert.That(stringEnumProperty.Type.NullableValueUnderlyingType.IsEnumStringSerializable).IsTrue();
        await Assert.That(regularEnumProperty.Type.NullableValueUnderlyingType.IsEnumStringSerializable).IsFalse();
        await Assert.That(nullableStringEnumProperty.Type.NullableValueUnderlyingType.IsEnumStringSerializable).IsTrue();
    }
}
