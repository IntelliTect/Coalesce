using System;
using System.Diagnostics.CodeAnalysis;

namespace IntelliTect.Coalesce
{    
    /// <summary>
    /// Configures Semantic Kernel function generation for entities, data sources, data source parameters, and methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public class KernelPluginAttribute : Attribute
    {
        public KernelPluginAttribute()
        {
        }

        [SetsRequiredMembers]
        public KernelPluginAttribute(string description)
        {
            Description = description;
        }

        /// <summary>
        /// Gets or sets the description of this item for Semantic Kernel.
        /// This description will be used to generate the function's documentation and inform the AI about its purpose.
        /// </summary>
        public required string Description { get; set; }

        #region Entity-specific properties

        /// <summary>
        /// Controls whether the Save operation for this entity is exposed through Semantic Kernel.
        /// Only applicable when applied to entity classes.
        /// Respects all other attribute-based security and Behaviors customization.
        /// </summary>
        public bool SaveEnabled { get; set; }

        ///// <summary>
        ///// Gets or sets the description for the Save operation when exposed to Semantic Kernel.
        ///// Only applicable when applied to entity classes and when SaveEnabled is true.
        ///// If not specified, a default description will be generated.
        ///// </summary>
        //public string? SaveDescription { get; set; }

        /// <summary>
        /// Controls whether the Delete operation for this entity is exposed through Semantic Kernel.
        /// Only applicable when applied to entity classes.
        /// Respects all other attribute-based security and Behaviors customization.
        /// </summary>
        public bool DeleteEnabled { get; set; }

        ///// <summary>
        ///// Gets or sets the description for the Delete operation when exposed to Semantic Kernel.
        ///// Only applicable when applied to entity classes and when DeleteEnabled is true.
        ///// If not specified, a default description will be generated.
        ///// </summary>
        //public string? DeleteDescription { get; set; }

        #endregion
    }
}
