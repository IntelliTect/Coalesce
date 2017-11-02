using IntelliTect.Coalesce.DataAnnotations;
using IntelliTect.Coalesce.TypeDefinition.Wrappers;
using IntelliTect.Coalesce.Utilities;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IntelliTect.Coalesce.Helpers
{
    public class SecurityPermission
    {
        private AttributeData _attributeData;
        private SecurityAttribute _attribute;

        private List<string> _roleList = null;

        public SecurityPermission(AttributeWrapper attribute)
        {
            _attributeData = attribute.AttributeData;
            _attribute = attribute.Attribute;
            if (_attributeData != null)
            {
                HasAttribute = true;
                PermissionLevel = (SecurityPermissionLevels)_attributeData.GetPropertyValue("PermissionLevel", SecurityPermissionLevels.AllowAuthorized);
                Roles = PermissionLevel != SecurityPermissionLevels.DenyAll ? (string)_attributeData.GetPropertyValue("Roles", string.Empty) : string.Empty;
                Name = _attributeData.AttributeClass.Name.Replace("Attribute", string.Empty);
            }
            else if (_attribute != null)
            {
                HasAttribute = true;
                PermissionLevel = _attribute.PermissionLevel;                
                Roles = PermissionLevel != SecurityPermissionLevels.DenyAll ? _attribute.Roles : string.Empty;
                Name = _attribute.GetType().Name.Replace("Attribute", string.Empty);
            }
        }

        public bool HasAttribute { get; private set; } = false;
        public SecurityPermissionLevels PermissionLevel { get; private set; } = SecurityPermissionLevels.AllowAuthorized;
        public string Roles { get; private set; } = "";
        public string Name { get; private set; } = "";
                

        public bool AllowAnonymous { get { return PermissionLevel == SecurityPermissionLevels.AllowAll; } }
        public bool NoAccess { get { return PermissionLevel == SecurityPermissionLevels.DenyAll; } }

        public bool HasRoles { get { return RoleList.Count() > 0; } }

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

        public string ExternalRoleList
        {
            get
            {
                return string.Join(",", RoleList);
            }
        }


        public string ToStringWithName()
        {
            return $"{Name}: {ToString()}";
        }

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