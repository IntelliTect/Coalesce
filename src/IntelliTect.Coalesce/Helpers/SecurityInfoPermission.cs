using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IntelliTect.Coalesce.Helpers
{
    public class SecurityPermission
    {

        public SecurityPermission()
        {
            HasAttribute = false;
        }

        public SecurityPermission(SecurityPermissionLevels level, string roles, string name)
        {
            HasAttribute = true;
            PermissionLevel = level;
            Roles = PermissionLevel != SecurityPermissionLevels.DenyAll ? roles : string.Empty;
            Name = name;
        }

        public bool HasAttribute { get; } = false;
        public SecurityPermissionLevels PermissionLevel { get; } = SecurityPermissionLevels.AllowAuthorized;
        public string Roles { get; } = "";
        public string Name { get; } = "";
                

        public bool AllowAnonymous => PermissionLevel == SecurityPermissionLevels.AllowAll;
        public bool NoAccess => PermissionLevel == SecurityPermissionLevels.DenyAll;
        public bool HasRoles => RoleList.Any();

        private IReadOnlyList<string> _roleList = null;
        public IReadOnlyList<string> RoleList
        {
            get
            {
                if (_roleList != null) return _roleList;

                var list = new List<string>();
                if (!string.IsNullOrEmpty(Roles))
                {
                    var roles = Roles.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    list.AddUnique(roles.SelectMany(role => RoleMapping.Map(role)));
                    list.AddUnique(roles);
                }
                return _roleList = list.AsReadOnly();
            }
        }

        public string ExternalRoleList => string.Join(",", RoleList);

        public string ToStringWithName() => $"{Name}: {ToString()}";

        public override string ToString()
        {
            if (HasAttribute)
            {
                if (PermissionLevel == SecurityPermissionLevels.AllowAll) return $"Allow All ";
                else if (PermissionLevel == SecurityPermissionLevels.DenyAll) return $"Deny All ";
                else return $"Allow Authorized Roles: {ExternalRoleList} ";
            }
            return $"Allow All Authorized";
        }
    }
}