using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.Tests.Util;
using IntelliTect.Coalesce.TypeDefinition;
using Xunit;

namespace IntelliTect.Coalesce.Tests.Tests.TypeDefinition
{
    public class EnumStringSerializationTests
    {
        public EnumStringSerializationTests()
        {
            ReflectionRepositoryFactory.Initialize();
        }

        [Fact]
        public void IsEnumStringSerializable_WithJsonStringEnumConverter_ReturnsTrue()
        {
            // Arrange
            var enumType = ReflectionRepository.Global.GetOrAddType(typeof(StringSerializedEnum));

            // Act & Assert
            Assert.True(enumType.IsEnum);
            Assert.True(enumType.IsEnumStringSerializable);
        }

        [Fact]
        public void IsEnumStringSerializable_WithoutJsonStringEnumConverter_ReturnsFalse()
        {
            // Arrange
            var enumType = ReflectionRepository.Global.GetOrAddType(typeof(RegularEnum));

            // Act & Assert
            Assert.True(enumType.IsEnum);
            Assert.False(enumType.IsEnumStringSerializable);
        }

        [Fact]
        public void StringEnumModel_Properties_DetectCorrectSerializationType()
        {
            // Arrange
            var classViewModel = ReflectionRepository.Global.GetClassViewModel<StringEnumModel>();

            // Act
            var stringEnumProperty = classViewModel.PropertyByName(nameof(StringEnumModel.StringEnum));
            var regularEnumProperty = classViewModel.PropertyByName(nameof(StringEnumModel.RegularEnum));
            var nullableStringEnumProperty = classViewModel.PropertyByName(nameof(StringEnumModel.NullableStringEnum));

            // Assert
            Assert.True(stringEnumProperty.Type.NullableValueUnderlyingType.IsEnumStringSerializable);
            Assert.False(regularEnumProperty.Type.NullableValueUnderlyingType.IsEnumStringSerializable);
            Assert.True(nullableStringEnumProperty.Type.NullableValueUnderlyingType.IsEnumStringSerializable);
        }
    }
}