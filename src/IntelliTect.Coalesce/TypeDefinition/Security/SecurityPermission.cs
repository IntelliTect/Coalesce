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
        internal SecurityPermission()
        {
            HasAttribute = false;
        }

        internal SecurityPermission(SecurityPermissionLevels level, string? roles, string name)
        {
            HasAttribute = true;
            PermissionLevel = level;
            Roles = PermissionLevel != SecurityPermissionLevels.DenyAll ? roles ?? "" : string.Empty;
            Name = name;
        }

        public bool HasAttribute { get; }
        public SecurityPermissionLevels PermissionLevel { get; } = SecurityPermissionLevels.AllowAuthorized;

        public bool AllowAnonymous => PermissionLevel == SecurityPermissionLevels.AllowAll;
        public bool NoAccess => PermissionLevel == SecurityPermissionLevels.DenyAll;

        /// <summary>
        /// C# code representing a Microsoft.AspNetCore.Authorization annotation that will apply security to an action on a controller.
        /// </summary>
        public string MvcAnnotation
        {
            get
            {
                if (NoAccess) throw new InvalidOperationException($"Cannot emit an annotation for security level {SecurityPermissionLevels.DenyAll}");
                if (AllowAnonymous) return "[AllowAnonymous]";
                if (HasRoles) return $"[Authorize(Roles={string.Join(",", RoleList).QuotedStringLiteralForCSharp()})]";
                return "[Authorize]";
            }
        }

        /// <summary>
        /// Return whether the action is generally allowed, without taking into account any particular user.
        /// </summary>
        public bool IsAllowed() => !NoAccess;

        /// <summary>
        /// Return whether the action is allowed for the specified user.
        /// </summary>
        public bool IsAllowed(ClaimsPrincipal? user)
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
            if (HasRoles) return $"Allow Only Authorized Roles: {string.Join(", ", RoleList)}";
            return $"Allow All Authenticated Users";
        }
    }
}