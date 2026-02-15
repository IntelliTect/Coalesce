using System;

namespace IntelliTect.Coalesce.DataAnnotations;

/// <summary>
/// <para>
/// When placed on an entity or custom <see cref="IClassDto{T}"/> class exposed by Coalesce,
/// controls the permissions for modifying existing instances of the model via the /save or /bulkSave endpoints.
/// </para>
/// <para>
/// When placed on a property exposed by Coalesce, controls the roles that are allowed
/// to send data from the client to the server for that property for any purpose,
/// including the /save and /bulkSave APIs, and method parameters. 
/// </para>
/// <para>When multiple roles are specified, they are evaluated with OR logic - the user only needs to be in any one of the specified roles.</para>
/// </summary>    
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public class EditAttribute : SecurityAttribute
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
