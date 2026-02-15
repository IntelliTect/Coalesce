using System.Reflection;

namespace IntelliTect.Coalesce.Helpers;

public static class Cloner
{
    /// <summary>
    /// Makes a shallow copy of an object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    public static T Copy<T>(this T source) where T : class
    {
        return (T)_MemberwiseCloneMethodInfo.Invoke(source, null)!;
    }

    private static MethodInfo _MemberwiseCloneMethodInfo = typeof(object)
        .GetMethod("MemberwiseClone", BindingFlags.NonPublic | BindingFlags.Instance)!;
}


