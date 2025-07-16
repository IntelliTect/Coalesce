namespace IntelliTect.Coalesce.Analyzers.Test
{
    internal static class TestCode
    {
        public const string TestAttributes = @"
using System;

namespace IntelliTect.Coalesce.DataAnnotations
{
    public enum SecurityPermissionLevels
    {
        AllowAll = 1,
        AllowAuthenticated = 2,
        DenyAll = 3,
    }

    public abstract class SecurityAttribute : Attribute
    {
        public virtual SecurityPermissionLevels PermissionLevel { get; set; } = SecurityPermissionLevels.AllowAuthenticated;
        public string Roles { get; set; } = """";
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class ReadAttribute : SecurityAttribute
    {
        public ReadAttribute() { }
        public ReadAttribute(SecurityPermissionLevels permissionLevel) { PermissionLevel = permissionLevel; }
        public ReadAttribute(params string[] roles) { Roles = string.Join("","", roles); }
        public bool NoAutoInclude { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public class EditAttribute : SecurityAttribute
    {
        public EditAttribute() { }
        public EditAttribute(SecurityPermissionLevels permissionLevel) { PermissionLevel = permissionLevel; }
        public EditAttribute(params string[] roles) { Roles = string.Join("","", roles); }
    }
}
";
    }
}