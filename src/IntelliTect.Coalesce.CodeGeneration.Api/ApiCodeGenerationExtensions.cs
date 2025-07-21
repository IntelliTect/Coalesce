using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Linq;

namespace IntelliTect.Coalesce.CodeGeneration.Api;

public static class ApiCodeGenerationExtensions
{
    public static string MvcAnnotation(this SecurityPermission permission)
    {
        if (permission.NoAccess) throw new InvalidOperationException($"Cannot emit an annotation for security level {SecurityPermissionLevels.DenyAll}");
        if (permission.AllowAnonymous) return "[AllowAnonymous]";
        if (permission.HasRoles) return string.Concat(permission.RoleLists.Select(rl =>
            $"[Authorize(Roles={string.Join(",", rl).QuotedStringLiteralForCSharp()})]"));
        return "[Authorize]";
    }

    public static string MapToModelChain(this ValueViewModel param, string mappingContextVar)
    {
        string ret = "";

        var cvm = param.Type.PureType.ClassViewModel;
        if (cvm != null && !cvm.IsCustomDto)
        {
            if (param.Type.IsCollection)
            {
                ret += $"?.Select(_m => _m.MapToNew({mappingContextVar}))";
            }
            else
            {
                ret += $"?.MapToNew({mappingContextVar})";
            }
        }

        if (param.Type.IsCollection)
        {
            if (param.PureType.IsFile)
            {
                ret += $"?.Cast<{param.PureType.FullyQualifiedName}>()";
            }

            if (param.Type.IsArray)
                ret += "?.ToArray()";
            else
                ret += "?.ToList()";
        }

        return ret;
    }
}
