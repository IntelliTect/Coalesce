using System;

namespace IntelliTect.Coalesce.DataAnnotations
{
    /// <summary>
    /// Specify that this property is only used on specific views of the content.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class DtoExcludesAttribute : Attribute
    {
        /// <summary>
        /// Comma-delimited list of content views this property should be excluded from.
        /// </summary>
        public string ContentViews { get; set; } = "";
    }
}
