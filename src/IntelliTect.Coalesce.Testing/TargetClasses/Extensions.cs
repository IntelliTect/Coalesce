
using System.Collections.Generic;

namespace IntelliTect.Coalesce.Testing.TargetClasses;

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
