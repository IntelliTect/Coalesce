using IntelliTect.Coalesce.Helpers;
using IntelliTect.Coalesce.TypeDefinition;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IntelliTect.Coalesce.DataAnnotations
{
#pragma warning disable RCS1203 // Use AttributeUsageAttribute. Class is abstract.
    public abstract class SecurityAttribute : Attribute
#pragma warning restore RCS1203 // Use AttributeUsageAttribute.
    {
        public virtual SecurityPermissionLevels PermissionLevel { get; set; } = SecurityPermissionLevels.AllowAuthorized;

        public string Roles { get; set; } = "";
    }

    public static class SecurityAttributeExtensions
    {
        public static SecurityPermission GetSecurityPermission<TAttribute>(this IAttributeProvider parent)
            where TAttribute : SecurityAttribute
        {
            var name = typeof(TAttribute).Name.Replace("Attribute", string.Empty);
            if (!parent.HasAttribute<TAttribute>())
            {
                return new SecurityPermission(name);
            }

            var level = parent.GetAttributeValue<TAttribute, SecurityPermissionLevels>(a => a.PermissionLevel) ?? SecurityPermissionLevels.AllowAuthorized;
            object attributeRoles = parent.GetAttributeValue<TAttribute>(nameof(SecurityAttribute.Roles)) ?? "";

            // This will happen in roslyn-based contexts due to us also accepting string arrays for the roles.
            if (attributeRoles is IEnumerable<object> objects && objects.All(v => v is string))
            {
                attributeRoles = string.Join(",", objects);
            }

            if (!(attributeRoles is string rolesString))
            {
                throw new InvalidCastException("Unknown type of 'roles' on SecurityAttribute");
            }

            return new SecurityPermission(
                level: parent.GetAttributeValue<TAttribute, SecurityPermissionLevels>(a => a.PermissionLevel) ?? SecurityPermissionLevels.AllowAuthorized,
                roles: rolesString,
                name: name
            );
        }
    }

    public enum SecurityPermissionLevels
    {
        AllowAll = 1,
        AllowAuthorized = 2,
        DenyAll = 3
    }
}
