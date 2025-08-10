using System;
using System.ComponentModel.DataAnnotations;

namespace IntelliTect.Coalesce.DataAnnotations;

/// <summary>
/// Controls the permissions and other behaviors for executing a custom method.
/// When multiple roles are specified, they are evaluated with OR logic - the user only needs to be in any one of the specified roles.
/// </summary>
[System.AttributeUsage(System.AttributeTargets.Method)]
public class ExecuteAttribute : SecurityAttribute
{
    public ExecuteAttribute()
    {
    }

    public ExecuteAttribute(SecurityPermissionLevels permissionLevel)
    {
        PermissionLevel = permissionLevel;
    }

    public ExecuteAttribute(params string[] roles)
    {
        Roles = string.Join(",", roles);
    }

    /// <summary>
    /// If true, admin pages will clear the parameter inputs when a successful invocation is performed.
    /// </summary>
    public bool AutoClear { get; set; }

    /// <summary>
    /// If true, validation of <see cref="ValidationAttribute"/> on parameters will be performed by the server.
    /// This setting defaults to the value of <see cref="CoalesceOptions.ValidateAttributesForMethods"/>.
    /// </summary>
    public bool ValidateAttributes
    {
        get => _ValidateAttributes.GetValueOrDefault();
        set => _ValidateAttributes = value;
    }
    public bool ValidateAttributesHasValue => _ValidateAttributes.HasValue;
    private bool? _ValidateAttributes;

    /// <summary>
    /// Sets the HTTP method used for the API endpoint. 
    /// Use of <see cref="HttpMethod.Get"/> is required to use <see cref="VaryByProperty"/>.
    /// </summary>
    public HttpMethod HttpMethod { get; set; }

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

    /// <summary>
    /// Specify that the targeted model instance method should load the instance it is called on
    /// from the specified data source when invoked from an API endpoint.
    /// By default, the model type's default data source is used.
    /// </summary>
    public Type? DataSource { get; set; }

    /// <summary>
    /// <para>
    /// For HTTP GET methods with <see cref="VaryByProperty"/>, specifies the client cache duration in seconds
    /// when the client provides the correct ETag value in the querystring.
    /// </para>
    /// <para>
    /// Defaults to 30 days (2,592,000 seconds) if not specified.
    /// </para>
    /// </summary>
    public int ClientCacheDurationSeconds { get; set; }

    /// <summary>
    /// Gets the ClientCacheDuration as a TimeSpan based on ClientCacheDurationSeconds.
    /// </summary>
    public TimeSpan? ClientCacheDuration => TimeSpan.FromSeconds(ClientCacheDurationSeconds);
}

public enum HttpMethod
{
    Post = 0,
    Get = 1,
    Put = 2,
    Delete = 3,
    Patch = 4
}
