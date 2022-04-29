using System.Linq;
using System.Security.Claims;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public class PropertySecurityPermission : SecurityPermissionBase
    {
        internal PropertySecurityPermission(bool allow, string? roles, string name)
        {
            NoAccess = !allow;
            Roles = NoAccess ? string.Empty : roles ?? "";
            Name = name;
        }

        public override bool IsAllowed(ClaimsPrincipal? user)
        {
            if (NoAccess) return false;
            if (RoleList.Count == 0) return true;
            return user != null && RoleList.Any(f => user.IsInRole(f));
        }

        public override string ToString()
        {
            if (NoAccess) return $"Deny";
            if (HasRoles) return $"Allow Roles: {string.Join(", ", RoleList)}";
            return $"Allow";
        }
    }
}
