using System;

namespace IntelliTect.Coalesce.DataAnnotations;

/// <summary>
/// Controls attributes of the API controller action generated for the target method.
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Method)]
[Obsolete("ControllerActionAttribute has been merged into ExecuteAttribute")]
public class ControllerActionAttribute : System.Attribute
{
    public ControllerActionAttribute() { }

    public ControllerActionAttribute(HttpMethod method)
    {
        Method = method;
    }

    public HttpMethod Method { get; set; }

    /// <summary>
    /// <para>
    /// For HTTP GET model instance methods, specifies the name of a property on the model
    /// that will be used for ETag values and for querystring-based cache variation.
    /// </para>
    /// <para>
    /// The ViewModels on the client will automatically include the property's value
    /// in the querystring of URLs for the method, and the generated controller will 
    /// set a long term Cache-Control value on the response if the client-provided value matches the true value.
    /// </para>
    /// </summary>
    public string? VaryByProperty { get; set; }
}
