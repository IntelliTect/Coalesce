using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace IntelliTect.Coalesce.Helpers;

public static class EnumExtensions
{
    public static string? GetDisplayName(this Enum value)
    {
        return value.GetType()
            .GetMember(value.ToString())
            .First()
            .GetCustomAttribute<DisplayAttribute>()
            ?.GetName()
            ?? value.ToString();
    }
}
