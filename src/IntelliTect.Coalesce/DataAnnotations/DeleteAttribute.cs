using IntelliTect.Coalesce.Helpers;
using System;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// Should users be allowed to delete an entity via the API/button.
    /// </summary>    
    [AttributeUsage(AttributeTargets.Class)]
    public class DeleteAttribute : SecurityAttribute
    {
        public DeleteAttribute()
        {
        }

        public DeleteAttribute(SecurityPermissionLevels permissionLevel)
        {
            PermissionLevel = permissionLevel;
        }

        public DeleteAttribute(params string[] roles)
        {
            Roles = string.Join(",", roles);
        }
    }
}
