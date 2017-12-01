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

        public bool HasAttribute { get; private set; } = false;
        public SecurityPermissionLevels PermissionLevel { get; private set; } = SecurityPermissionLevels.AllowAuthorized;
        public string Roles { get; private set; } = "";
        public string Name { get; private set; } = "";
                

        public bool AllowAnonymous => PermissionLevel == SecurityPermissionLevels.AllowAll;
        public bool NoAccess => PermissionLevel == SecurityPermissionLevels.DenyAll;
        public bool HasRoles => RoleList.Any();

        private List<string> _roleList = null;
        public List<string> RoleList
        {
            get
            {
                if (_roleList == null)
                {
                    _roleList = new List<string>();
                    if (!string.IsNullOrEmpty(Roles))
                    {
                        var roles = Roles.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        _roleList.AddUnique(roles.SelectMany(role => RoleMapping.Map(role)));
                        _roleList.AddUnique(roles);
                    }
                }
                return _roleList;
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