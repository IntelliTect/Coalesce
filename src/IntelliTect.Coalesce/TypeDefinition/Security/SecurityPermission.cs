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
        {
            HasAttribute = true;
            PermissionLevel = level;
            Roles = PermissionLevel != SecurityPermissionLevels.DenyAll ? roles ?? "" : string.Empty;
            NoAccess = PermissionLevel == SecurityPermissionLevels.DenyAll;
            Name = name;
        }

        public bool HasAttribute { get; }
        public SecurityPermissionLevels PermissionLevel { get; } = SecurityPermissionLevels.AllowAuthorized;

        public bool AllowAnonymous => PermissionLevel == SecurityPermissionLevels.AllowAll;


        public override bool IsAllowed(ClaimsPrincipal? user)
        {
            if (NoAccess) return false;
            if (AllowAnonymous) return true;

            var userIsAuthenticated = user?.Identity?.IsAuthenticated ?? false;

            if (HasRoles)
            {
                return userIsAuthenticated && RoleList.Any(s => user!.IsInRole(s));
            }

            return userIsAuthenticated;
        }

        public override string ToString()
        {
            if (NoAccess) return $"Deny All";
            if (AllowAnonymous) return $"Allow All (Including Anonymous)";
            if (HasRoles) return $"Allow Roles: {string.Join(", ", RoleList)}";
            return $"Allow Authenticated";
        }
    }
}