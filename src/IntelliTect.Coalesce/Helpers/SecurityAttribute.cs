using System;

namespace IntelliTect.Coalesce.Helpers
{
    public abstract class SecurityAttribute : Attribute
    {
        public virtual SecurityPermissionLevels PermissionLevel { get; set; } = SecurityPermissionLevels.AllowAuthorized;

        public virtual string Roles { get; set; } = "";
    }
}
