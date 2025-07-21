using System;

namespace IntelliTect.Coalesce.DataAnnotations;

/// <summary>
/// When placed on a method parameter declaration, causes the parameter to be injected from the application's IServiceContainer.
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Parameter)]
public class InjectAttribute : Attribute
{

}
