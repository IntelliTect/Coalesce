using IntelliTect.Coalesce.Tests.TargetClasses;
using IntelliTect.Coalesce.Tests.Util;
using IntelliTect.Coalesce.TypeDefinition;
using System.Linq;

namespace IntelliTect.Coalesce.Tests.TypeDefinition;

public class SimpleModelAttributeTests
{
    [Fact]
    public void CoalesceAttributeOnly_AddsTypeAsSimpleModel()
    {
        // Arrange
        var repo = ReflectionRepositoryFactory.Reflection;
        
        // Act
        var simpleModelTypes = repo.ExternalTypes;
        var simpleModelTarget = simpleModelTypes
            .FirstOrDefault(t => t.Name == nameof(SimpleModelTarget));
        
        // Assert
        Assert.NotNull(simpleModelTarget);
        Assert.Equal(nameof(SimpleModelTarget), simpleModelTarget.Name);
    }

    [Fact]
    public void TypeWithoutCoalesceAttribute_NotInExternalTypes()
    {
        // Arrange
        var repo = ReflectionRepositoryFactory.Reflection;
        
        // Act
        var simpleModelTypes = repo.ExternalTypes;
        var notMarkedType = simpleModelTypes
            .FirstOrDefault(t => t.Name == nameof(NotMarkedAsSimpleModel));
        
        // Assert
        Assert.Null(notMarkedType);
    }
}