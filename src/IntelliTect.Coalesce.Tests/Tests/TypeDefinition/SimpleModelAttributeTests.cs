using IntelliTect.Coalesce.Testing.TargetClasses;
using IntelliTect.Coalesce.Testing.Util;
using IntelliTect.Coalesce.TypeDefinition;
using System.Linq;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.Tests.TypeDefinition;

public class SimpleModelAttributeTests
{
    [Test]
    public async Task SimpleModelAttribute_AddsTypeAsSimpleModel()
    {
        // Arrange
        var repo = ReflectionRepositoryFactory.Reflection;
        
        // Act
        var simpleModelTypes = repo.ExternalTypes;
        var simpleModelTarget = simpleModelTypes
            .FirstOrDefault(t => t.Name == nameof(SimpleModelTarget));
        
        // Assert
        await Assert.That(simpleModelTarget).IsNotNull();
        await Assert.That(simpleModelTarget.Name).IsEqualTo(nameof(SimpleModelTarget));
    }

    [Test]
    public async Task TypeWithoutSimpleModelAttribute_NotInExternalTypes()
    {
        // Arrange
        var repo = ReflectionRepositoryFactory.Reflection;
        
        // Act
        var simpleModelTypes = repo.ExternalTypes;
        var notMarkedType = simpleModelTypes
            .FirstOrDefault(t => t.Name == nameof(NotMarkedAsSimpleModel));
        
        // Assert
        await Assert.That(notMarkedType).IsNull();
    }

    [Test]
    public async Task CoalesceAttributeOnly_DoesNotAddTypeAsSimpleModel()
    {
        // Arrange
        var repo = ReflectionRepositoryFactory.Reflection;
        
        // Act
        var simpleModelTypes = repo.ExternalTypes;
        var coalesceOnlyType = simpleModelTypes
            .FirstOrDefault(t => t.Name == nameof(CoalesceOnlyTarget));
        
        // Assert
        await Assert.That(coalesceOnlyType).IsNull();
    }
}
