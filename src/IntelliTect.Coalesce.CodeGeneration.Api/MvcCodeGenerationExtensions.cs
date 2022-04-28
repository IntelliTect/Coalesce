using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.TypeDefinition;
using IntelliTect.Coalesce.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace IntelliTect.Coalesce.CodeGeneration.Api
{
    public static class MvcCodeGenerationExtensions
    {
        public static string MvcAnnotation(this SecurityPermission permission)
        {
            if (permission.NoAccess) throw new InvalidOperationException($"Cannot emit an annotation for security level {SecurityPermissionLevels.DenyAll}");
            if (permission.AllowAnonymous) return "[AllowAnonymous]";
            if (permission.HasRoles) return $"[Authorize(Roles={string.Join(",", permission.RoleList).QuotedStringLiteralForCSharp()})]";
            return "[Authorize]";
        }
    }
}
