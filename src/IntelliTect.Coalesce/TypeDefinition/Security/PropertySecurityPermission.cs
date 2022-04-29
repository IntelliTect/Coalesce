using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

#nullable enable

namespace IntelliTect.Coalesce.TypeDefinition
{
    public class PropertySecurityPermission : SecurityPermissionBase
    {
        private readonly Func<PropertyViewModel, HashSet<PropertyViewModel>, bool?> computeIsUnused;
        private PropertyViewModel prop { get; }

        internal PropertySecurityPermission(
            PropertyViewModel prop, 
            bool allow, 
            string? roles, 
            string name, 
            Func<PropertyViewModel, HashSet<PropertyViewModel>, bool?>? computeIsUnused = null
        )
        {
            NoAccess = !allow;
            Roles = NoAccess ? string.Empty : roles ?? "";
            Name = name;
            this.prop = prop;
            
            // Unused-ness is lazily computed because if we did it eagerly,
            // we could enter infinte recusion when examining the SecurityInfo of other properties.

            // Another note about analysys of IsUnused - it cannot be used to affect code generation output
            // because it will yield incorrect results when multiple code gens with different RootTypesWhitelist
            // are ran against the same codebase. 
            this.computeIsUnused = computeIsUnused ?? (allow ? (_, _) => false : (_, _) => true);
        }

        /// <summary>
        /// Whether static analysis has determined that the action on the property is not used by any API endpoint.
        /// </summary>
        public bool IsUnused => computeIsUnused(prop, new())!.Value;

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
