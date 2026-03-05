#if NET10_0_OR_GREATER

using IntelliTect.Coalesce.TypeDefinition;
using System.Reflection;

namespace IntelliTect.Coalesce.Tests.Tests.TypeDefinition;

/// <summary>
/// C# 14 extension block that produces a compiler-generated type
/// which is generic but whose Name does not contain a backtick character.
/// </summary>
public static class ExtensionBlockTestHelpers
{
    extension<T>(HashSet<T> hashSet)
    {
    }
}

public class GenericExtensionBlockTests
{
    [Test]
    public async Task ReflectionTypeViewModel_HandlesGenericExtensionBlock()
    {
        // C# 14 extension blocks generate types that are generic
        // but whose Name does not contain a backtick (`) character,
        // unlike standard generic types (e.g. List`1).
        var extensionType = System.Reflection.Assembly.GetExecutingAssembly().GetTypes()
            .FirstOrDefault(t => t.IsGenericType && !t.Name.Contains('`'));

        // Precondition: the extension block type exists and has the expected characteristics
        await Assert.That(extensionType).IsNotNull()
            .Because("a C# 14 extension block should produce a generic type without a backtick in its name");

        // This should not throw, but currently crashes with ArgumentOutOfRangeException
        // in GetTypeName/GetVerboseTypeName because they assume all generic type names
        // contain a backtick character. When IndexOf('`') returns -1,
        // Substring(0, -1) throws.
        var vm = new ReflectionTypeViewModel(extensionType!);

        await Assert.That(vm.FullyQualifiedName).IsNotNull();
        await Assert.That(vm.VerboseFullyQualifiedName).IsNotNull();
    }
}

#endif
