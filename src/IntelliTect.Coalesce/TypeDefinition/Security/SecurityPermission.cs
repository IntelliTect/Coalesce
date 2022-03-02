using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IntelliTect.Coalesce.TypeDefinition
{
    public class SecurityPermission
    {

        public SecurityPermission()
        {
            HasAttribute = false;
        }

        public SecurityPermission(SecurityPermissionLevels level, string? roles, string name)
        {
            HasAttribute = true;
            PermissionLevel = level;
            Roles = PermissionLevel != SecurityPermissionLevels.DenyAll ? roles ?? "" : string.Empty;
            Name = name;
        }

        public bool HasAttribute { get; } = false;
        public SecurityPermissionLevels PermissionLevel { get; } = SecurityPermissionLevels.AllowAuthorized;
        public string Roles { get; } = "";
        public string Name { get; } = "";
                

        public bool AllowAnonymous => PermissionLevel == SecurityPermissionLevels.AllowAll;
        public bool NoAccess => PermissionLevel == SecurityPermissionLevels.DenyAll;
        public bool HasRoles => RoleList.Count > 0;

        private IReadOnlyList<string>? _roleList = null;
        public IReadOnlyList<string> RoleList
        {
            get
            {
                if (_roleList != null) return _roleList;

                var list = new List<string>();
                if (!string.IsNullOrEmpty(Roles))
                {
                    // Logic here should mirror ASP.NET Core:
                    // split on commas, then trim each item.
                    // https://github.com/dotnet/aspnetcore/blob/d88935709b8908f371aa97e32a3ce4a74af2368f/src/Security/Authorization/Core/src/AuthorizationPolicy.cs#L155
                    var roles = Roles.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    list.AddRange(roles.Where(r => !string.IsNullOrWhiteSpace(r)).Select(r => r.Trim()).Distinct());
                }
                return _roleList = list.AsReadOnly();
            }
        }

        public string AttributeRoleList => string.Join(",", RoleList);

        public string ToStringWithName() => $"{Name}: {ToString()}";

        public override string ToString()
        {
            if (HasAttribute)
            {
                if (PermissionLevel == SecurityPermissionLevels.AllowAll) return $"Allow All";
                else if (PermissionLevel == SecurityPermissionLevels.DenyAll) return $"Deny All";
                else return $"Allow Authorized Roles: {string.Join(", ", RoleList)}";
            }
            return $"Allow All Authorized";
        }
    }
}