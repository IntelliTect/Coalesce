using System;

namespace IntelliTect.Coalesce;

/// <summary>
/// Defines static, assembly-level configuration for the Coalesce models in the targeted assembly.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
public sealed class CoalesceConfigurationAttribute : Attribute
{
    /// <summary>
    /// <para>
    /// If true, models defined in the targeted assembly will not be loaded by Coalesce's 
    /// <see cref="QueryableExtensions.IncludeChildren"/> method, 
    /// nor by the <a href="https://coalesce.intellitect.com/modeling/model-components/data-sources.html#default-loading-behavior">Default Loading Behavior</a>, 
    /// which normally automatically includes the first level of navigation properties of any entity returned from a Coalesce-generated /get, /list, or /save API endpoint.
    /// </para>
    /// <para>
    /// If true, you must always Include the desired navigation properties in a custom data source, or load entites directly from the type's /get or /list endpoints.
    /// </para>
    /// <para>
    /// Use this to prevent accidental unrestricted, unfiltered loading of types that hold sensitive information.</para>
    /// </summary>
    public bool NoAutoInclude { get; set; }
}
