using IntelliTect.Coalesce.Helpers;
using System;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// The Class or Property is read/write for the users and groups and not accessible to others.
    /// </summary>    
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class EditAttribute: SecurityAttribute
    {
        public EditAttribute()
        {
        }

        public EditAttribute(SecurityPermissionLevels permissionLevel)
        {
            PermissionLevel = permissionLevel;
        }

        public EditAttribute(params string[] roles)
        {
            Roles = string.Join(",", roles);
        }
    }
}
