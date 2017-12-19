using IntelliTect.Coalesce.Helpers;
using System;

namespace IntelliTect.Coalesce.DataAnnotations
{
    public abstract class SecurityAttribute : Attribute
    {
        public virtual SecurityPermissionLevels PermissionLevel { get; set; } = SecurityPermissionLevels.AllowAuthorized;

        public virtual string Roles { get; set; } = "";
    }

    public enum SecurityPermissionLevels
    {
        AllowAll = 1,
        AllowAuthorized = 2,
        DenyAll = 3
    }
}
