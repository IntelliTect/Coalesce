using IntelliTect.Coalesce.Helpers;
using IntelliTect.Coalesce.TypeDefinition;
using System;

namespace IntelliTect.Coalesce.DataAnnotations
{
    public abstract class SecurityAttribute : Attribute
    {
        public virtual SecurityPermissionLevels PermissionLevel { get; set; } = SecurityPermissionLevels.AllowAuthorized;

        public virtual string Roles { get; set; } = "";
    }

    public static class SecurityAttributeExtensions
    {
        public static SecurityPermission GetSecurityPermission<TAttribute>(this IAttributeProvider parent)
            where TAttribute : SecurityAttribute =>
            !parent.HasAttribute<TAttribute>()
            ? new SecurityPermission()
            : new SecurityPermission(
                level: parent.GetAttributeValue<TAttribute, SecurityPermissionLevels>(a => a.PermissionLevel) ?? SecurityPermissionLevels.AllowAuthorized,
                roles: parent.GetAttributeValue<TAttribute>(a => a.Roles),
                name: typeof(TAttribute).Name.Replace("Attribute", string.Empty)
            );
    }

    public enum SecurityPermissionLevels
    {
        AllowAll = 1,
        AllowAuthorized = 2,
        DenyAll = 3
    }
}
