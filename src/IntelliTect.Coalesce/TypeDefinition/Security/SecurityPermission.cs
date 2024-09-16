using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public class SecurityPermission : SecurityPermissionBase
    {
        internal SecurityPermission(string name)
        {
            HasAttribute = false;
            Name = name;
        }

        internal SecurityPermission(SecurityPermissionLevels level, string? roles, string name)
            : this(level, SplitRoles(roles), name) { }

        internal SecurityPermission(SecurityPermissionLevels level, IEnumerable<string>? roles, string name)
        {
            HasAttribute = true;
            PermissionLevel = level;
            RoleLists = PermissionLevel == SecurityPermissionLevels.DenyAll || roles is null || !roles.Any()
                ? NoRoles
                : new List<IReadOnlyList<string>> { roles.ToList().AsReadOnly() }.AsReadOnly();
            NoAccess = PermissionLevel == SecurityPermissionLevels.DenyAll;
            Name = name;
        }

        public bool HasAttribute { get; }
        public SecurityPermissionLevels PermissionLevel { get; } = SecurityPermissionLevels.AllowAuthenticated;

        public bool AllowAnonymous => PermissionLevel == SecurityPermissionLevels.AllowAll;


        public override bool IsAllowed(ClaimsPrincipal? user)
        {
            if (NoAccess) return false;
            if (AllowAnonymous) return true;

            var userIsAuthenticated = user?.Identity?.IsAuthenticated ?? false;

            if (HasRoles)
            {
                return userIsAuthenticated && RoleLists.All(rl => rl.Any(s => user!.IsInRole(s)));
            }

            return userIsAuthenticated;
        }

        public override string ToString()
        {
            if (NoAccess) return $"Deny All";
            if (AllowAnonymous) return $"Allow All (Including Anonymous)";
            if (HasRoles) return $"Allow Roles: {string.Join(" && ", RoleLists.Select(rl => "(" + string.Join(", ", rl) + ")"))}";
            return $"Allow Authenticated";
        }
    }
}