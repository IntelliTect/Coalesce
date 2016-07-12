using System;

namespace Intellitect.ComponentModel.DataAnnotations
{
    /// <summary>
    /// Specify that this property is only used on specific views of the content.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IncludesAttribute : Attribute
    {
        /// <summary>
        /// Comma-delimited list of content views this property should be included on.
        /// </summary>
        public string ContentViews { get; set; } = "";
    }
}
