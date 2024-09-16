namespace Coalesce.Starter.Vue.Data;

/// <summary>
/// The permissions available for assignment to <see cref="Models.Role"/>s. 
/// Permissions generally describe actions that a user can take within the application,
/// while roles are usually representative of a job title or function.
/// </summary>
public enum Permission
{
    // Note: Enum values/numbers are not used. Only the names are used for persistence and API representation.

    [Display(Name = "Admin - General", Description = "Modify application configuration and other administrative functions excluding user/role management.")]
    Admin = 1,

    [Display(Name = "Admin - Users", Description = "Add and modify users accounts and their assigned roles. Edit roles and their permissions.")]
    UserAdmin,

    ViewAuditLogs
}
