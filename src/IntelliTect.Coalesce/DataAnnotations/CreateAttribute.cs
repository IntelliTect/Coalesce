using System;

namespace IntelliTect.Coalesce.DataAnnotations;

/// <summary>
/// <para>
/// When placed on an entity or custom <see cref="IClassDto{T}"/> class exposed by Coalesce,
/// controls the permissions for saving new instances of the model via the /save or /bulkSave endpoints.
/// </para>
/// <para>When multiple roles are specified, they are evaluated with OR logic - the user only needs to be in any one of the specified roles.</para>
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
