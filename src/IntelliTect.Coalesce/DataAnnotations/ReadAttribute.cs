using IntelliTect.Coalesce.Helpers;
using System;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// The Class or Property is read only for the users and groups and not accessible to others.
    /// If no roles are specified, the target is readable by anyone.
    /// If specified on a property with no [Edit] attribute, the property is read-only.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class ReadAttribute : SecurityAttribute
    {
        public ReadAttribute()
        {
        }

        public ReadAttribute(SecurityPermissionLevels permissionLevel)
        {
            PermissionLevel = permissionLevel;
        }

        public ReadAttribute(params string[] roles)
        {
            Roles = string.Join(",", roles);
        }
    }
}
