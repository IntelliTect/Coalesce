using System;
using IntelliTect.Coalesce.Helpers;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// Should users be allowed to create an entity via the API/button.
    /// </summary>    
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CreateAttribute : SecurityAttribute
    {
        public CreateAttribute()
        {
        }

        public CreateAttribute(SecurityPermissionLevels permissionLevel)
        {
            PermissionLevel = permissionLevel;
        }

        public CreateAttribute(params string[] roles)
        {
            Roles = string.Join(",", roles);
        }
    }
}
