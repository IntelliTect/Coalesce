using System;

namespace IntelliTect.Coalesce;

/// <summary>
/// Configures Semantic Kernel function generation for entities, data sources, data source parameters, and methods.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
public class SemanticKernelAttribute : Attribute
{
    public SemanticKernelAttribute()
    {
    }

    public SemanticKernelAttribute(string description)
    {
        Description = description;
    }

    /// <summary>
    /// Gets or sets the description of this item for Semantic Kernel.
    /// This description will be used to generate the function's documentation and inform the AI about its purpose.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Controls whether the Save operation for this model is exposed through Semantic Kernel.
    /// Only applicable when applied to model classes.
    /// Respects all other attribute-based security and Behaviors customization.
    /// </summary>
    public bool SaveEnabled { get; set; }

    ///// <summary>
    ///// Gets or sets the description for the Save operation when exposed to Semantic Kernel.
    ///// Only applicable when applied to model classes and when SaveEnabled is true.
    ///// If not specified, a default description will be generated.
    ///// </summary>
    //public string? SaveDescription { get; set; }

    /// <summary>
    /// Controls whether the Delete operation for this model is exposed through Semantic Kernel.
    /// Only applicable when applied to model classes.
    /// Respects all other attribute-based security and Behaviors customization.
    /// </summary>
    public bool DeleteEnabled { get; set; }

    ///// <summary>
    ///// Gets or sets the description for the Delete operation when exposed to Semantic Kernel.
    ///// Only applicable when applied to model classes and when DeleteEnabled is true.
    ///// If not specified, a default description will be generated.
    ///// </summary>
    //public string? DeleteDescription { get; set; }

    /// <summary>
    /// Controls whether the Default Data Source for this model is exposed through Semantic Kernel.
    /// To expose specific data sources, annotate each desired individual Data Source class with <see cref="SemanticKernelAttribute" />.
    /// </summary>
    public bool DefaultDataSourceEnabled { get; set; }
}
