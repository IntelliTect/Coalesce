using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

#nullable enable

namespace IntelliTect.Coalesce.TypeDefinition
{
    public class PropertySecurityPermission : SecurityPermissionBase
    {
        private Func<PropertyViewModel, HashSet<PropertyViewModel>, bool?> ComputeIsUnused { get; }
        private PropertyViewModel Prop { get; }

        internal PropertySecurityPermission(
            PropertyViewModel prop,
            string name,
            IEnumerable<IReadOnlyList<string>>? roleLists, 
            Func<PropertyViewModel, HashSet<PropertyViewModel>, bool?> computeIsUnused
        )
        {
            NoAccess = false;
            RoleLists = NoAccess || roleLists is null 
                ? NoRoles 
                : (roleLists.Where(l => l.Any()).ToList().AsReadOnly() ?? NoRoles);
            Name = name;
            Prop = prop;
            
            // Unused-ness is lazily computed because if we did it eagerly,
            // we could enter infinte recusion when examining the SecurityInfo of other properties.

            // Another note about analysys of IsUnused - it shouldn't be used to affect code generation output
            // because it will yield incorrect results when multiple code gens with different RootTypesWhitelist
            // are ran against the same codebase (but we do it anyway because otherwise the ergonomics of using c#
            // `required`/`init` properties or record types are really bad).
            ComputeIsUnused = computeIsUnused;
        }

        internal PropertySecurityPermission(
            PropertyViewModel prop,
            string name,
            string denyReason
        )
        {
            NoAccess = true;
            Roles = string.Empty;
            Name = name;
            Reason = denyReason;
            Prop = prop;
            ComputeIsUnused = (_, _) => true; // a NoAccess permission is always unused
        }

        /// <summary>
        /// Whether static analysis has determined that the action on the property is not used by any API endpoint.
        /// </summary>
        public bool IsUnused => ComputeIsUnused(Prop, new())!.Value;

        public override bool IsAllowed(ClaimsPrincipal? user)
        {
            if (NoAccess) return false;
            if (RoleLists.Count == 0) return true;
            return user != null && RoleLists.All(rl => rl.Any(f => user.IsInRole(f)));
        }

        public override string ToString()
        {
            if (NoAccess) return $"Deny";
            if (HasRoles) return $"Allow Roles: {string.Join(" && ", RoleLists.Select(rl => "(" + string.Join(", ", rl) + ")"))}";
            return $"Allow";
        }
    }
}
