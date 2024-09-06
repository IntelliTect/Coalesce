namespace Coalesce.Starter.Vue.Data;

/// <summary>
/// The permissions available for assignment to <see cref="Models.Role"/>s. 
/// Permissions generally describe actions that a user can take within the application,
/// while roles are usually representative of a job title or function.
/// </summary>
public enum Permission
{
    // Note about usage of Permission values:
    // The numeric values are stored in the database on `Role.Permissions`.
    // The string value are stored in the user claims,
    // issused by `ClaimsPrincipalFactory` and consumed by role-based security attributes with `nameof`.

    // Therefore, use caution and avoid editing existing roles.
    // Always assign new roles the next highest value, never reusing old numbers.

    [Display(Name = "Admin - General", Description = "Modify application configuration and other administrative functions excluding user/role management.")]
    Admin = 1,

    [Display(Name = "Admin - Users", Description = "Add and modify users accounts and their assigned roles. Edit roles and their permissions.")]
    UserAdmin = 2,

    ViewAuditLogs = 3
}
