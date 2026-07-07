using IntelliTect.Coalesce.Mapping;
using IntelliTect.Coalesce.Models;
using IntelliTect.Coalesce.Testing;
using IntelliTect.Coalesce.Testing.TargetClasses;
using IntelliTect.Coalesce.Testing.TargetClasses.TestDbContext;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IntelliTect.Coalesce.CodeGeneration.Tests;

public class DtoMappingTests
{
    private static dynamic NewDto(string typeName)
        => Activator.CreateInstance(
            CodeGenTestBase.WebAssembly.Value.GetType($"MyProject.Models.{typeName}", throwOnError: true)!)!;

    // ----- Directly against the polymorphic base's MapToModelOrNew -----

    /// <summary>
    /// Invokes the base <c>AbstractModelParameter.MapToModelOrNew(AbstractModel, ...)</c> overload
    /// (resolved off the dynamically-generated web assembly) - this is the overload the generated
    /// setter code calls for a polymorphic property, and where the derived-type dispatch lives.
    /// </summary>
    private static AbstractModel MapToModelOrNew(
        string dtoTypeName, AbstractModel existing, Action<dynamic> configureDto)
    {
        dynamic dto = NewDto(dtoTypeName);
        configureDto(dto);

        var mapMethod = CodeGenTestBase.WebAssembly.Value
            .GetType("MyProject.Models.AbstractModelParameter", throwOnError: true)!
            .GetMethod(nameof(IGeneratedParameterDto<AbstractModel>.MapToModelOrNew),
                new[] { typeof(AbstractModel), typeof(IMappingContext) })!;

        var context = new MappingContext(new ClaimsPrincipal(), null);
        return (AbstractModel)mapMethod.Invoke(dto, new object[] { existing, context })!;
    }

    [Test]
    public async Task MapToModelOrNew_WhenChangingPolymorphicType_ReturnsNewInstanceOfCorrectType()
    {
        var existing = new AbstractImpl2 { Id = 1, Impl2OnlyField = "old" };

        var result = MapToModelOrNew("AbstractImpl1Parameter", existing, dto =>
        {
            dto.Id = 1;
            dto.Impl1OnlyField = "new";
        });

        // A new AbstractImpl1 rather than the AbstractImpl2 that was passed in.
        await Assert.That(result.GetType()).IsEqualTo(typeof(AbstractImpl1));
        await Assert.That(((AbstractImpl1)result).Impl1OnlyField).IsEqualTo("new");
    }

    [Test]
    public async Task MapToModelOrNew_WhenPolymorphicTypeUnchanged_UpdatesInPlace()
    {
        var existing = new AbstractImpl1 { Id = 1, Impl1OnlyField = "old" };

        var result = MapToModelOrNew("AbstractImpl1Parameter", existing, dto =>
        {
            dto.Id = 1;
            dto.Impl1OnlyField = "new";
        });

        await Assert.That(result).IsSameReferenceAs(existing);
        await Assert.That(((AbstractImpl1)result).Impl1OnlyField).IsEqualTo("new");
    }

    // ----- Through a parent type that has the polymorphic value as a child property -----

    /// <summary>
    /// Maps an <see cref="ExternalPolyHolder"/> DTO - whose polymorphic child is set to an instance
    /// of <paramref name="childDtoTypeName"/> - onto an existing holder entity. This exercises the
    /// generated `entity.PolyChild = PolyChild?.MapToModelOrNew(entity.PolyChild, context)` call site,
    /// i.e. mapping a polymorphic value that is a property on some other type.
    /// </summary>
    private static ExternalPolyBase MapChildOnto(
        ExternalPolyHolder existing, string childDtoTypeName, Action<dynamic> configureChild)
    {
        dynamic child = NewDto(childDtoTypeName);
        configureChild(child);

        dynamic holder = NewDto("ExternalPolyHolderParameter");
        holder.PolyChild = child;
        holder.MapTo(existing, new MappingContext(new ClaimsPrincipal(), null));

        return existing.PolyChild;
    }

    [Test]
    public async Task MapToModelOrNew_WhenChildPolymorphicTypeChanges_ReplacesWithNewInstance()
    {
        var existing = new ExternalPolyHolder { PolyChild = new ExternalPolyImplB { BField = "old" } };

        var child = MapChildOnto(existing, "ExternalPolyImplAParameter", c => c.AField = "new");

        // The child must be a new ExternalPolyImplA, not the ExternalPolyImplB it replaced.
        await Assert.That(child.GetType()).IsEqualTo(typeof(ExternalPolyImplA));
        await Assert.That(((ExternalPolyImplA)child).AField).IsEqualTo("new");
    }

    [Test]
    public async Task MapToModelOrNew_WhenChildPolymorphicTypeUnchanged_UpdatesInPlace()
    {
        var original = new ExternalPolyImplA { AField = "old" };
        var existing = new ExternalPolyHolder { PolyChild = original };

        var child = MapChildOnto(existing, "ExternalPolyImplAParameter", c => c.AField = "new");

        // Same concrete type => the existing instance is updated in place.
        await Assert.That(child).IsSameReferenceAs(original);
        await Assert.That(((ExternalPolyImplA)child).AField).IsEqualTo("new");
    }
}
