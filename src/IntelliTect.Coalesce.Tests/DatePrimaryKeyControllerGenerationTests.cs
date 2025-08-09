using System;
using IntelliTect.Coalesce.CodeGeneration.Api.Generators;
using IntelliTect.Coalesce.CodeGeneration.Generation;
using IntelliTect.Coalesce.Tests.TargetClasses.TestDbContext;
using IntelliTect.Coalesce.TypeDefinition;
using Xunit;

namespace IntelliTect.Coalesce.Tests
{
    public class DatePrimaryKeyControllerGenerationTests
    {
        [Theory]
        [InlineData(typeof(DateTimeEntity), "DateTime")]
        [InlineData(typeof(DateOnlyEntity), "DateOnly")]
        [InlineData(typeof(DateTimeOffsetEntity), "DateTimeOffset")]
        public void GeneratedController_WithDatePrimaryKey_UsesStringParameterAndConversion(Type entityType, string expectedTypeName)
        {
            // Arrange
            var repository = new ReflectionRepository();
            var classViewModel = repository.GetClassViewModel(entityType);
            var services = new GeneratorServices(repository);
            var generator = new ModelApiController(services);
            
            // Act - generate controller for entity with date primary key
            generator.Model = classViewModel;
            var generatedCode = generator.GenerateOutput();
            
            // Assert - should use string parameter instead of date type for route parameters
            Assert.Contains("string id", generatedCode);
            Assert.Contains(@"[HttpGet(""get/{id}"")]", generatedCode);
            Assert.Contains(@"[HttpPost(""delete/{id}"")]", generatedCode);
            
            // Should contain conversion logic
            Assert.Contains($"var parsedId = ({expectedTypeName})Convert.ChangeType(id, typeof({expectedTypeName}));", generatedCode);
            Assert.Contains("return GetImplementation(parsedId, parameters, dataSource);", generatedCode);
            Assert.Contains("return DeleteImplementation(parsedId, new DataSourceParameters(), dataSource, behaviors);", generatedCode);
        }

        [Fact]
        public void GeneratedController_WithIntPrimaryKey_UsesOriginalBehavior()
        {
            // Arrange
            var repository = new ReflectionRepository();
            var classViewModel = repository.GetClassViewModel<Person>();
            var services = new GeneratorServices(repository);
            var generator = new ModelApiController(services);
            
            // Act - generate controller for entity with int primary key
            generator.Model = classViewModel;
            var generatedCode = generator.GenerateOutput();
            
            // Assert - should continue to use int parameter (not string)
            Assert.Contains("int id", generatedCode);
            Assert.DoesNotContain("string id", generatedCode);
            Assert.DoesNotContain("Convert.ChangeType", generatedCode);
            
            // Should use inline lambda (original behavior)
            Assert.Contains("=> GetImplementation(id, parameters, dataSource);", generatedCode);
            Assert.Contains("=> DeleteImplementation(id, new DataSourceParameters(), dataSource, behaviors);", generatedCode);
        }
    }
}