using IntelliTect.Coalesce.CodeGeneration.Api.Generators;
using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Reflection;
using Xunit;

namespace IntelliTect.Coalesce.Tests
{
    public class DateTimePrimaryKeyTests
    {
        [Theory]
        [InlineData(typeof(DateTimeEntity), typeof(DateTime))]
        [InlineData(typeof(DateOnlyEntity), typeof(DateOnly))]
        [InlineData(typeof(DateTimeOffsetEntity), typeof(DateTimeOffset))]
        public void DatePrimaryKey_ControllerGeneration_GeneratesCorrectParameterType(Type entityType, Type expectedKeyType)
        {
            // Arrange
            var repository = new ReflectionRepository();
            var classViewModel = repository.GetClassViewModel(entityType);
            
            // Act & Assert
            Assert.NotNull(classViewModel.PrimaryKey);
            Assert.Equal(expectedKeyType, classViewModel.PrimaryKey.Type.TypeInfo);
            
            // This test demonstrates the current behavior - the primary key type is correctly identified
            // But we need to verify that the generated controller can handle it properly
        }

        [Theory]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(DateOnly))]  
        [InlineData(typeof(DateTimeOffset))]
        public void DateType_IsDetectedAsDate(Type dateType)
        {
            // Arrange
            var repository = new ReflectionRepository();
            var typeViewModel = repository.GetTypeViewModel(dateType);
            
            // Act & Assert
            Assert.True(typeViewModel.IsDate || typeViewModel.IsDateOrTime);
        }
    }
}