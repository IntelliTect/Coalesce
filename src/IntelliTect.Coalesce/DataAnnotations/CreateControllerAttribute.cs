using System;

namespace IntelliTect.Coalesce.DataAnnotations;

/// <summary>
/// Allows specifying the types of controllers to create. Not including will create all.
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Class)]
[Obsolete("Use security attributes (Read/Edit/Create/Delete) or InternalUse to hide parts of entities or whole entities from Coalesce APIs")]
public class CreateControllerAttribute : System.Attribute
{
    public bool WillCreateView { get; set; }
    public bool WillCreateApi { get; set; }

    public CreateControllerAttribute(bool willCreateView = true, bool willCreateApi = true)
    {
        WillCreateView = willCreateView;
        WillCreateApi = willCreateApi;
    }
}
