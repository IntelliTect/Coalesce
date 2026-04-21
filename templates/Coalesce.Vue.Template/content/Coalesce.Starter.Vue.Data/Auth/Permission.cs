using System.Text.Json.Serialization;

namespace Coalesce.Starter.Vue.Data;

/// <summary>
/// The permissions available for assignment to <see cref="Models.Role"/>s. 
/// Permissions generally describe actions that a user can take within the application,
/// while roles are usually representative of a job title or function.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Permission
{
    // Note: Permissions are serialized as strings both in the database and over the API.

    [Display(Name = "Admin - General", Description = "Modify application configuration and other administrative functions excluding user/role management.")]
    Admin = 1,

    [Display(Name = "Admin - Users", Description = "Add and modify users accounts and their assigned roles. Edit roles and their permissions.")]
    UserAdmin,
#if AuditLogs
    ViewAuditLogs
#endif
}
