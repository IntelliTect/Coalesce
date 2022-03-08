using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Helpers;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public class MethodSecurityInfo
    {
        public MethodSecurityInfo(SecurityPermission execute)
        {
            Execute = execute;
        }

        public SecurityPermission Execute { get; }


        /// <summary>
        /// Returns an MVC Action annotation for executing methods
        /// </summary>
        public string ExecuteAnnotation
        {
            get
            {
                if (Execute.NoAccess) throw new InvalidOperationException($"Cannot emit an annotation for security level {SecurityPermissionLevels.DenyAll}");
                if (Execute.AllowAnonymous) return "[AllowAnonymous]";
                if (Execute.HasRoles) return $"[Authorize(Roles=\"{Execute.AttributeRoleList}\")]";
                return "[Authorize]";
            }
        }

        public bool IsExecuteAllowed(ClaimsPrincipal user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            if (Execute.HasAttribute)
            {
                if (Execute.AllowAnonymous) return true;
                if (Execute.HasRoles)
                    return Execute.RoleList.Any(s => user.IsInRole(s));
            }

            return user.Identity?.IsAuthenticated ?? false;
        }

        public override string ToString() => $"Execute: {Execute} ";
    }
}
